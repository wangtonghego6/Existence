using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 储物箱控制器
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
            // 如果你存在地图刷新物品，那这里需要检测一下是否有当前建筑物的数据，因为地图刷新可能没有
            storageBoxData = ArchiveManager.Instance.GetMapObjectTypeData(id) as StorageBoxData;
        }
    }

    public override void OnSelect()
    {
        // 打开储物箱UI
        InventoryManager.Instance.OpenStorageBoxWindow(this, storageBoxData.InventoryData, UIWindowGridSize);
    }
    

    private void OnDisable()
    {
        storageBoxData = null;
    }

}
