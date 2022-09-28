using JKFrame;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

[UIElement(false, "UI/UI_MainInventoryWindow", 1)]
public class UI_MainInventoryWindow : UI_InventoryWindowBase
{
    private MainInventoryData mainInventoryData;
    [SerializeField] UI_ItemSlot weaponSlot;// װ����

    public override void Init()
    {
        base.Init();
        // ����Ŀǰ��������������ڴ���ģ�����������ڲ������٣���ʹ�رգ�ҲҪ���������¼�
        EventManager.AddEventListener(EventName.PlayerWeaponAttackSucceed, OnPlayerWeaponAttackSucceed);
    }

    public void InitData()
    {
        // ȷ���浵����
        inventoryData = ArchiveManager.Instance.MainInventoryData;
        mainInventoryData = inventoryData as MainInventoryData;
        // ��ʼ������
        InitSlotData();
        // ��ʼ��������е�����
        Player_Controller.Instance.ChangeWeapon(mainInventoryData.WeaponSlotItemData);
    }
    private void InitSlotData()
    {
        // ����ͨ���ӳ�ʼ������
        for (int i = 0; i < inventoryData.ItemDatas.Length; i++)
        {
            slots[i].Init(i, this,UseItem);
            slots[i].InitData(inventoryData.ItemDatas[i]);
        }
        // ���������ӳ�ʼ������
        UI_ItemSlot.WeaponSlot = weaponSlot;
        weaponSlot.Init(inventoryData.ItemDatas.Length, this,UseItem);
        weaponSlot.InitData(mainInventoryData.WeaponSlotItemData);
    }
    private void Update()
    {
        #region ����
        if (Input.GetKeyDown(KeyCode.Alpha0)) AddItemAndPlayAuio(0);
        if (Input.GetKeyDown(KeyCode.Alpha1)) AddItemAndPlayAuio(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) AddItemAndPlayAuio(2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) AddItemAndPlayAuio(3);
        #endregion
    }

    /// <summary>
    /// �����ʹ�����������ɹ���
    /// </summary>
    private void OnPlayerWeaponAttackSucceed()
    {
        if (mainInventoryData.WeaponSlotItemData == null) return;

        ItemWeaponData weaponData = mainInventoryData.WeaponSlotItemData.ItemTypeData as ItemWeaponData;
        ItemWeaponInfo weaponInfo = mainInventoryData.WeaponSlotItemData.Config.ItemTypeInfo as ItemWeaponInfo;
        weaponData.Durability -= weaponInfo.AttackDurabilityCost;
        if (weaponData.Durability <= 0)
        {
            // ��
            mainInventoryData.RemoveWeaponItem();
            // �������õ��������
            weaponSlot.InitData(null);
            // ֪ͨ���ж������
            Player_Controller.Instance.ChangeWeapon(null);
        }
        else
        {
            // �����;ö�UI
            weaponSlot.UpdateCountTextView();
        }
    }

    /// <summary>
    /// �Ƴ�һ����Ʒ�����Ҳ������Ƿ�ѻ�
    /// </summary>
    /// <param name="index"></param>
    protected override void RemoveItem(int index)
    {
        // ������
        if (index == inventoryData.ItemDatas.Length)
        {
            mainInventoryData.RemoveWeaponItem();
            weaponSlot.InitData(null);
        }
        // ��ͨ����
        else
        {
            base.RemoveItem(index);
        }
    }

    public override void DiscardItem(int index)
    {
        // ������ֱ�Ӷ���
        if (index == slots.Count)
        {
            RemoveItem(index);
        }
        else
        {
            base.DiscardItem(index);
        }
    }

    public override void SetItem(int index, ItemData itemData)
    {
        // ������
        if (index == mainInventoryData.ItemDatas.Length)
        {
            mainInventoryData.SetWeaponItem(itemData);
            weaponSlot.InitData(itemData);
            // ����������ͬ�������
            Player_Controller.Instance.ChangeWeapon(itemData);
        }
        else
        {
            base.SetItem(index, itemData);
        }
    }

    /// <summary>
    /// ʹ����Ʒ
    /// </summary>
    public AudioType UseItem(int index)
    {
        // ��ҵ�״̬Ҳ������ʹ����Ʒ
        if (Player_Controller.Instance.CanUseItem == false) return AudioType.PlayerConnot;
        // ��Ʒ�ۣ���ͼ�Ƿ�������
        if (index == slots.Count)
        {
            int emptySlotIndex = GetEmptySlot();
            if (emptySlotIndex > 0)
            {
                // �����ۺͿո��ӽ��н���
                UI_ItemSlot.SwapSlotItem(weaponSlot, slots[emptySlotIndex]);
                return AudioType.TakeDownWeapon;
            }
            else
            {
                // ˵��û�пո��� ���Ա���
                return AudioType.Fail;
            }
        }
        ItemData itemData = slots[index].ItemData;
        switch (itemData.Config.ItemType)
        {
            case ItemType.Weapon:
                // ʹ������
                UI_ItemSlot.SwapSlotItem(slots[index], weaponSlot);
                return AudioType.TakeUpWeapon;
            case ItemType.Consumable:
                // ����ҵ����ݽ�����Ч
                ItemCosumableInfo info = itemData.Config.ItemTypeInfo as ItemCosumableInfo;
                if (info.RecoverHP != 0) Player_Controller.Instance.RecoverHP(info.RecoverHP);
                if (info.RecoverHungry != 0) Player_Controller.Instance.RecoverHungry(info.RecoverHungry);
                // ������Ʒ������
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
                return AudioType.ConsumablesOK;
            default:
                return AudioType.Fail;
        }
    }
    /// <summary>
    /// ���ڽ������� ������Ʒ
    /// </summary>
    public void UpdateItemsForBuild(BuildConfig buildConfig)
    {
        for (int i = 0; i < buildConfig.BuildConfigConditionList.Count; i++)
        {
            UpdateItemForBuild(buildConfig.BuildConfigConditionList[i]);
        }
    }
    private void UpdateItemForBuild(BuildConfigCondition buildConfigCondition)
    {
        int count = buildConfigCondition.Count;
        for (int i = 0; i < inventoryData.ItemDatas.Length; i++)
        {
            ItemData itemData = inventoryData.ItemDatas[i];
            if (itemData != null && itemData.ConfigID == buildConfigCondition.ItemId)
            {
                if (itemData.ItemTypeData is PileItemTypeDataBase)
                {
                    PileItemTypeDataBase pileItemTypeData = itemData.ItemTypeData as PileItemTypeDataBase;
                    // ��� = ��ǰ���������е����� - ��Ҫ������
                    int quantity = pileItemTypeData.Count - count;
                    if (quantity > 0) // ����������
                    {
                        pileItemTypeData.Count -= count;
                        slots[i].UpdateCountTextView();
                        return;
                    }
                    else // �����������߸պ�
                    {
                        count -= pileItemTypeData.Count;
                        RemoveItem(i);
                        if (count == 0) return;
                    }
                }
                else
                {
                    count -= 1;
                    RemoveItem(i);
                    if (count == 0) return;
                }
            }
        }

        // ���ִ�е����˵���Ǹ�BUG
        Debug.LogError("���������������ڵ�����������������!");
    }
}
