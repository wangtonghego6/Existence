using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ������Ҫ��������͸���
/// </summary>
public class MapGrid
{
    // ��������
    public Dictionary<Vector2Int, MapVertex> vertexDic = new Dictionary<Vector2Int, MapVertex>();
    // ��������
    public Dictionary<Vector2Int, MapCell> cellDic = new Dictionary<Vector2Int, MapCell>();

    public MapGrid(int mapHeight, int mapWdith, float cellSize)
    {
        MapHeight = mapHeight;
        MapWidth = mapWdith;
        CellSize = cellSize;

        // ���ɶ�������
        for (int x = 1; x < mapWdith; x++)
        {
            for (int z = 1; z < mapHeight; z++)
            {
                AddVertex(x, z);
                AddCell(x, z);
            }
        }
        // ����һ��һ��
        for (int x = 1; x <= mapWdith; x++)
        {
            AddCell(x, mapHeight);
        }
        for (int z = 1; z < mapWdith; z++)
        {
            AddCell(mapWdith, z);
        }

    }

    public int MapHeight { get; private set; }
    public int MapWidth { get; private set; }
    public float CellSize { get; private set; }


    #region ����
    private void AddVertex(int x, int y)
    {
        vertexDic.Add(new Vector2Int(x, y)
            , new MapVertex()
            {
                Position = new Vector3(x * CellSize, 0, y * CellSize)
            });
    }

    /// <summary>
    /// ��ȡ���㣬����Ҳ�������Null
    /// </summary>
    public MapVertex GetVertex(Vector2Int index)
    {
        MapVertex vertex = null;
        vertexDic.TryGetValue(index, out vertex);
        return vertex;
    }
    public MapVertex GetVertex(int x, int y)
    {
        return GetVertex(new Vector2Int(x, y));
    }

    /// <summary>
    /// ͨ�����������ȡ����
    /// </summary>
    public MapVertex GetVertexByWorldPosition(Vector3 position)
    {
        int x = Mathf.Clamp(Mathf.RoundToInt(position.x / CellSize), 1, MapWidth);
        int y = Mathf.Clamp(Mathf.RoundToInt(position.z / CellSize), 1, MapHeight);
        return GetVertex(x, y);
    }


    /// <summary>
    /// ���ö�������
    /// </summary>
    private void SetVertexType(Vector2Int vertexIndex, MapVertexType mapVertexType)
    {
        MapVertex vertex = GetVertex(vertexIndex);
        if (vertex.VertexType != mapVertexType)
        {
            vertex.VertexType = mapVertexType;
            // ֻ��������Ҫ����
            if (vertex.VertexType == MapVertexType.Marsh)
            {
                // ���㸽������ͼȨ��

                MapCell tempCell = GetLeftBottomMapCell(vertexIndex);
                if (tempCell != null) tempCell.TextureIndex += 1;

                tempCell = GetRightBottomMapCell(vertexIndex);
                if (tempCell != null) tempCell.TextureIndex += 2;

                 tempCell = GetLeftTopMapCell(vertexIndex);
                if (tempCell != null) tempCell.TextureIndex += 4;

                tempCell = GetRightTopMapCell(vertexIndex);
                if (tempCell != null) tempCell.TextureIndex += 8;
            }
        }
    }

    /// <summary>
    /// ���ö�������
    /// </summary>
    private void SetVertexType(int x,int y,MapVertexType mapVertexType)
    {
        SetVertexType(new Vector2Int(x, y), mapVertexType);
    }
    #endregion

    #region ����
    private void AddCell(int x, int y)
    {
        float offset = CellSize / 2;
        cellDic.Add(new Vector2Int(x, y), 
            new MapCell()
            {
                Position = new Vector3(x * CellSize - offset, 0, y * CellSize - offset)
            }
        );
    }

    /// <summary>
    /// ��ȡ���ӣ����û���ҵ��᷵��Null
    /// </summary>
    public MapCell GetCell(Vector2Int index)
    {
        MapCell cell = null;
        cellDic.TryGetValue(index, out cell);
        return cell;
    }

    public MapCell GetCell(int x,int y)
    {
        return GetCell(new Vector2Int(x,y));
    }

    /// <summary>
    /// ��ȡ���½Ǹ���
    /// </summary>
    public MapCell GetLeftBottomMapCell(Vector2Int vertexIndex)
    {
        return GetCell(vertexIndex);
    }

    /// <summary>
    /// ��ȡ���½Ǹ���
    /// </summary>
    public MapCell GetRightBottomMapCell(Vector2Int vertexIndex)
    {
        return GetCell(vertexIndex.x+1,vertexIndex.y);
    }

    /// <summary>
    /// ��ȡ���ϽǸ���
    /// </summary>
    public MapCell GetLeftTopMapCell(Vector2Int vertexIndex)
    {
        return GetCell(vertexIndex.x, vertexIndex.y+1);
    }

    /// <summary>
    /// ��ȡ���ϽǸ���
    /// </summary>
    public MapCell GetRightTopMapCell(Vector2Int vertexIndex)
    {
        return GetCell(vertexIndex.x+1, vertexIndex.y + 1);
    }
    #endregion

    /// <summary>
    /// ���������ͼ����������
    /// </summary>
    public void CalculateMapVertexType(float[,] noiseMap,float limit)
    { 
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        for (int x = 1; x < width; x++)
        {
            for (int z = 1; z < height; z++)
            {
                // ���������е�ֵȷ��������������
                // ���ڱ߽������󣬷�����ɭ��
                if (noiseMap[x,z] >=limit)
                {
                    SetVertexType(x, z, MapVertexType.Marsh);
                }
                else
                {
                    SetVertexType(x, z, MapVertexType.Forest);
                }
            }
        }
    }
}

/// <summary>
/// ��������
/// </summary>
public enum MapVertexType
{ 
    None,
    Forest, //ɭ��
    Marsh,  //����
}

/// <summary>
/// ��ͼ����
/// </summary>
public class MapVertex
{
    public Vector3 Position;
    public MapVertexType VertexType;
    public ulong MapObjectID;   // ��ǰ��ͼ�����ϵĵ�ͼ����ID,0����յ�
}

/// <summary>
/// ��ͼ����
/// </summary>
public class MapCell
{
    public Vector3 Position;
    public int TextureIndex;
}
