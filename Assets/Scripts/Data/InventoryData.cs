using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// ��������
/// </summary>
[Serializable]
public class InventoryData
{
    // ��������װ����Ʒ
    public ItemData[] ItemDatas { get; protected set; }
    public InventoryData(int itemCount)
    {
        ItemDatas = new ItemData[itemCount];
    }
    public void RemoveItem(int index)
    {
        ItemDatas[index] = null;
    }
    public void SetItem(int index, ItemData itemData)
    {
        ItemDatas[index] = itemData;
    }
}

/// <summary>
/// ����������
/// </summary>
[Serializable]
public class MainInventoryData: InventoryData
{
    public MainInventoryData(int itemCount) : base(itemCount) { }

    // ��������װ����Ʒ
    public ItemData WeaponSlotItemData { get; private set; }

    public void RemoveWeaponItem()
    {
        WeaponSlotItemData = null;
    }
    public void SetWeaponItem(ItemData itemData)
    {
        WeaponSlotItemData = itemData;
    }
}
