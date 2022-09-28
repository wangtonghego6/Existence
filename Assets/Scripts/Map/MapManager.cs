using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
using UnityEngine.AI;
using System;

public class MapManager : SingletonMono<MapManager>
{
    #region ����ʱ�ı���
    [SerializeField] MeshCollider meshCollider;
    private Vector3 lastViewerPos = Vector3.one * -1;
    private Dictionary<Vector2Int, MapChunkController> mapChunkDic;  // ȫ�����еĵ�ͼ��
    private MapGenerator mapGenerator;
    private Transform viewer;        // �۲���
    private float updateChunkTime = 1f;
    private bool canUpdateChunk = true;
    private float mapSizeOnWorld;    // ��������ʵ�ʵĵ�ͼ����ߴ�
    private float chunkSizeOnWorld;  // ��������ʵ�ʵĵ�ͼ��ߴ� ��λ��
    private List<MapChunkController> lastVisibleChunkList = new List<MapChunkController>();
    #endregion

    #region ����
    private MapConfig mapConfig;   //��ͼ����
    public MapConfig MapConfig { get => mapConfig; }   //��ͼ����
    private Dictionary<MapVertexType, List<int>> spawnMapObjectConfigDic;// ĳ�����Ϳ���������Щ��ͼ�������õ�ID
    private Dictionary<MapVertexType, List<int>> spawnAIConfigDic;// ĳ�����Ϳ���������ЩAI���õ�ID
    #endregion

    #region �浵
    private MapInitData mapInitData;
    private MapData mapData;
    #endregion

    #region �������
    [SerializeField] NavMeshSurface navMeshSurface;
    /// <summary>
    /// �決��������
    /// </summary>
    public void BakeNavMesh()
    {
        navMeshSurface.BuildNavMesh();
    }
    #endregion

    public void Init()
    {
        StartCoroutine(DoInit());
    }
    private IEnumerator DoInit()
    {
        // ȷ���浵
        mapInitData = ArchiveManager.Instance.MapInitData;
        mapData = ArchiveManager.Instance.MapData;

        // ȷ������
        mapConfig = ConfigManager.Instance.GetConfig<MapConfig>(ConfigName.Map);
        Dictionary<int, ConfigBase> tempDic = ConfigManager.Instance.GetConfigs(ConfigName.MapObject);
        spawnMapObjectConfigDic = new Dictionary<MapVertexType, List<int>>();
        spawnMapObjectConfigDic.Add(MapVertexType.Forest, new List<int>());
        spawnMapObjectConfigDic.Add(MapVertexType.Marsh, new List<int>());
        foreach (var item in tempDic)
        {
            MapVertexType mapVertexType = (item.Value as MapObjectConfig).MapVertexType;
            if (mapVertexType == MapVertexType.None) continue;
            spawnMapObjectConfigDic[mapVertexType].Add(item.Key);
        }

        tempDic = ConfigManager.Instance.GetConfigs(ConfigName.AI);
        spawnAIConfigDic = new Dictionary<MapVertexType, List<int>>();
        spawnAIConfigDic.Add(MapVertexType.Forest, new List<int>());
        spawnAIConfigDic.Add(MapVertexType.Marsh, new List<int>());
        foreach (var item in tempDic)
        {
            MapVertexType mapVertexType = (item.Value as AIConfig).MapVertexType;
            if (mapVertexType == MapVertexType.None) continue;
            spawnAIConfigDic[mapVertexType].Add(item.Key);
        }

        // ��ʼ����ͼ������
        mapGenerator = new MapGenerator(mapConfig, mapInitData, mapData,spawnMapObjectConfigDic, spawnAIConfigDic) ;
        mapGenerator.GenerateMapData();
        mapChunkDic = new Dictionary<Vector2Int, MapChunkController>();
        chunkSizeOnWorld = mapConfig.mapChunkSize * mapConfig.cellSize;
        mapSizeOnWorld = chunkSizeOnWorld * mapInitData.mapSize;
        // ���ɵ�����ײ�������
        meshCollider.sharedMesh = GenerateGroundMesh(mapSizeOnWorld, mapSizeOnWorld);
        // �決��������
        BakeNavMesh();
        int mapChunkCount = mapData.MapChunkIndexList.Count;
        // �ϴ浵
        if (mapChunkCount>0)
        {
            GameSceneManager.Instance.SetProgressMapChunkCount(mapChunkCount);
            // ���ݴ浵ȥ�ָ�������ͼ״̬
            for (int i = 0; i < mapData.MapChunkIndexList.Count; i++)
            {
                Serialization_Vector2 chunkIndex = mapData.MapChunkIndexList[i];
                MapChunkData chunkData = ArchiveManager.Instance.GetMapChunkData(chunkIndex);
                GenerateMapChunk(chunkIndex.ConverToSVector2Init(), chunkData).gameObject.SetActive(false);
                for (int f = 0; f < 5; f++) yield return null;
            }
        }
        // �´浵
        else
        {
            GameSceneManager.Instance.SetProgressMapChunkCount(GetMapChunkCountOnGameInit());
            // ��ǰ�۲������ڵĵ�ͼ��
            Vector2Int currChunkIndex = GetMapChunkIndexByWorldPosition(viewer.position);
            int startX = currChunkIndex.x - mapConfig.viewDinstance;
            int startY = currChunkIndex.y - mapConfig.viewDinstance;
            // ������Ҫ��ʾ�ĵ�ͼ��
            for (int x = 0; x < 2 * mapConfig.viewDinstance + 1; x++)
            {
                for (int y = 0; y < 2 * mapConfig.viewDinstance + 1; y++)
                {
                    Vector2Int chunkIndex = new Vector2Int(startX + x, startY + y);
                    GenerateMapChunk(chunkIndex, null);
                    for (int f = 0; f < 5; f++) yield return null;
                }
            }
        }
        DoUpdateVisibleChunk();
        // ��ʾһ��MapUI,���ó�ʼ�����ٹرյ�
        ShowMapUI(); 
        CloseMapUI();
    }

    public void UpdateViewer(Transform player)
    {
        this.viewer = player;
    }

    void Update()
    {
        if (GameSceneManager.Instance.IsInitialized == false) return;
        UpdateVisibleChunk();

        if (Input.GetKeyDown(KeyCode.M))
        {
            if (isShowMaping) CloseMapUI();
            else ShowMapUI();
            isShowMaping = !isShowMaping;
        }

        if (isShowMaping)
        {
            UpdateMapUI();
        }
    }

    /// <summary>
    /// ���ɵ���Mesh
    /// </summary>
    private Mesh GenerateGroundMesh(float height, float wdith)
    {
        Mesh mesh = new Mesh();
        // ȷ������������
        mesh.vertices = new Vector3[]
        {
            new Vector3(0,0,0),
            new Vector3(0,0,height),
            new Vector3(wdith,0,height),
            new Vector3(wdith,0,0),
        };
        // ȷ����Щ���γ�������
        mesh.triangles = new int[]
        {
            0,1,2,
            0,2,3
        };
        mesh.uv = new Vector2[]
        {
            new Vector3(0,0),
            new Vector3(0,1),
            new Vector3(1,1),
            new Vector3(1,0),
        };
        return mesh;
    }

    #region ��ͼ�����
    // ���ݹ۲��ߵ�λ����ˢ����Щ��ͼ����Կ���
    private void UpdateVisibleChunk()
    {
        // ����۲���û���ƶ���������Ҫˢ��
        if (viewer.position == lastViewerPos) return;
        lastViewerPos = viewer.position;
        // ���µ�ͼUI������
        if (isShowMaping) mapUI.UpdatePivot(viewer.position);

        // ���ʱ��û�� ���������
        if (canUpdateChunk == false) return;

        DoUpdateVisibleChunk();
    }

    private void DoUpdateVisibleChunk()
    {
        // ��ǰ�۲������ڵĵ�ͼ��
        Vector2Int currChunkIndex = GetMapChunkIndexByWorldPosition(viewer.position);

        // �ر�ȫ������Ҫ��ʾ�ĵ�ͼ��
        for (int i = lastVisibleChunkList.Count - 1; i >= 0; i--)
        {
            Vector2Int chunkIndex = lastVisibleChunkList[i].ChunkIndex;
            if (Mathf.Abs(chunkIndex.x - currChunkIndex.x) > mapConfig.viewDinstance
                || Mathf.Abs(chunkIndex.y - currChunkIndex.y) > mapConfig.viewDinstance)
            {
                lastVisibleChunkList[i].SetActive(false);
                lastVisibleChunkList.RemoveAt(i);
            }
        }

        int startX = currChunkIndex.x - mapConfig.viewDinstance;
        int startY = currChunkIndex.y - mapConfig.viewDinstance;
        // ������Ҫ��ʾ�ĵ�ͼ��
        for (int x = 0; x < 2 * mapConfig.viewDinstance + 1; x++)
        {
            for (int y = 0; y < 2 * mapConfig.viewDinstance + 1; y++)
            {

                Vector2Int chunkIndex = new Vector2Int(startX + x, startY + y);
                // �ڵ�ͼ�ֵ��У�Ҳ����֮ǰ���ع������ǲ�һ����������ˣ���Ϊ��ͼ����Э����ִ�У�ִ����ɺ�����ʼ�����
                if (mapChunkDic.TryGetValue(chunkIndex, out MapChunkController chunk))
                {
                    // ��һ����ʾ�ĵ�ͼ�б��в������������ͼ�� && ͬʱ���Ѿ�����˳�ʼ��
                    if (lastVisibleChunkList.Contains(chunk) == false && chunk.IsInitialized)
                    {
                        lastVisibleChunkList.Add(chunk);
                        chunk.SetActive(true);
                    }
                }
                // ֮ǰû�м���
                else
                {
                    chunk = GenerateMapChunk(chunkIndex, null);
                }
            }
        }
        canUpdateChunk = false;
        Invoke(nameof(RestCanUpdateChunkFlag), updateChunkTime);
    }

    /// <summary>
    /// �������������ȡ��ͼ�������
    /// </summary>
    private Vector2Int GetMapChunkIndexByWorldPosition(Vector3 worldPostion)
    {
        int x = Mathf.Clamp(Mathf.FloorToInt(worldPostion.x / chunkSizeOnWorld), 1, mapInitData.mapSize);
        int y = Mathf.Clamp(Mathf.FloorToInt(worldPostion.z / chunkSizeOnWorld), 1, mapInitData.mapSize);
        return new Vector2Int(x, y);
    }

    private int GetMapChunkCountOnGameInit()
    {
        // ��ǰ�۲������ڵĵ�ͼ��
        int res = 0;
        Vector2Int currChunkIndex = GetMapChunkIndexByWorldPosition(viewer.position);
        int startX = currChunkIndex.x - mapConfig.viewDinstance;
        int startY = currChunkIndex.y - mapConfig.viewDinstance;

        for (int x = 0; x < 2 * mapConfig.viewDinstance + 1; x++)
        {
            for (int y = 0; y < 2 * mapConfig.viewDinstance + 1; y++)
            {
                int indexX = startX + x;
                int indexY = startY + y;
                // �������ĺϷ���
                if (indexX > mapInitData.mapSize - 1 || indexY > mapInitData.mapSize - 1) continue;
                if (indexX < 0 || indexY < 0) continue;
                res+=1;
            }
        }
        return res;
    }

    /// <summary>
    /// �������������ȡ��ͼ��
    /// </summary>
    public MapChunkController GetMapChunkByWorldPosition(Vector3 worldPostion)
    {
        return mapChunkDic[GetMapChunkIndexByWorldPosition(worldPostion)];
    }


    /// <summary>
    /// ���ɵ�ͼ��
    /// </summary>
    private MapChunkController GenerateMapChunk(Vector2Int index, MapChunkData mapChunkData = null)
    {
        // �������ĺϷ���
        if (index.x > mapInitData.mapSize - 1 || index.y > mapInitData.mapSize - 1) return null;
        if (index.x < 0 || index.y < 0) return null;
        MapChunkController chunk = mapGenerator.GenerateMapChunk(index, transform, mapChunkData, () => mapUIUpdateChunkIndexList.Add(index));
        mapChunkDic.Add(index, chunk);
        return chunk;
    }

    private void RestCanUpdateChunkFlag()
    {
        canUpdateChunk = true;
    }

    /// <summary>
    /// Ϊ��ͼ��ˢ�£����ɵ�ͼ�����б�
    /// ����Ϊnull
    /// ���һ����ͼ������������һ�����������ͼ����������������
    /// </summary>
    public List<MapObjectData> SpawnMapObjectDataOnMapChunkRefresh(Vector2Int chunkIndex)
    {
        return mapGenerator.GenerateMapObjectDataListOnMapChunkRefresh(chunkIndex);
    }

    #endregion

    #region ��ͼUI���
    private bool mapUIInitialized = false;
    private bool isShowMaping = false;
    private List<Vector2Int> mapUIUpdateChunkIndexList = new List<Vector2Int>();    // �����µ��б�
    private UI_MapWindow mapUI;

    // ��ʾ��ͼUI
    private void ShowMapUI()
    {
        mapUI = UIManager.Instance.Show<UI_MapWindow>();
        if (!mapUIInitialized)
        {
            mapUI.InitMap(mapInitData.mapSize, mapConfig.mapChunkSize, mapSizeOnWorld, mapConfig.forestTexutre);
            mapUIInitialized = true;
        }
        // ����
        UpdateMapUI();
    }
    private void UpdateMapUI()
    {
        for (int i = 0; i < mapUIUpdateChunkIndexList.Count; i++)
        {
            Vector2Int chunkIndex = mapUIUpdateChunkIndexList[i];
            Texture2D texture = null;
            MapChunkController mapchunk = mapChunkDic[chunkIndex];
            if (mapchunk.IsAllForest == false)
            {
                texture = (Texture2D)mapchunk.GetComponent<MeshRenderer>().material.mainTexture;
            }
            mapUI.AddMapChunk(chunkIndex, mapchunk.MapChunkData.MapObjectDic, texture);
        }
        mapUIUpdateChunkIndexList.Clear();
        // Content������
        mapUI.UpdatePivot(viewer.position);
    }
    // �رյ�ͼUI
    private void CloseMapUI()
    {
        UIManager.Instance.Close<UI_MapWindow>();
    }


    #endregion

    #region ��ͼ�������
    /// <summary>
    /// �Ƴ�һ����ͼ����
    /// </summary>
    /// <param name="id"></param>
    public void RemoveMapObject(ulong id)
    {
        // ����Icon
        if (mapUI != null) mapUI.RemoveMapObjectIcon(id);
    }

    /// <summary>
    /// ����һ����ͼ����
    /// </summary>
    public void SpawnMapObject(int mapObjectConfigID, Vector3 pos, bool isFormBuild)
    {
        Vector2Int chunkIndex = GetMapChunkIndexByWorldPosition(pos);
        SpawnMapObject(mapChunkDic[chunkIndex], mapObjectConfigID, pos, isFormBuild);
    }


    /// <summary>
    /// ����һ����ͼ����
    /// </summary>
    public void SpawnMapObject(MapChunkController mapChunkController, int mapObjectConfigID, Vector3 pos, bool isFormBuild)
    {
        // ��������
        MapObjectData mapObjectData = mapGenerator.GenerateMapObjectData(mapObjectConfigID, pos);
        if (mapObjectData == null) return;

        // ������ͼ��
        mapChunkController.AddMapObject(mapObjectData, isFormBuild);

        // ����Icon
        if (mapUI != null) mapUI.AddMapObjectIcon(mapObjectData);
    }

    #endregion

    private void OnDestroy()
    {
        ArchiveManager.Instance.SaveMapData();
    }
}
