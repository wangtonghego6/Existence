using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// ��ͼ��ʼ������
/// </summary>
[Serializable]
public class MapInitData
{
    public int mapSize;
    public int mapSeed;
    public int spawnSeed;
    public float marshLimit;
}

/// <summary>
/// ��ͼ����
/// </summary>
[Serializable]
public class MapData
{
    // ��ǰ��ͼ����IDȡֵ
    public ulong CurrentID = 1;
    // ��ǰ���ȥ�������е�ͼ�飨�Ѿ����ɹ��ĵ�ͼ�飩
    public List<Serialization_Vector2> MapChunkIndexList = new List<Serialization_Vector2>();
}

/// <summary>
/// ��ͼ������
/// </summary>
[Serializable]
public class MapChunkData
{
    public Serialization_Dic<ulong, MapObjectData> MapObjectDic;
    public Serialization_Dic<ulong, MapObjectData> AIDataDic;

    [NonSerialized] public List<MapVertex> ForestVertexList;
    [NonSerialized] public List<MapVertex> MarhshVertexList;
}

/// <summary>
/// ��ͼ����һ�����������
/// </summary>
[Serializable]
public class MapObjectData
{
    // Ψһ����ݱ�ʶ
    public ulong ID;
    // ��ǰ������ʲô����
    public int ConfigID;
    // ʣ��ĸ���������-1������Ч
    public int DestoryDays;
    // ����
    private Serialization_Vector3 position;
    public Vector3 Position
    {
        get => position.ConverToVector3();
        set => position = value.ConverToSVector3();
    }
}