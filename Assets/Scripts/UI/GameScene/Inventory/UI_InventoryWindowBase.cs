using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JKFrame;
using System;

[UIElement(true,"UI/UI_InventoryWindow",1)]
public abstract class UI_InventoryWindowBase : UI_WindowBase
{
    protected InventoryData inventoryData;
    [SerializeField] protected List<UI_ItemSlot> slots;   // 物品槽
    public Sprite[] bgSprites;  // 框图

    /// <summary>
    /// 添加物品
    /// </summary>
    public bool AddItemAndPlayAuio(int itemConfigID)
    {
        bool res = AddItem(itemConfigID);
        if (res)
        {
            ProjectTool.PlayAudio(AudioType.Bag);
        }
        else
        {
            ProjectTool.PlayAudio(AudioType.Fail);
        }
        return res;
    }

    /// <summary>
    /// 添加物品
    /// </summary>
    public bool AddItem(int itemConfigID)
    {
        ItemConfig itemConfig = ConfigManager.Instance.GetConfig<ItemConfig>(ConfigName.Item, itemConfigID);
        switch (itemConfig.ItemType)
        {
            case ItemType.Weapon:
                // 武器只能放空位
                return CheckAndAddItemForEmptySlot(itemConfigID);
            case ItemType.Consumable:
                // 优先堆叠，否则才放空位
                if (CheckAndPileItemForSlot(itemConfigID))
                {
                    return true;
                }
                else
                {
                    return CheckAndAddItemForEmptySlot(itemConfigID);
                }
            case ItemType.Material:
                // 优先堆叠，否则才放空位
                if (CheckAndPileItemForSlot(itemConfigID))
                {
                    return true;
                }
                else
                {
                    return CheckAndAddItemForEmptySlot(itemConfigID);
                }
        }
        return false;
    }

    /// <summary>
    /// 检查以及添加物品到空格子
    /// </summary>
    /// <returns></returns>
    protected bool CheckAndAddItemForEmptySlot(int itemConfigID)
    {
        int index = GetEmptySlot();
        if (index>=0)
        {
            SetItem(index, ItemData.CreateItemData(itemConfigID));
            return true;
        }
        return false;   // 没空格子了
    }

    /// <summary>
    /// 只获取一个空格子，如果没有返回-1
    /// </summary>
    protected int GetEmptySlot()
    {
        // 遍历所有物品，看是不是没数据
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].ItemData == null)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// 检测并且堆叠物品到格子上
    /// </summary>
    /// <returns></returns>
    protected bool CheckAndPileItemForSlot(int itemConfigID)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            // 不为空 && 是同一个物品 && 没堆满
            if (slots[i].ItemData !=null
                && slots[i].ItemData.ConfigID == itemConfigID)
            {
                // 比较的是配置中的最大堆叠数量和当前存档数据的对比
                PileItemTypeDataBase data = slots[i].ItemData.ItemTypeData as PileItemTypeDataBase;
                PileItemTypeInfoBase info = slots[i].ItemData.Config.ItemTypeInfo as PileItemTypeInfoBase;
                // 说明没有堆满
                if (data.Count < info.MaxCount)
                {
                    data.Count += 1; //增加一个
                    slots[i].UpdateCountTextView();
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 移除一个物品，而且不考虑是否堆积
    /// </summary>
    /// <param name="index"></param>
    protected virtual void RemoveItem(int index)
    {
        inventoryData.RemoveItem(index);
        slots[index].InitData(null);
    }

    /// <summary>
    /// 丢去一件物品
    /// </summary>
    /// <param name="index"></param>
    public virtual void DiscardItem(int index)
    {
        ItemData itemData = slots[index].ItemData;
        switch (itemData.Config.ItemType)
        {
            case ItemType.Weapon:
                RemoveItem(index);
                break;
            default:
                // 其他都有存在堆叠情况的
                PileItemTypeDataBase typeData = itemData.ItemTypeData as PileItemTypeDataBase;
                typeData.Count -= 1;
                if (typeData.Count == 0)
                {
                    RemoveItem(index);
                }
                else
                { 
                    slots[index].UpdateCountTextView();
                }
                break;
        }

    }

    /// <summary>
    /// 设置某个格子上的物品
    /// </summary>
    public virtual void SetItem(int index,ItemData itemData)
    {
        inventoryData.SetItem(index, itemData);
        slots[index].InitData(itemData);
    }

    /// <summary>
    /// 获取某个物体的数量
    /// </summary>
    public int GetItemCount(int configID)
    {
        int count = 0;
        for (int i = 0; i < inventoryData.ItemDatas.Length; i++)
        {
            if (inventoryData.ItemDatas[i]!=null && inventoryData.ItemDatas[i].ConfigID == configID)
            {
                if (inventoryData.ItemDatas[i].ItemTypeData is PileItemTypeDataBase)
                {
                    count += ((PileItemTypeDataBase)inventoryData.ItemDatas[i].ItemTypeData).Count;
                }
                else count += 1;
            }
        }
        return count;
    }


}
