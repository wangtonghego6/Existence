using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
using UnityEngine.AI;
using System;

public class MapManager : SingletonMono<MapManager>
{
    #region 运行时的变量
    [SerializeField] MeshCollider meshCollider;
    private Vector3 lastViewerPos = Vector3.one * -1;
    private Dictionary<Vector2Int, MapChunkController> mapChunkDic;  // 全部已有的地图块
    private MapGenerator mapGenerator;
    private Transform viewer;        // 观察者
    private float updateChunkTime = 1f;
    private bool canUpdateChunk = true;
    private float mapSizeOnWorld;    // 在世界中实际的地图整体尺寸
    private float chunkSizeOnWorld;  // 在世界中实际的地图块尺寸 单位米
    private List<MapChunkController> lastVisibleChunkList = new List<MapChunkController>();
    #endregion

    #region 配置
    private MapConfig mapConfig;   //地图配置
    public MapConfig MapConfig { get => mapConfig; }   //地图配置
    private Dictionary<MapVertexType, List<int>> spawnMapObjectConfigDic;// 某个类型可以生成那些地图对象配置的ID
    private Dictionary<MapVertexType, List<int>> spawnAIConfigDic;// 某个类型可以生成那些AI配置的ID
    #endregion

    #region 存档
    private MapInitData mapInitData;
    private MapData mapData;
    #endregion

    #region 导航相关
    [SerializeField] NavMeshSurface navMeshSurface;
    /// <summary>
    /// 烘焙导航网格
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
        // 确定存档
        mapInitData = ArchiveManager.Instance.MapInitData;
        mapData = ArchiveManager.Instance.MapData;

        // 确定配置
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

        // 初始化地图生成器
        mapGenerator = new MapGenerator(mapConfig, mapInitData, mapData,spawnMapObjectConfigDic, spawnAIConfigDic) ;
        mapGenerator.GenerateMapData();
        mapChunkDic = new Dictionary<Vector2Int, MapChunkController>();
        chunkSizeOnWorld = mapConfig.mapChunkSize * mapConfig.cellSize;
        mapSizeOnWorld = chunkSizeOnWorld * mapInitData.mapSize;
        // 生成地面碰撞体的网格
        meshCollider.sharedMesh = GenerateGroundMesh(mapSizeOnWorld, mapSizeOnWorld);
        // 烘焙导航网格
        BakeNavMesh();
        int mapChunkCount = mapData.MapChunkIndexList.Count;
        // 老存档
        if (mapChunkCount>0)
        {
            GameSceneManager.Instance.SetProgressMapChunkCount(mapChunkCount);
            // 根据存档去恢复整个地图状态
            for (int i = 0; i < mapData.MapChunkIndexList.Count; i++)
            {
                Serialization_Vector2 chunkIndex = mapData.MapChunkIndexList[i];
                MapChunkData chunkData = ArchiveManager.Instance.GetMapChunkData(chunkIndex);
                GenerateMapChunk(chunkIndex.ConverToSVector2Init(), chunkData).gameObject.SetActive(false);
                for (int f = 0; f < 5; f++) yield return null;
            }
        }
        // 新存档
        else
        {
            GameSceneManager.Instance.SetProgressMapChunkCount(GetMapChunkCountOnGameInit());
            // 当前观察者所在的地图块
            Vector2Int currChunkIndex = GetMapChunkIndexByWorldPosition(viewer.position);
            int startX = currChunkIndex.x - mapConfig.viewDinstance;
            int startY = currChunkIndex.y - mapConfig.viewDinstance;
            // 开启需要显示的地图块
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
        // 显示一次MapUI,做好初始化后再关闭掉
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
    /// 生成地面Mesh
    /// </summary>
    private Mesh GenerateGroundMesh(float height, float wdith)
    {
        Mesh mesh = new Mesh();
        // 确定顶点在哪里
        mesh.vertices = new Vector3[]
        {
            new Vector3(0,0,0),
            new Vector3(0,0,height),
            new Vector3(wdith,0,height),
            new Vector3(wdith,0,0),
        };
        // 确定哪些点形成三角形
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

    #region 地图块相关
    // 根据观察者的位置来刷新那些地图块可以看到
    private void UpdateVisibleChunk()
    {
        // 如果观察者没有移动过，不需要刷新
        if (viewer.position == lastViewerPos) return;
        lastViewerPos = viewer.position;
        // 更新地图UI的坐标
        if (isShowMaping) mapUI.UpdatePivot(viewer.position);

        // 如果时间没到 不允许更新
        if (canUpdateChunk == false) return;

        DoUpdateVisibleChunk();
    }

    private void DoUpdateVisibleChunk()
    {
        // 当前观察者所在的地图块
        Vector2Int currChunkIndex = GetMapChunkIndexByWorldPosition(viewer.position);

        // 关闭全部不需要显示的地图块
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
        // 开启需要显示的地图块
        for (int x = 0; x < 2 * mapConfig.viewDinstance + 1; x++)
        {
            for (int y = 0; y < 2 * mapConfig.viewDinstance + 1; y++)
            {

                Vector2Int chunkIndex = new Vector2Int(startX + x, startY + y);
                // 在地图字典中，也就是之前加载过，但是不一定加载完成了，因为贴图会在协程中执行，执行完成后才算初始化完毕
                if (mapChunkDic.TryGetValue(chunkIndex, out MapChunkController chunk))
                {
                    // 上一次显示的地图列表中并不包含这个地图块 && 同时它已经完成了初始化
                    if (lastVisibleChunkList.Contains(chunk) == false && chunk.IsInitialized)
                    {
                        lastVisibleChunkList.Add(chunk);
                        chunk.SetActive(true);
                    }
                }
                // 之前没有加载
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
    /// 根据世界坐标获取地图块的索引
    /// </summary>
    private Vector2Int GetMapChunkIndexByWorldPosition(Vector3 worldPostion)
    {
        int x = Mathf.Clamp(Mathf.FloorToInt(worldPostion.x / chunkSizeOnWorld), 1, mapInitData.mapSize);
        int y = Mathf.Clamp(Mathf.FloorToInt(worldPostion.z / chunkSizeOnWorld), 1, mapInitData.mapSize);
        return new Vector2Int(x, y);
    }

    private int GetMapChunkCountOnGameInit()
    {
        // 当前观察者所在的地图块
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
                // 检查坐标的合法性
                if (indexX > mapInitData.mapSize - 1 || indexY > mapInitData.mapSize - 1) continue;
                if (indexX < 0 || indexY < 0) continue;
                res+=1;
            }
        }
        return res;
    }

    /// <summary>
    /// 根据世界坐标获取地图块
    /// </summary>
    public MapChunkController GetMapChunkByWorldPosition(Vector3 worldPostion)
    {
        return mapChunkDic[GetMapChunkIndexByWorldPosition(worldPostion)];
    }


    /// <summary>
    /// 生成地图块
    /// </summary>
    private MapChunkController GenerateMapChunk(Vector2Int index, MapChunkData mapChunkData = null)
    {
        // 检查坐标的合法性
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
    /// 为地图块刷新，生成地图对象列表
    /// 可能为null
    /// 你给一个地图块索引，返回一个今天这个地图块多出来的物体数据
    /// </summary>
    public List<MapObjectData> SpawnMapObjectDataOnMapChunkRefresh(Vector2Int chunkIndex)
    {
        return mapGenerator.GenerateMapObjectDataListOnMapChunkRefresh(chunkIndex);
    }

    #endregion

    #region 地图UI相关
    private bool mapUIInitialized = false;
    private bool isShowMaping = false;
    private List<Vector2Int> mapUIUpdateChunkIndexList = new List<Vector2Int>();    // 待更新的列表
    private UI_MapWindow mapUI;

    // 显示地图UI
    private void ShowMapUI()
    {
        mapUI = UIManager.Instance.Show<UI_MapWindow>();
        if (!mapUIInitialized)
        {
            mapUI.InitMap(mapInitData.mapSize, mapConfig.mapChunkSize, mapSizeOnWorld, mapConfig.forestTexutre);
            mapUIInitialized = true;
        }
        // 更新
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
        // Content的坐标
        mapUI.UpdatePivot(viewer.position);
    }
    // 关闭地图UI
    private void CloseMapUI()
    {
        UIManager.Instance.Close<UI_MapWindow>();
    }


    #endregion

    #region 地图对象相关
    /// <summary>
    /// 移除一个地图对象
    /// </summary>
    /// <param name="id"></param>
    public void RemoveMapObject(ulong id)
    {
        // 处理Icon
        if (mapUI != null) mapUI.RemoveMapObjectIcon(id);
    }

    /// <summary>
    /// 生成一个地图对象
    /// </summary>
    public void SpawnMapObject(int mapObjectConfigID, Vector3 pos, bool isFormBuild)
    {
        Vector2Int chunkIndex = GetMapChunkIndexByWorldPosition(pos);
        SpawnMapObject(mapChunkDic[chunkIndex], mapObjectConfigID, pos, isFormBuild);
    }


    /// <summary>
    /// 生成一个地图对象
    /// </summary>
    public void SpawnMapObject(MapChunkController mapChunkController, int mapObjectConfigID, Vector3 pos, bool isFormBuild)
    {
        // 生成数据
        MapObjectData mapObjectData = mapGenerator.GenerateMapObjectData(mapObjectConfigID, pos);
        if (mapObjectData == null) return;

        // 交给地图块
        mapChunkController.AddMapObject(mapObjectData, isFormBuild);

        // 处理Icon
        if (mapUI != null) mapUI.AddMapObjectIcon(mapObjectData);
    }

    #endregion

    private void OnDestroy()
    {
        ArchiveManager.Instance.SaveMapData();
    }
}
