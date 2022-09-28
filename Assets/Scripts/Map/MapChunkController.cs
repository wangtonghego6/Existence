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
    // MapChunkData������Ҫ���ٵĵ�ͼ����
    private Dictionary<ulong, MapObjectData> wantDestoryMapObjectDic;

    // ����ʵ���ڵ�ͼ�ϵ�GameObject
    private Dictionary<ulong,MapObjectBase> mapObjectDic;
    // ����ʵ���ڵ�ͼ�ϵ�AI GameObject
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
            // �Ӷ�����л�ȡ��������
            if (isActive)
            {
                // �����ͼ����
                foreach (MapObjectData item in MapChunkData.MapObjectDic.Dictionary.Values)
                {
                    InstantiateMapObject(item,false);
                }
                // ����AI����
                foreach (MapObjectData item in MapChunkData.AIDataDic.Dictionary.Values)
                {
                    InstantiateAIObject(item);
                }

            }
            // ����������Żض����
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
    #region ��ͼ����
    /// <summary>
    /// ����MapObjectDataʵ������ͼ����
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
    /// �Ƴ�һ����ͼ����
    /// </summary>
    public void RemoveMapObject(ulong mapObjectID)
    {
        // ���ݲ����Ƴ�
        // ���ݷŽ������
        MapChunkData.MapObjectDic.Dictionary.Remove(mapObjectID, out MapObjectData mapObjectData);
        mapObjectData.JKObjectPushPool();

        // ������ʾ�����Ƴ�
        if (mapObjectDic.TryGetValue(mapObjectID, out MapObjectBase mapObjectBase))
        {
            // ����Ϸ����Ž������
            mapObjectBase.JKGameObjectPushPool();
            mapObjectDic.Remove(mapObjectID);
        }

        // ֪ͨ�ϼ�����Ҫ��UI��ͼ�Ƴ�
        MapManager.Instance.RemoveMapObject(mapObjectID);
    }

    /// <summary>
    /// ���һ����ͼ����
    /// 1.��Ҷ�������֮���
    /// 2.�糿ˢ������
    /// </summary>
    public void AddMapObject(MapObjectData mapObjectData, bool isFormBuild)
    {
        // ��Ӵ浵����
        MapChunkData.MapObjectDic.Dictionary.Add(mapObjectData.ID, mapObjectData);
        if (mapObjectData.DestoryDays > 0) wantDestoryMapObjectDic.Add(mapObjectData.ID, mapObjectData);
        // ��ʾ����
        if (isActive)
        {
            InstantiateMapObject(mapObjectData, isFormBuild);
        }
    }
    #endregion

    #region AI
    /// <summary>
    /// ����MapObjectDataʵ������ͼ����
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
    /// ��ȡAI���Ե����������
    /// </summary>
    public Vector3 GetAIRandomPoint(MapVertexType mapVertexType)
    {
        List<MapVertex> verticeList = null;
        // ������㲻������Ȼ������һ�����͵Ķ����б�
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
        // ȷ��AI���Ե������λ��
        if (NavMesh.SamplePosition(verticeList[index].Position,out NavMeshHit hitInfo,1,NavMesh.AllAreas))
        {
            return hitInfo.position;
        }
        // �ݹ������һ������
        return GetAIRandomPoint(mapVertexType);
    }
    /// <summary>
    /// ��ΪǨ��ɾ��һ��AI����
    /// ֻɾ�����س��е����ݣ�����ʵ�ʵĶ���ɾ��������
    /// </summary>
    public void RemoveAIObjectOnTransfer(ulong aiObjectID)
    {
        MapChunkData.AIDataDic.Dictionary.Remove(aiObjectID,out MapObjectData aiData);
        aiData.JKObjectPushPool();
        AIObjectDic.Remove(aiObjectID);
    }

    /// <summary>
    /// AI����Ϸ���塢���ݴ浵�ȶ�Ҫ�Ƴ�
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
    /// ��ΪǨ�����һ��AI����
    /// </summary>
    public void AddAIObjectFormTransfer(MapObjectData aiObjectData,AIBase aiObject)
    {
        MapChunkData.AIDataDic.Dictionary.Add(aiObjectData.ID, aiObjectData);
        AIObjectDic.Add(aiObjectData.ID, aiObject);
        aiObject.transform.SetParent(transform);
        aiObject.InitOnTransfer(this);
    }

    #endregion

    // ִ�����ٵĵ�ͼ�����б�
    private static List<ulong> doDestoryMapObjectList = new List<ulong>(20);
    /// <summary>
    /// ���糿ʱ��ˢ�µ�ͼ����
    /// </summary>
    private void OnMorn()
    {
        // ��������Ҫ���ٵĵ�ͼ������ʱ�����
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


        // �õ������ĵ�ͼ��������
        List<MapObjectData> mapObjectDatas = MapManager.Instance.SpawnMapObjectDataOnMapChunkRefresh(ChunkIndex);
        for (int i = 0; i < mapObjectDatas.Count; i++)
        {
            AddMapObject(mapObjectDatas[i],false);
        }

        // TODO:ˢ��AI
    }

    private void OnDestroy()
    {
        ArchiveManager.Instance.SaveMapChunkData(ChunkIndex, MapChunkData);
    }
}
