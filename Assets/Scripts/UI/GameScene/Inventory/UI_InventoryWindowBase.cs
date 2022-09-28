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
    [SerializeField] protected List<UI_ItemSlot> slots;   // ��Ʒ��
    public Sprite[] bgSprites;  // ��ͼ

    /// <summary>
    /// �����Ʒ
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
    /// �����Ʒ
    /// </summary>
    public bool AddItem(int itemConfigID)
    {
        ItemConfig itemConfig = ConfigManager.Instance.GetConfig<ItemConfig>(ConfigName.Item, itemConfigID);
        switch (itemConfig.ItemType)
        {
            case ItemType.Weapon:
                // ����ֻ�ܷſ�λ
                return CheckAndAddItemForEmptySlot(itemConfigID);
            case ItemType.Consumable:
                // ���ȶѵ�������ŷſ�λ
                if (CheckAndPileItemForSlot(itemConfigID))
                {
                    return true;
                }
                else
                {
                    return CheckAndAddItemForEmptySlot(itemConfigID);
                }
            case ItemType.Material:
                // ���ȶѵ�������ŷſ�λ
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
    /// ����Լ������Ʒ���ո���
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
        return false;   // û�ո�����
    }

    /// <summary>
    /// ֻ��ȡһ���ո��ӣ����û�з���-1
    /// </summary>
    protected int GetEmptySlot()
    {
        // ����������Ʒ�����ǲ���û����
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
    /// ��Ⲣ�Ҷѵ���Ʒ��������
    /// </summary>
    /// <returns></returns>
    protected bool CheckAndPileItemForSlot(int itemConfigID)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            // ��Ϊ�� && ��ͬһ����Ʒ && û����
            if (slots[i].ItemData !=null
                && slots[i].ItemData.ConfigID == itemConfigID)
            {
                // �Ƚϵ��������е����ѵ������͵�ǰ�浵���ݵĶԱ�
                PileItemTypeDataBase data = slots[i].ItemData.ItemTypeData as PileItemTypeDataBase;
                PileItemTypeInfoBase info = slots[i].ItemData.Config.ItemTypeInfo as PileItemTypeInfoBase;
                // ˵��û�ж���
                if (data.Count < info.MaxCount)
                {
                    data.Count += 1; //����һ��
                    slots[i].UpdateCountTextView();
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// �Ƴ�һ����Ʒ�����Ҳ������Ƿ�ѻ�
    /// </summary>
    /// <param name="index"></param>
    protected virtual void RemoveItem(int index)
    {
        inventoryData.RemoveItem(index);
        slots[index].InitData(null);
    }

    /// <summary>
    /// ��ȥһ����Ʒ
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
                // �������д��ڶѵ������
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
    /// ����ĳ�������ϵ���Ʒ
    /// </summary>
    public virtual void SetItem(int index,ItemData itemData)
    {
        inventoryData.SetItem(index, itemData);
        slots[index].InitData(itemData);
    }

    /// <summary>
    /// ��ȡĳ�����������
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
