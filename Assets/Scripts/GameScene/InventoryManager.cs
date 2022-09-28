using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
public class InventoryManager : SingletonMono<InventoryManager>
{
    private UI_MainInventoryWindow mainInventoryWindow; // 快捷栏
    public void Init()
    {
        mainInventoryWindow = UIManager.Instance.Show<UI_MainInventoryWindow>();
        mainInventoryWindow.InitData();
    }
    #region 主背包
    /// <summary>
    /// 获取某个物体的数量
    /// </summary>
    public int GetMainInventoryItemCount(int configID)
    {
        return mainInventoryWindow.GetItemCount(configID);
    }
    /// <summary>
    /// 基于建造配置 减少物品
    /// </summary>
    public void UpdateMainInventoryItemsForBuild(BuildConfig buildConfig)
    {
        mainInventoryWindow.UpdateItemsForBuild(buildConfig);
    }

    /// <summary>
    /// 添加物品
    /// </summary>
    public bool AddMainInventoryItemAndPlayAudio(int itemConfigID)
    {
        return mainInventoryWindow.AddItemAndPlayAuio(itemConfigID);
    }
    /// <summary>
    /// 添加物品
    /// </summary>
    public bool AddMainInventoryItem(int itemConfigID)
    {
        return mainInventoryWindow.AddItem(itemConfigID);
    }
    #endregion

    #region 储物箱背包
    public void OpenStorageBoxWindow(StorageBox_Controller storageBox, InventoryData inventoryData, Vector2Int size)
    {
        ProjectTool.PlayAudio(AudioType.Bag);
        UIManager.Instance.Close<UI_StorageBoxInventoryWindow>();
        UIManager.Instance.Show<UI_StorageBoxInventoryWindow>().Init(storageBox, inventoryData, size);
    }
    #endregion
    private void OnDestroy()
    {
        // 保存主背包数据
        ArchiveManager.Instance.SaveMainInventoryData();
    }
}
