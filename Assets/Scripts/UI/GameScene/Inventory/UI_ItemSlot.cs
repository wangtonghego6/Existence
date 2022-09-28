using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JKFrame;
using UnityEngine.EventSystems;
using System;

/// <summary>
/// ��Ʒ���е�һ������
/// </summary>
[Pool]
public class UI_ItemSlot : MonoBehaviour
{
    [SerializeField] Image bgImg;
    [SerializeField] Image iconImg;
    [SerializeField] Text countText;

    public ItemData ItemData { get; private set; }  // �������������
    public int Index { get; private set; }          // �ڼ�������
    private UI_InventoryWindowBase ownerWindow;         // �������ڣ���Ʒ����ֿ�

    private Transform slotTransform;
    private Transform iconTransform;

    public static UI_ItemSlot CurrentMouseEnterSlot;
    public static UI_ItemSlot WeaponSlot;

    private Func<int, AudioType> onUseAction;
    private void Start()
    {
        slotTransform = transform;
        iconTransform = iconImg.transform;
        // ��꽻���¼�
        this.OnMouseEnter(MouseEnter);
        this.OnMouseExit(MouseExit);
        this.OnBeginDrag(BeginDrag);
        this.OnDrag(Drag);
        this.OnEndDrag(EndDrag);
        UITool.BindMouseEffect(this);
    }
    private void OnEnable()
    {
        this.OnUpdate(CheckMouseRightClick);
    }
    private void OnDisable()
    {
        this.RemoveUpdate(CheckMouseRightClick);
    }

    /// <summary>
    /// �������Ҽ�
    /// �Ƿ����ʹ����Ʒ���
    /// </summary>
    private void CheckMouseRightClick()
    {
        if (ItemData == null || onUseAction ==null) return;
        // �� ��ǰ����Ϊѡ��״̬�����Ұ������Ҽ�����Ŀ����ʹ����Ʒ
        if (isMouseStay && Input.GetMouseButtonDown(1))
        {
            // ����ʹ�õ������������Ч
            AudioType reslutAudioType = onUseAction.Invoke(Index);
            ProjectTool.PlayAudio(reslutAudioType);
        }
    }

    public void Init(int index,UI_InventoryWindowBase ownerWindow, Func<int, AudioType> onUseAction = null)
    { 
        this.Index = index;
        this.ownerWindow = ownerWindow;
        this.onUseAction = onUseAction;
        bgImg.sprite = ownerWindow.bgSprites[0];
    }

    public void InitData(ItemData itemData=null)
    {
        this.ItemData = itemData;
        // �������Ϊ�գ���ζ���Ǹ��ո���
        if (ItemData == null)
        {
            // �������Ϊ��ɫ������������Icon
            bgImg.color = Color.white;
            countText.gameObject.SetActive(false);
            iconImg.sprite = null;
            iconImg.gameObject.SetActive(false);
            return;
        }
        // �����ݵ����
        countText.gameObject.SetActive(true);
        iconImg.gameObject.SetActive(true);
        iconImg.sprite = ItemData.Config.Icon;  // ���ݸ������������ ����������ʾͼƬ
        UpdateCountTextView();
    }

    public void UpdateCountTextView()
    {
        // ���ݲ�ͬ����Ʒ���ͣ���ʾ��ͬ��Ч��
        switch (ItemData.Config.ItemType)
        {
            case ItemType.Weapon:
                bgImg.color = Color.white;
                countText.text = (ItemData.ItemTypeData as ItemWeaponData).Durability.ToString() + "%";
                break;

            case ItemType.Consumable:
                bgImg.color = new Color(0, 1, 0, 0.5f);
                countText.text = (ItemData.ItemTypeData as ItemConsumableData).Count.ToString();
                break;

            case ItemType.Material:
                bgImg.color = Color.white;
                countText.text = (ItemData.ItemTypeData as ItemMaterialData).Count.ToString();
                break;
        }
    }

    #region ��꽻��
    private bool isMouseStay = false;
    private void MouseEnter(PointerEventData arg1, object[] arg2)
    {
        GameManager.Instance.SetCursorState(CursorState.Handle);
        bgImg.sprite = ownerWindow.bgSprites[1];
        isMouseStay = true;
        CurrentMouseEnterSlot = this;
    }
    private void MouseExit(PointerEventData arg1, object[] arg2)
    {
        GameManager.Instance.SetCursorState(CursorState.Normal);
        bgImg.sprite = ownerWindow.bgSprites[0];
        isMouseStay = false;
        CurrentMouseEnterSlot = null;
    }

    private void BeginDrag(PointerEventData arg1, object[] arg2)
    {
        if (ItemData == null) return;
        iconTransform.SetParent(UIManager.Instance.DragLayer);
    }
    private void Drag(PointerEventData eventData, object[] arg2)
    {
        if (ItemData == null) return;
        GameManager.Instance.SetCursorState(CursorState.Handle);
        iconTransform.position = eventData.position;

    }
    private void EndDrag(PointerEventData eventData, object[] arg2)
    {
        if (ItemData == null) return;
        // ���Ӻͽ�����Ľ���
        if (onUseAction!=null&&InputManager.Instance.CheckSlotEndDragOnBuilding(ItemData.ConfigID))
        {
            RestIcon();
            ProjectTool.PlayAudio(AudioType.Bag);
            ownerWindow.DiscardItem(Index);
            return;
        }

        // ��Ȼ�����ˣ�������껹��ĳ����������
        if (CurrentMouseEnterSlot == null)
        {
            GameManager.Instance.SetCursorState(CursorState.Normal);
        }

        // ��ǰ��ק�е�Icon��λ
        RestIcon();
        // ��������Լ� ��ִ���κβ���
        if (CurrentMouseEnterSlot == this) return;

        // ��Ʒ����
        if (CurrentMouseEnterSlot == null)
        {
            // ����ȥ�����û�г���Mask������κ�UI����
            if (InputManager.Instance.CheckMouseOnUI()) return;
            // ����ȥ����ֹ��ҽ���Ʒ�ӵ�������������
            if (InputManager.Instance.CheckMouseOnBigMapObject()) return;
            if (InputManager.Instance.GetMouseWorldPositionOnGround(eventData.position,out Vector3 worldPos))
            {
                // �ڵ���������Ʒ
                worldPos.y = 1;
                MapManager.Instance.SpawnMapObject(ItemData.Config.MapObjectConfigID, worldPos,false);
                ProjectTool.PlayAudio(AudioType.Bag);
                // ����ִ�ж����߼�
                ownerWindow.DiscardItem(Index);
            }
            return;
        }

        // ��ǰ��������������
        if (this == WeaponSlot)
        {
            // Ŀ������ǿյģ�ж�������Ĳ���
            if (CurrentMouseEnterSlot.ItemData == null)
            {
                ProjectTool.PlayAudio(AudioType.TakeDownWeapon);
                SwapSlotItem(this, CurrentMouseEnterSlot);
            }
            // Ŀ�����������
            else if (CurrentMouseEnterSlot.ItemData.Config.ItemType == ItemType.Weapon)
            {
                ProjectTool.PlayAudio(AudioType.TakeUpWeapon);
                SwapSlotItem(this, CurrentMouseEnterSlot);
            }
            // Ŀ����Ӳ�������
            else
            {
                ProjectTool.PlayAudio(AudioType.Fail);
                UIManager.Instance.AddTips("�������װ������!");
            }
        }

        // ��ǰ����ͨ����
        else
        {
            // ��ͨ��������Ķ���ק��������
            if (CurrentMouseEnterSlot == WeaponSlot)
            {
                // ��ǰ���Ӳ������������Բ���ק��ȥ
                if (ItemData.Config.ItemType != ItemType.Weapon)
                {
                    ProjectTool.PlayAudio(AudioType.Fail);
                    UIManager.Instance.AddTips("�������װ������!");
                }
                // ��ǰ����������������ק��
                else
                {
                    ProjectTool.PlayAudio(AudioType.TakeUpWeapon);
                    Debug.Log("����װ����Ʒ:" + ItemData.Config.Name);
                    SwapSlotItem(this, CurrentMouseEnterSlot);
                }
            }
            // ��ͨ���Ӻ���ͨ���ӽ���
            else
            {
                // ������Ʒ����
                SwapSlotItem(this, CurrentMouseEnterSlot);
                ProjectTool.PlayAudio(AudioType.Bag);
            }
        }
    }

    private void RestIcon()
    {
        iconTransform.SetParent(slotTransform);
        iconTransform.localPosition = Vector3.zero;
        iconTransform.localScale = Vector3.one;
    }

    /// <summary>
    /// ���������е���Ʒ����
    /// </summary>
    public static void SwapSlotItem(UI_ItemSlot slot1,UI_ItemSlot slot2)
    {
        ItemData itemData1 = slot1.ItemData;
        ItemData itemData2 = slot2.ItemData;
        // ���ڡ��浵�����ݽ���
        // �п����ǿ細�ڽ����������������ֿ⽻��
        slot1.ownerWindow.SetItem(slot1.Index, itemData2);
        slot2.ownerWindow.SetItem(slot2.Index, itemData1);
    }

    #endregion
}
