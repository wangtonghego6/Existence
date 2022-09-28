using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JKFrame;

[UIElement(true, "UI/UI_StorageBoxInventoryWindow",1)]
/// <summary>
/// ������UI����
/// </summary>
public class UI_StorageBoxInventoryWindow : UI_InventoryWindowBase
{
    [SerializeField] Button closeButton;
    [SerializeField] Transform itemParent;
    private StorageBox_Controller storageBox;
    public override void Init()
    {
        base.Init();
        slots = new List<UI_ItemSlot>(20);   // ��Ϊ���ӵ�����һ�㲻���ܳ���20
        closeButton.onClick.AddListener(CloseButtonClick);
    }

    public void Init(StorageBox_Controller storageBox,InventoryData inventoryData,Vector2Int size)
    {
        this.storageBox = storageBox;
        this.inventoryData = inventoryData;
        SetWindowSize(size);

        for (int i = 0; i < inventoryData.ItemDatas.Length; i++)
        {
            UI_ItemSlot slot = ResManager.Load<UI_ItemSlot>("UI/UI_ItemSlot", itemParent);
            slot.Init(i, this);
            slot.InitData(inventoryData.ItemDatas[i]);
            slots.Add(slot);
        }
    }

    private void Update()
    {
        if (Player_Controller.Instance!=null)
        {
            if (Vector3.Distance(Player_Controller.Instance.transform.position,storageBox.transform.position)> storageBox.TouchDinstance)
            {
                CloseButtonClick();
            }
        }
    }

    [Sirenix.OdinInspector.Button("SetWindowSize")]
    private void SetWindowSize(Vector2Int size)
    {
        // ��� = ����15+�м��������
        // �߶� = �Ϸ�50 + �м�������� + �ײ�15
        RectTransform rectTransform = transform as RectTransform;
        rectTransform.sizeDelta = new Vector2(30 + size.x * 100, 65 + size.y * 100);
    }

    private void CloseButtonClick()
    {
        ProjectTool.PlayAudio(AudioType.Bag);
        Close();
    }
    public override void OnClose()
    {
        base.OnClose();
       
        // �Ž������
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].JKGameObjectPushPool();
        }
        slots.Clear();
        inventoryData = null;
    }
}
