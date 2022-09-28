using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;

/// <summary>
/// ���ݴ浵������
/// </summary>
public class ArchiveManager : Singleton<ArchiveManager>
{
    public ArchiveManager()
    {
        LoadSaveData();
    }
    public PlayerTransformData PlayerTransformData { get; private set; }
    public PlayerMainData PlayerMainData { get; private set; }
    public MapInitData MapInitData { get; private set; }
    public MapData MapData { get; private set; }
    public Serialization_Dic<ulong, IMapObjectTypeData> MapObjectTypeDataDic { get; private set; }

    public MainInventoryData MainInventoryData { get; private set; }
    public TimeData TimeData { get; private set; }

    public ScienceData ScienceData { get; private set; }

    /// <summary>
    /// �Ƿ��д浵
    /// </summary>
    public bool HaveArchive { get; private set; }

    /// <summary>
    /// ���ش浵ϵͳ���������
    /// </summary>
    public void LoadSaveData()
    {
        // ���浵���ƣ�ֻ��0�Ŵ浵
        SaveItem saveItem = SaveManager.GetSaveItem(0);
        HaveArchive = saveItem != null;
    }

    /// <summary>
    /// �����´浵
    /// ����ʽ��
    /// </summary>
    public void CreateNewArchive(int mapSize,int mapSeed,int spawnSeed,float marshLimit)
    {
        // ��յ�ǰ�浵
        SaveManager.Clear();
        // �����浵��Ϣ
        // 1.Ҫ��SaveManager�д���һ��SaveItem
        SaveManager.CreateSaveItem();
        HaveArchive = true;
        // 2.���������Լ��ĳ�ʼ������
        // ��ͼ��ʼ������
        MapInitData = new MapInitData()
        {
            mapSize = mapSize,
            mapSeed = mapSeed,
            spawnSeed = spawnSeed,
            marshLimit = marshLimit
        };
        SaveManager.SaveObject(MapInitData);

        MapConfig mapConfig = ConfigManager.Instance.GetConfig<MapConfig>(ConfigName.Map);
        float mapSizeOnWorld = mapSize * mapConfig.mapChunkSize * mapConfig.cellSize;
        // �������
        // ��ҵ�λ������
        PlayerTransformData = new PlayerTransformData()
        {
            Position = new Vector3(mapSizeOnWorld / 2, 0, mapSizeOnWorld / 2),
            Rotation = Vector3.zero
        };
        SavePlayerTransformData();
        PlayerConfig playerConfig = ConfigManager.Instance.GetConfig<PlayerConfig>(ConfigName.Player);
        // �����Ҫ����
        PlayerMainData = new PlayerMainData()
        {
            Hp = playerConfig.MaxHp,
            Hungry = playerConfig.MaxHungry
        };
        SavePlayerMainData();

        // ��ͼ����
        MapData = new MapData();
        MapObjectTypeDataDic = new Serialization_Dic<ulong, IMapObjectTypeData>();
        SaveMapData();

        // �������� д��������14��
        MainInventoryData = new MainInventoryData(14);

        #region ��������
        MainInventoryData.ItemDatas[0] = ItemData.CreateItemData(0);
        (MainInventoryData.ItemDatas[0].ItemTypeData as ItemMaterialData).Count = 5;

        MainInventoryData.ItemDatas[1] = ItemData.CreateItemData(1);

        MainInventoryData.ItemDatas[2] = ItemData.CreateItemData(2);
        (MainInventoryData.ItemDatas[2].ItemTypeData as ItemWeaponData).Durability = 60;

        MainInventoryData.ItemDatas[3] = ItemData.CreateItemData(3);
        (MainInventoryData.ItemDatas[3].ItemTypeData as ItemConsumableData).Count = 10;

        MainInventoryData.ItemDatas[4] = ItemData.CreateItemData(4);
        MainInventoryData.ItemDatas[5] = ItemData.CreateItemData(5);
        #endregion
        SaveMainInventoryData();

        // ʱ������
        TimeConfig timeConfig = ConfigManager.Instance.GetConfig<TimeConfig>(ConfigName.Time);
        TimeData = new TimeData()
        {
            StateIndex = 0,
            DayNum = 0,
            CalculateTime = timeConfig.TimeStateConfigs[0].durationTime
        };
        SaveTimeData();

        // �Ƽ�����
        ScienceData = new ScienceData();
        SaveScienceData();
    }

    /// <summary>
    /// ���ص�ǰ�浵-������Ϸ
    /// </summary>
    public void LoadCurrentArchive()
    {
        MapInitData = SaveManager.LoadObject<MapInitData>();
        PlayerTransformData = SaveManager.LoadObject<PlayerTransformData>();
        MapData = SaveManager.LoadObject<MapData>();
        MapObjectTypeDataDic = SaveManager.LoadObject<Serialization_Dic<ulong,IMapObjectTypeData>>();
        MainInventoryData = SaveManager.LoadObject<MainInventoryData>();
        TimeData = SaveManager.LoadObject<TimeData>();
        PlayerMainData = SaveManager.LoadObject<PlayerMainData>();
        ScienceData = SaveManager.LoadObject<ScienceData>();
    }

    /// <summary>
    /// �������λ�����ݴ浵������
    /// </summary>
    public void SavePlayerTransformData()
    {
        SaveManager.SaveObject(PlayerTransformData);
    }

    /// <summary>
    /// ������ҵ���Ҫ����
    /// </summary>
    public void SavePlayerMainData()
    {
        SaveManager.SaveObject(PlayerMainData);
    }

    /// <summary>
    /// �����ͼ���ݴ浵������
    /// </summary>
    public void SaveMapData()
    {
        SaveMapObjectTypeData();
        SaveManager.SaveObject(MapData);
    }

    /// <summary>
    /// ����Լ�����һ����ͼ������
    /// </summary>
    public void AddAndSaveMapChunkData(Vector2Int chunkIndex,MapChunkData mapChunkData)
    {
        Serialization_Vector2 index = chunkIndex.ConverToSVector2();
        MapData.MapChunkIndexList.Add(index);
        SaveMapData();
        // ����һ����ͼ��
        SaveManager.SaveObject(mapChunkData, "Map_" + index.ToString());
    }

    /// <summary>
    /// ����һ����ͼ������
    /// </summary>
    public void SaveMapChunkData(Vector2Int chunkIndex, MapChunkData mapChunkData)
    {
        Serialization_Vector2 index = chunkIndex.ConverToSVector2();
        SaveManager.SaveObject(mapChunkData, "Map_" + index.ToString());
    }

    /// <summary>
    /// ��ȡһ����ͼ��浵����
    /// </summary>
    public MapChunkData GetMapChunkData(Serialization_Vector2 chunkIndex)
    {
        return SaveManager.LoadObject<MapChunkData>("Map_" + chunkIndex.ToString());
    }

    /// <summary>
    /// ���汳��/���������
    /// </summary>
    public void SaveMainInventoryData()
    { 
        SaveManager.SaveObject(MainInventoryData);
    }

    /// <summary>
    /// ����ʱ������
    /// </summary>
    public void SaveTimeData()
    {
        SaveManager.SaveObject(TimeData);
    }

    /// <summary>
    /// ����Ƽ�����
    /// </summary>
    public void SaveScienceData()
    { 
        SaveManager.SaveObject(ScienceData);
    }

    /// <summary>
    /// ��ȡ��ͼ�������������
    /// </summary>
    public IMapObjectTypeData GetMapObjectTypeData(ulong id)
    {
        return MapObjectTypeDataDic.Dictionary[id];
    }

    /// <summary>
    /// ���Ի�ȡ��ͼ�������������
    /// </summary>
    public bool TryGetMapObjectTypeData(ulong id,out IMapObjectTypeData mapObjectTypeData)
    {
        return MapObjectTypeDataDic.Dictionary.TryGetValue(id, out mapObjectTypeData);
    }

    /// <summary>
    /// ��ӵ�ͼ�������������
    /// </summary>
    public void AddMapObjectTypeData(ulong mapObjectID,IMapObjectTypeData mapObjectTypeData)
    {
        MapObjectTypeDataDic.Dictionary.Add(mapObjectID, mapObjectTypeData);
    }

    /// <summary>
    /// �Ƴ���ͼ�������������
    /// </summary>
    /// <param name="mapObjectID"></param>
    public void RemoveMapObjectTypeData(ulong mapObjectID)
    {
        MapObjectTypeDataDic.Dictionary.Remove(mapObjectID);
    }

    public void SaveMapObjectTypeData()
    {
        SaveManager.SaveObject(MapObjectTypeDataDic);
    }
}

