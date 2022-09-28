using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
using Sirenix.OdinInspector;

/// <summary>
/// ��������
/// </summary>
[CreateAssetMenu(fileName ="��������",menuName ="Config/��������")]
public class LootConfig : ConfigBase
{
    [LabelText("���������б�")] public List<LootConfigModel> Configs;

    public void GeneaterMapObject(MapChunkController mapChunk,Vector3 position)
    {
        for (int i = 0; i < Configs.Count; i++)
        {
            // ���ݸ��ʾ����Ƿ�ʵ����
            int randomValue = Random.Range(1, 101); // ʵ�ʷ�Χ��1~100
            if (randomValue < Configs[i].Probability)
            {
                // ���ɵ�������
                MapManager.Instance.SpawnMapObject(mapChunk, Configs[i].LootObjectConfigID, position, false);
            }
        }
    }
}

public class LootConfigModel
{
    [LabelText("��������ID")] public int LootObjectConfigID;
    [LabelText("�������%")] public int Probability;
}
