using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using JKFrame;
using System;
using Random = UnityEngine.Random;
using System.Threading;
/// <summary>
/// ��ͼ���ɹ���
/// </summary>
public class MapGenerator
{
    #region ����ʱ���߼�
    private MapGrid mapGrid;        // ��ͼ�߼����񡢶�������
    private Material marshMaterial;
    private Mesh chunkMesh;
    private int mapObjectForestWeightTotal;
    private int mapObjectMarshWeightTotal;
    private int AIForestWeightTotal;
    private int AIMarshWeightTotal;
    #endregion

    #region ����
    private Dictionary<MapVertexType, List<int>> mapObjectConfigDic;
    private Dictionary<MapVertexType, List<int>> AIConfigDic;
    private MapConfig mapConfig;
    #endregion

    #region �浵
    private MapInitData mapInitData;
    private MapData mapData;
    #endregion

    public MapGenerator(MapConfig mapConfig,MapInitData mapInitData, MapData mapData, Dictionary<MapVertexType, List<int>> mapObjectConfigDic, Dictionary<MapVertexType, List<int>> AIConfigDic)
    {
        this.mapConfig = mapConfig;
        this.mapInitData = mapInitData;
        this.mapData = mapData;
        this.mapObjectConfigDic = mapObjectConfigDic;
        this.AIConfigDic = AIConfigDic;
    }


    /// <summary>
    /// ���ɵ�ͼ���ݣ���Ҫ�����е�ͼ�鶼ͨ�õ�����
    /// </summary>
    public void GenerateMapData()
    {
        // ��������ͼ
        // Ӧ�õ�ͼ����
        Random.InitState(mapInitData.mapSeed);
        float[,] noiseMap = GenerateNoiseMap(mapInitData.mapSize * mapConfig.mapChunkSize, mapInitData.mapSize * mapConfig.mapChunkSize, mapConfig.noiseLacunarity);
        // ������������
        mapGrid = new MapGrid(mapInitData.mapSize * mapConfig.mapChunkSize, mapInitData.mapSize * mapConfig.mapChunkSize, mapConfig.cellSize);
        // ȷ������ ���ӵ���ͼ����
        mapGrid.CalculateMapVertexType(noiseMap, mapInitData.marshLimit);
        // ��ʼ��Ĭ�ϲ��ʵĳߴ�
        mapConfig.mapMaterial.mainTexture = mapConfig.forestTexutre;
        mapConfig.mapMaterial.SetTextureScale("_MainTex", new Vector2(mapConfig.cellSize * mapConfig.mapChunkSize, mapConfig.cellSize * mapConfig.mapChunkSize));
        // ʵ����һ���������
        marshMaterial = new Material(mapConfig.mapMaterial);
        marshMaterial.SetTextureScale("_MainTex", Vector2.one);

        chunkMesh = GenerateMapMesh(mapConfig.mapChunkSize, mapConfig.mapChunkSize, mapConfig.cellSize);
        // ʹ�������������������
        Random.InitState(mapInitData.spawnSeed);

        List<int> temps = mapObjectConfigDic[MapVertexType.Forest];
        for (int i = 0; i < temps.Count; i++) mapObjectForestWeightTotal += ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.MapObject, temps[i]).Probability;
        temps = mapObjectConfigDic[MapVertexType.Marsh];
        for (int i = 0; i < temps.Count; i++) mapObjectMarshWeightTotal += ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.MapObject, temps[i]).Probability;

        temps = AIConfigDic[MapVertexType.Forest];
        for (int i = 0; i < temps.Count; i++) 
            AIForestWeightTotal += ConfigManager.Instance.GetConfig<AIConfig>(ConfigName.AI, temps[i]).Probability;
        temps = AIConfigDic[MapVertexType.Marsh];
        for (int i = 0; i < temps.Count; i++) 
            AIMarshWeightTotal += ConfigManager.Instance.GetConfig<AIConfig>(ConfigName.AI, temps[i]).Probability;

    }

    #region ��ͼ��
    /// <summary>
    /// ���ɵ�ͼ��
    /// </summary>
    public MapChunkController GenerateMapChunk(Vector2Int chunkIndex, Transform parent, MapChunkData mapChunkData, Action callBackForMapTexture)
    {
        // ���ɵ�ͼ������
        GameObject mapChunkObj = new GameObject("Chunk_" + chunkIndex.ToString());
        MapChunkController mapChunk = mapChunkObj.AddComponent<MapChunkController>();
        // ����Mesh
        mapChunkObj.AddComponent<MeshFilter>().mesh = chunkMesh;

        bool allForest;
        // ���ɵ�ͼ�����ͼ
        Texture2D mapTexture;
        this.StartCoroutine
        (
            GenerateMapTexture(chunkIndex, (tex, isAllForest) => {
                allForest = isAllForest;
                 // �����ȫ��ɭ�֣�û��Ҫ��ʵ����һ��������
                if (isAllForest)
                {
                    mapChunkObj.AddComponent<MeshRenderer>().sharedMaterial = mapConfig.mapMaterial;
                }
                else
                {
                    mapTexture = tex;
                    Material material = new Material(marshMaterial);
                    material.mainTexture = tex;
                    mapChunkObj.AddComponent<MeshRenderer>().material = material;
                }
                callBackForMapTexture?.Invoke();

                 // ȷ������
                Vector3 position = new Vector3(chunkIndex.x * mapConfig.mapChunkSize * mapConfig.cellSize, 0, chunkIndex.y * mapConfig.mapChunkSize * mapConfig.cellSize);
                mapChunk.transform.position = position;
                mapChunkObj.transform.SetParent(parent);

                 // ���û��ָ����ͼ�����ݣ�˵�����½��ģ���Ҫ����Ĭ������
                if (mapChunkData == null)
                {
                     // ���ɳ�����������
                    mapChunkData = GenerateMapChunkData(chunkIndex);
                     // ���ɺ���г־û�����
                    ArchiveManager.Instance.AddAndSaveMapChunkData(chunkIndex, mapChunkData);
                }
                else
                {
                    // �ָ�VertextList
                    RecoverMapChunkData(chunkIndex, mapChunkData);
                }
                mapChunk.Init(chunkIndex, position + new Vector3((mapConfig.mapChunkSize * mapConfig.cellSize) / 2, 0, (mapConfig.mapChunkSize * mapConfig.cellSize) / 2), allForest, mapChunkData);

                // ���Ŀǰ��Ϸû����ɳ�ʼ����Ҫ����GameSceneManager���½���
                if (GameSceneManager.Instance.IsInitialized == false)
                {
                    GameSceneManager.Instance.OnGenerateMapChunkSucceed();
                }
            }));

        return mapChunk;
    }

    /// <summary>
    /// ���ɵ���Mesh
    /// </summary>
    private Mesh GenerateMapMesh(int height, int wdith, float cellSize)
    {
        Mesh mesh = new Mesh();
        // ȷ������������
        mesh.vertices = new Vector3[]
        {
            new Vector3(0,0,0),
            new Vector3(0,0,height*cellSize),
            new Vector3(wdith*cellSize,0,height*cellSize),
            new Vector3(wdith*cellSize,0,0),
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
        // ���㷨��
        mesh.RecalculateNormals();
        return mesh;
    }

    /// <summary>
    /// ��������ͼ
    /// </summary>
    private float[,] GenerateNoiseMap(int width, int height, float lacunarity)
    {
        lacunarity += 0.1f;
        // ���������ͼ��Ϊ�˶�������
        float[,] noiseMap = new float[width, height];
        float offsetX = Random.Range(-10000f, 10000f);
        float offsetY = Random.Range(-10000f, 10000f);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                noiseMap[x, y] = Mathf.PerlinNoise(x * lacunarity + offsetX, y * lacunarity + offsetY);
            }
        }
        return noiseMap;
    }

    /// <summary>
    /// ��֡ ���ɵ�ͼ��ͼ
    /// ��������ͼ����ȫ��ɭ�֣�ֱ�ӷ���ɭ����ͼ
    /// </summary>
    private IEnumerator GenerateMapTexture(Vector2Int chunkIndex, System.Action<Texture2D, bool> callBack)
    {
        // ��ǰ�ؿ��ƫ���� �ҵ������ͼ������ÿһ������
        int cellOffsetX = chunkIndex.x * mapConfig.mapChunkSize + 1;
        int cellOffsetY = chunkIndex.y * mapConfig.mapChunkSize + 1;

        // �ǲ���һ��������ɭ�ֵ�ͼ��
        bool isAllForest = true;
        // ����Ƿ�ֻ��ɭ�����͵ĸ���
        for (int y = 0; y < mapConfig.mapChunkSize; y++)
        {
            if (isAllForest == false) break;
            for (int x = 0; x < mapConfig.mapChunkSize; x++)
            {
                MapCell cell = mapGrid.GetCell(x + cellOffsetX, y + cellOffsetY);
                if (cell != null && cell.TextureIndex != 0)
                {
                    isAllForest = false;
                    break;
                }
            }
        }

        Texture2D mapTexture = null;
        // ����������
        if (!isAllForest)
        {
            // ��ͼ���Ǿ���
            int textureCellSize = mapConfig.forestTexutre.width;
            // ������ͼ��Ŀ��,������
            int textureSize = mapConfig.mapChunkSize * textureCellSize;
            mapTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGB24, false);

            // ����ÿһ������
            for (int y = 0; y < mapConfig.mapChunkSize; y++)
            {
                // һִֻ֡��һ�� ֻ����һ�е�����
                yield return null;
                // ����ƫ����
                int pixelOffsetY = y * textureCellSize;
                for (int x = 0; x < mapConfig.mapChunkSize; x++)
                {

                    int pixelOffsetX = x * textureCellSize;
                    int textureIndex = mapGrid.GetCell(x + cellOffsetX, y + cellOffsetY).TextureIndex - 1;
                    // ����ÿһ�������ڵ�����
                    // ����ÿһ�����ص�
                    for (int y1 = 0; y1 < textureCellSize; y1++)
                    {
                        for (int x1 = 0; x1 < textureCellSize; x1++)
                        {

                            // ����ĳ�����ص����ɫ
                            // ȷ����ɭ�ֻ�������
                            // ����ط���ɭ�� ||
                            // ����ط�����������͸���ģ����������Ҫ����groundTextureͬλ�õ�������ɫ
                            if (textureIndex < 0)
                            {
                                Color color = mapConfig.forestTexutre.GetPixel(x1, y1);
                                mapTexture.SetPixel(x1 + pixelOffsetX, y1 + pixelOffsetY, color);
                            }
                            else
                            {
                                // ��������ͼ����ɫ
                                Color color = mapConfig.marshTextures[textureIndex].GetPixel(x1, y1);
                                if (color.a < 1f)
                                {
                                    mapTexture.SetPixel(x1 + pixelOffsetX, y1 + pixelOffsetY, mapConfig.forestTexutre.GetPixel(x1, y1));
                                }
                                else
                                {
                                    mapTexture.SetPixel(x1 + pixelOffsetX, y1 + pixelOffsetY, color);
                                }
                            }

                        }
                    }
                }
            }
            mapTexture.filterMode = FilterMode.Point;
            mapTexture.wrapMode = TextureWrapMode.Clamp;
            mapTexture.Apply();
        }
        callBack?.Invoke(mapTexture, isAllForest);
    }


    /// <summary>
    /// ����һ����ͼ���������
    /// </summary>
    public MapObjectData GenerateMapObjectData(int mapObjectConfigID, Vector3 pos)
    {
        MapObjectData mapObjectData = null;
        MapObjectConfig mapObjectConfig = ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.MapObject, mapObjectConfigID);
        if (mapObjectConfig.IsEmpty == false)
        {
            mapObjectData = GenerateMapObjectData(mapObjectConfigID,pos, mapObjectConfig.DestoryDays);
        }
        return mapObjectData;
    }

    /// <summary>
    /// ͨ��Ȩ�ػ�ȡһ����ͼ���������ID
    /// </summary>
    /// <returns></returns>
    private int GetMapObjectConfigIDForWeight(MapVertexType mapVertexType)
    {
        // ���ݸ����������
        List<int> configIDs = mapObjectConfigDic[mapVertexType];
        // ȷ��Ȩ�ص��ܺ�
        int weightTotal = mapVertexType == MapVertexType.Forest ? mapObjectForestWeightTotal : mapObjectMarshWeightTotal;
        int randValue = Random.Range(1, weightTotal + 1); // ʵ�����������Ǵ�1~weightTotal
        float temp = 0;
        for (int i = 0; i < configIDs.Count; i++)
        {
            temp += ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.MapObject, configIDs[i]).Probability;
            if (randValue < temp)
            {
                // ����
                return configIDs[i];
            }
        }
        return 0;
    }

    /// <summary>
    /// ͨ��Ȩ�ػ�ȡһ��AI������ID
    /// </summary>
    /// <returns></returns>
    private int GetAIConfigIDForWeight(MapVertexType mapVertexType)
    {
        // ���ݸ����������
        List<int> configIDs = AIConfigDic[mapVertexType];
        // ȷ��Ȩ�ص��ܺ�
        int weightTotal = mapVertexType == MapVertexType.Forest ? AIForestWeightTotal : AIMarshWeightTotal;
        int randValue = Random.Range(1, weightTotal + 1); // ʵ�����������Ǵ�1~weightTotal
        float temp = 0;
        for (int i = 0; i < configIDs.Count; i++)
        {
            temp += ConfigManager.Instance.GetConfig<AIConfig>(ConfigName.AI, configIDs[i]).Probability;
            if (randValue < temp)
            {
                // ����
                return configIDs[i];
            }
        }
        return 0;
    }

    /// <summary>
    /// ���ɵ�ͼ�������ݣ�Ϊ�˵�ͼ���ʼ��׼����
    /// </summary>
    private MapChunkData GenerateMapChunkData(Vector2Int chunkIndex)
    {
        MapChunkData mapChunkData = new MapChunkData();
        mapChunkData.MapObjectDic = new Serialization_Dic<ulong, MapObjectData>();
        mapChunkData.AIDataDic = new Serialization_Dic<ulong, MapObjectData>();
        mapChunkData.ForestVertexList = new List<MapVertex>(mapConfig.mapChunkSize * mapConfig.mapChunkSize);
        mapChunkData.MarhshVertexList = new List<MapVertex>(mapConfig.mapChunkSize * mapConfig.mapChunkSize);

        int offsetX = chunkIndex.x * mapConfig.mapChunkSize;
        int offsetY = chunkIndex.y * mapConfig.mapChunkSize;

        for (int x = 1; x < mapConfig.mapChunkSize; x++)
        {
            for (int y = 1; y < mapConfig.mapChunkSize; y++)
            {
                MapVertex mapVertex = mapGrid.GetVertex(x + offsetX, y + offsetY);

                if (mapVertex.VertexType == MapVertexType.Forest)
                {
                    mapChunkData.ForestVertexList.Add(mapVertex);
                }
                else if (mapVertex.VertexType == MapVertexType.Marsh)
                {
                    mapChunkData.MarhshVertexList.Add(mapVertex);
                }

                // ����Ȩ�ػ�ȡһ����ͼ���������ID
                int configID = GetMapObjectConfigIDForWeight(mapVertex.VertexType);
                // ȷ����������ʲô��ͼ����
                MapObjectConfig objectConfig = ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.MapObject, configID);
                if (objectConfig.IsEmpty == false)
                {
                    Vector3 position = mapVertex.Position + new Vector3(Random.Range(-mapConfig.cellSize / 2, mapConfig.cellSize / 2), 0, Random.Range(-mapConfig.cellSize / 2, mapConfig.cellSize / 2));
                    mapVertex.MapObjectID = mapData.CurrentID;
                    mapChunkData.MapObjectDic.Dictionary.Add(mapData.CurrentID, GenerateMapObjectData(configID, position, objectConfig.DestoryDays));
                }
            }
        }

        // ����AI����
        // һ����ͼ�� ɭ�ֻ�����Ķ�����Ҫ�������õĲ�����
        if (mapChunkData.ForestVertexList.Count > mapConfig.GenerateAIMinVertexCountOnChunk)
        {
            for (int i = 0; i < mapConfig.maxAIOnChunk; i++)
            {
                int configID = GetAIConfigIDForWeight(MapVertexType.Forest);
                AIConfig config = ConfigManager.Instance.GetConfig<AIConfig>(ConfigName.AI, configID);
                if (config.IsEmpty == false)
                {
                    mapChunkData.AIDataDic.Dictionary.Add(mapData.CurrentID, GenerateMapObjectData(configID, Vector3.zero, -1));
                }
            }
        }
        if (mapChunkData.MarhshVertexList.Count>mapConfig.GenerateAIMinVertexCountOnChunk)
        {
            for (int i = 0; i < mapConfig.maxAIOnChunk; i++)
            {
                int configID = GetAIConfigIDForWeight(MapVertexType.Marsh);
                AIConfig config = ConfigManager.Instance.GetConfig<AIConfig>(ConfigName.AI, configID);
                if (config.IsEmpty == false)
                {
                    mapChunkData.AIDataDic.Dictionary.Add(mapData.CurrentID, GenerateMapObjectData(configID, Vector3.zero, -1));
                }
            }
        }
        return mapChunkData;
    }

    /// <summary>
    /// �ָ���ͼ������
    /// Ŀǰֻ�ָ�VertexList��
    /// </summary>
    private void RecoverMapChunkData(Vector2Int chunkIndex,MapChunkData mapChunkData)
    {
        mapChunkData.ForestVertexList = new List<MapVertex>(mapConfig.mapChunkSize * mapConfig.mapChunkSize);
        mapChunkData.MarhshVertexList = new List<MapVertex>(mapConfig.mapChunkSize * mapConfig.mapChunkSize);

        int offsetX = chunkIndex.x * mapConfig.mapChunkSize;
        int offsetY = chunkIndex.y * mapConfig.mapChunkSize;

        for (int x = 1; x < mapConfig.mapChunkSize; x++)
        {
            for (int y = 1; y < mapConfig.mapChunkSize; y++)
            {
                MapVertex mapVertex = mapGrid.GetVertex(x + offsetX, y + offsetY);

                if (mapVertex.VertexType == MapVertexType.Forest)
                {
                    mapChunkData.ForestVertexList.Add(mapVertex);
                }
                else if (mapVertex.VertexType == MapVertexType.Marsh)
                {
                    mapChunkData.MarhshVertexList.Add(mapVertex);
                }
            }
        }
    }

    // ��������ÿ�ζ�����һ���µ�list����
    List<MapObjectData> mapOjbectDataList = new List<MapObjectData>();
    /// <summary>
    /// Ϊ��ͼ��ˢ�£����ɵ�ͼ�����б�
    /// ����Ϊnull
    /// ���һ����ͼ������������һ�����������ͼ����������������
    /// </summary>
    public List<MapObjectData> GenerateMapObjectDataListOnMapChunkRefresh(Vector2Int chunkIndex)
    {
        // ��������
        mapOjbectDataList.Clear();
        int offsetX = chunkIndex.x * mapConfig.mapChunkSize;
        int offsetY = chunkIndex.y * mapConfig.mapChunkSize;

        // ������ͼ����
        for (int x = 1; x < mapConfig.mapChunkSize; x++)
        {
            for (int y = 1; y < mapConfig.mapChunkSize; y++)
            {
                // �������û���У���һ�����㲻ˢ��
                if (Random.Range(0, mapConfig.RefreshProbability) != 0) continue;
                MapVertex mapVertex = mapGrid.GetVertex(x + offsetX, y + offsetY);

                // ��Ϊ����������
                if (mapVertex.MapObjectID !=0) continue;

                // ����Ȩ�ػ�ȡһ����ͼ���������ID
                int configID = GetMapObjectConfigIDForWeight(mapVertex.VertexType);
                // ȷ����������ʲô��ͼ����
                MapObjectConfig objectConfig = ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.MapObject, configID);
                if (objectConfig.IsEmpty == false)
                {
                    Vector3 position = mapVertex.Position + new Vector3(Random.Range(-mapConfig.cellSize / 2, mapConfig.cellSize / 2), 0, Random.Range(-mapConfig.cellSize / 2, mapConfig.cellSize / 2));
                    mapVertex.MapObjectID = mapData.CurrentID;
                    mapOjbectDataList.Add(GenerateMapObjectData(configID, position, objectConfig.DestoryDays));
                }
            }
        }
        return mapOjbectDataList;
    }

    /// <summary>
    /// ����һ����ͼ��������
    /// </summary>
    private MapObjectData GenerateMapObjectData(int mapObjectConfigID,Vector3 position,int destoryDays)
    {
        MapObjectData mapObjectData = PoolManager.Instance.GetObject<MapObjectData>();
        mapObjectData.ConfigID = mapObjectConfigID;
        mapObjectData.ID = mapData.CurrentID;
        mapData.CurrentID += 1;
        mapObjectData.Position = position;
        mapObjectData.DestoryDays = destoryDays;
        return mapObjectData;
    }

    #endregion



}


