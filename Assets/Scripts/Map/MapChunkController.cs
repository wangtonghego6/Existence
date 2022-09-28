using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
using UnityEngine.AI;

public class MapChunkController : MonoBehaviour
{
    public Vector2Int ChunkIndex { get; private set; }
    public Vector3 CentrePosition { get; private set; }
    public bool IsAllForest { get; private set; }
    public MapChunkData MapChunkData { get; private set; }
    // MapChunkData里面需要销毁的地图对象
    private Dictionary<ulong, MapObjectData> wantDestoryMapObjectDic;

    // 引用实际在地图上的GameObject
    private Dictionary<ulong,MapObjectBase> mapObjectDic;
    // 引用实际在地图上的AI GameObject
    private Dictionary<ulong, AIBase> AIObjectDic;
    public bool IsInitialized { get; private set; } = false;
    private bool isActive = false;
    public void Init(Vector2Int chunkIndex, Vector3 centrePosition, bool isAllForest,MapChunkData mapChunkData)
    {
        ChunkIndex = chunkIndex;
        CentrePosition = centrePosition;
        MapChunkData = mapChunkData;
        mapObjectDic = new Dictionary<ulong, MapObjectBase>(MapChunkData.MapObjectDic.Dictionary.Count);
        AIObjectDic = new Dictionary<ulong, AIBase>(MapChunkData.AIDataDic.Dictionary.Count);
        IsAllForest = isAllForest;        
        IsInitialized = true;
        wantDestoryMapObjectDic = new Dictionary<ulong, MapObjectData>();
        foreach (var item in MapChunkData.MapObjectDic.Dictionary.Values)
        {
            if (item.DestoryDays>0) wantDestoryMapObjectDic.Add(item.ID, item);
        }

        EventManager.AddEventListener(EventName.OnMorn, OnMorn);
    }
    
    public void SetActive(bool active)
    {
        if (isActive!=active)
        {
            isActive = active;
            gameObject.SetActive(isActive);
            // 从对象池中获取所有物体
            if (isActive)
            {
                // 处理地图对象
                foreach (MapObjectData item in MapChunkData.MapObjectDic.Dictionary.Values)
                {
                    InstantiateMapObject(item,false);
                }
                // 处理AI对象
                foreach (MapObjectData item in MapChunkData.AIDataDic.Dictionary.Values)
                {
                    InstantiateAIObject(item);
                }

            }
            // 把所有物体放回对象池
            else
            {
                foreach (var item in mapObjectDic)
                {
                    item.Value.JKGameObjectPushPool();
                }
                foreach (var item in AIObjectDic)
                {
                    item.Value.Destroy();
                }
                mapObjectDic.Clear();
                AIObjectDic.Clear();
            }
        }

    }
    #region 地图对象
    /// <summary>
    /// 基于MapObjectData实例化地图对象
    /// </summary>
    private void InstantiateMapObject(MapObjectData mapObjectData, bool isFormBuild)
    {
        MapObjectConfig config = ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.MapObject, mapObjectData.ConfigID);
        MapObjectBase mapObj = PoolManager.Instance.GetGameObject(config.Prefab, transform).GetComponent<MapObjectBase>();
        mapObj.transform.position = mapObjectData.Position;
        mapObj.Init(this, mapObjectData.ID, isFormBuild);
        mapObjectDic.Add(mapObjectData.ID, mapObj);
    }

    /// <summary>
    /// 移除一个地图对象
    /// </summary>
    public void RemoveMapObject(ulong mapObjectID)
    {
        // 数据层面移除
        // 数据放进对象池
        MapChunkData.MapObjectDic.Dictionary.Remove(mapObjectID, out MapObjectData mapObjectData);
        mapObjectData.JKObjectPushPool();

        // 自身显示层面移除
        if (mapObjectDic.TryGetValue(mapObjectID, out MapObjectBase mapObjectBase))
        {
            // 把游戏物体放进对象池
            mapObjectBase.JKGameObjectPushPool();
            mapObjectDic.Remove(mapObjectID);
        }

        // 通知上级，主要做UI地图移除
        MapManager.Instance.RemoveMapObject(mapObjectID);
    }

    /// <summary>
    /// 添加一个地图对象
    /// 1.玩家丢弃物体之类的
    /// 2.早晨刷新物体
    /// </summary>
    public void AddMapObject(MapObjectData mapObjectData, bool isFormBuild)
    {
        // 添加存档数据
        MapChunkData.MapObjectDic.Dictionary.Add(mapObjectData.ID, mapObjectData);
        if (mapObjectData.DestoryDays > 0) wantDestoryMapObjectDic.Add(mapObjectData.ID, mapObjectData);
        // 显示层面
        if (isActive)
        {
            InstantiateMapObject(mapObjectData, isFormBuild);
        }
    }
    #endregion

    #region AI
    /// <summary>
    /// 基于MapObjectData实例化地图对象
    /// </summary>
    private void InstantiateAIObject(MapObjectData aiData)
    {
        AIConfig config = ConfigManager.Instance.GetConfig<AIConfig>(ConfigName.AI, aiData.ConfigID);
        AIBase aiObj = PoolManager.Instance.GetGameObject(config.Prefab, transform).GetComponent<AIBase>();
        if (aiData.Position == Vector3.zero)
        {
            aiData.Position = GetAIRandomPoint(config.MapVertexType);
        }
        aiObj.Init(this, aiData);
        AIObjectDic.Add(aiData.ID, aiObj);
    }

    /// <summary>
    /// 获取AI可以到的随机坐标
    /// </summary>
    public Vector3 GetAIRandomPoint(MapVertexType mapVertexType)
    {
        List<MapVertex> verticeList = null;
        // 如果顶点不够，依然用另外一个类型的顶点列表
        if (mapVertexType == MapVertexType.Forest)
        {
            if (MapChunkData.ForestVertexList.Count< MapManager.Instance.MapConfig.GenerateAIMinVertexCountOnChunk)
                verticeList = MapChunkData.MarhshVertexList;
            else verticeList = MapChunkData.ForestVertexList;
        }
        else if (mapVertexType == MapVertexType.Marsh)
        {
            if (MapChunkData.ForestVertexList.Count < MapManager.Instance.MapConfig.GenerateAIMinVertexCountOnChunk)
                verticeList = MapChunkData.ForestVertexList;
            else verticeList = MapChunkData.MarhshVertexList;
        }
        int index = Random.Range(0, verticeList.Count);
        // 确保AI可以到达这个位置
        if (NavMesh.SamplePosition(verticeList[index].Position,out NavMeshHit hitInfo,1,NavMesh.AllAreas))
        {
            return hitInfo.position;
        }
        // 递归查找下一个坐标
        return GetAIRandomPoint(mapVertexType);
    }
    /// <summary>
    /// 因为迁移删除一个AI对象
    /// 只删除本地持有的数据，不做实际的对象删除、处理
    /// </summary>
    public void RemoveAIObjectOnTransfer(ulong aiObjectID)
    {
        MapChunkData.AIDataDic.Dictionary.Remove(aiObjectID,out MapObjectData aiData);
        aiData.JKObjectPushPool();
        AIObjectDic.Remove(aiObjectID);
    }

    /// <summary>
    /// AI的游戏物体、数据存档等都要移除
    /// </summary>
    /// <param name="aiObjectID"></param>
    public void RemoveAIObject(ulong aiObjectID)
    {
        MapChunkData.AIDataDic.Dictionary.Remove(aiObjectID, out MapObjectData aiData);
        aiData.JKObjectPushPool();
        if (AIObjectDic.Remove(aiObjectID,out AIBase aiObject))
        {
            aiObject.Destroy();
        }
    }


    /// <summary>
    /// 因为迁移添加一个AI对象
    /// </summary>
    public void AddAIObjectFormTransfer(MapObjectData aiObjectData,AIBase aiObject)
    {
        MapChunkData.AIDataDic.Dictionary.Add(aiObjectData.ID, aiObjectData);
        AIObjectDic.Add(aiObjectData.ID, aiObject);
        aiObject.transform.SetParent(transform);
        aiObject.InitOnTransfer(this);
    }

    #endregion

    // 执行销毁的地图对象列表
    private static List<ulong> doDestoryMapObjectList = new List<ulong>(20);
    /// <summary>
    /// 当早晨时，刷新地图对象
    /// </summary>
    private void OnMorn()
    {
        // 遍历可能要销毁的地图对象，做时间计算
        foreach (var item in wantDestoryMapObjectDic.Values)
        {
            item.DestoryDays -= 1;
            if (item.DestoryDays == 0) 
            {
                doDestoryMapObjectList.Add(item.ID);
            }
        }
        for (int i = 0; i < doDestoryMapObjectList.Count; i++)
        {
            RemoveMapObject(doDestoryMapObjectList[i]);
        }
        doDestoryMapObjectList.Clear();


        // 得到新增的地图对象数据
        List<MapObjectData> mapObjectDatas = MapManager.Instance.SpawnMapObjectDataOnMapChunkRefresh(ChunkIndex);
        for (int i = 0; i < mapObjectDatas.Count; i++)
        {
            AddMapObject(mapObjectDatas[i],false);
        }

        // TODO:刷新AI
    }

    private void OnDestroy()
    {
        ArchiveManager.Instance.SaveMapChunkData(ChunkIndex, MapChunkData);
    }
}
