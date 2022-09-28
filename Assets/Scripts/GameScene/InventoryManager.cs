using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
public class InventoryManager : SingletonMono<InventoryManager>
{
    private UI_MainInventoryWindow mainInventoryWindow; // �����
    public void Init()
    {
        mainInventoryWindow = UIManager.Instance.Show<UI_MainInventoryWindow>();
        mainInventoryWindow.InitData();
    }
    #region ������
    /// <summary>
    /// ��ȡĳ�����������
    /// </summary>
    public int GetMainInventoryItemCount(int configID)
    {
        return mainInventoryWindow.GetItemCount(configID);
    }
    /// <summary>
    /// ���ڽ������� ������Ʒ
    /// </summary>
    public void UpdateMainInventoryItemsForBuild(BuildConfig buildConfig)
    {
        mainInventoryWindow.UpdateItemsForBuild(buildConfig);
    }

    /// <summary>
    /// �����Ʒ
    /// </summary>
    public bool AddMainInventoryItemAndPlayAudio(int itemConfigID)
    {
        return mainInventoryWindow.AddItemAndPlayAuio(itemConfigID);
    }
    /// <summary>
    /// �����Ʒ
    /// </summary>
    public bool AddMainInventoryItem(int itemConfigID)
    {
        return mainInventoryWindow.AddItem(itemConfigID);
    }
    #endregion

    #region �����䱳��
    public void OpenStorageBoxWindow(StorageBox_Controller storageBox, InventoryData inventoryData, Vector2Int size)
    {
        ProjectTool.PlayAudio(AudioType.Bag);
        UIManager.Instance.Close<UI_StorageBoxInventoryWindow>();
        UIManager.Instance.Show<UI_StorageBoxInventoryWindow>().Init(storageBox, inventoryData, size);
    }
    #endregion
    private void OnDestroy()
    {
        // ��������������
        ArchiveManager.Instance.SaveMainInventoryData();
    }
}
