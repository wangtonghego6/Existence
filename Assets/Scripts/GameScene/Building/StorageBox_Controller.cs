using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �����������
/// </summary>
public class StorageBox_Controller : BuildingBase
{
    private StorageBoxData storageBoxData;
    [SerializeField] Vector2Int UIWindowGridSize;
    public override void Init(MapChunkController mapChunk, ulong id, bool isFormBuild)
    {
        base.Init(mapChunk, id, isFormBuild);
        if (isFormBuild)
        {
            storageBoxData = new StorageBoxData(UIWindowGridSize.x * UIWindowGridSize.y);
            ArchiveManager.Instance.AddMapObjectTypeData(id, storageBoxData);
        }
        else
        {
            // �������ڵ�ͼˢ����Ʒ����������Ҫ���һ���Ƿ��е�ǰ����������ݣ���Ϊ��ͼˢ�¿���û��
            storageBoxData = ArchiveManager.Instance.GetMapObjectTypeData(id) as StorageBoxData;
        }
    }

    public override void OnSelect()
    {
        // �򿪴�����UI
        InventoryManager.Instance.OpenStorageBoxWindow(this, storageBoxData.InventoryData, UIWindowGridSize);
    }
    

    private void OnDisable()
    {
        storageBoxData = null;
    }

}
