using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
using Sirenix.OdinInspector;
using System;

/// <summary>
/// ��Ʒ����
/// </summary>
public enum ItemType
{ 
    [LabelText("װ��")] Weapon,    
    [LabelText("����Ʒ")] Consumable, 
    [LabelText("����")] Material    
}

/// <summary>
/// ��������
/// </summary>
public enum WeaponType
{
    [LabelText("��ͷ")] Axe,
    [LabelText("��")] PickAxe,
    [LabelText("����")] Sickle,
}

/// <summary>
/// ��Ʒ����
/// </summary>
[CreateAssetMenu(menuName ="Config/��Ʒ����")]
public class ItemConfig:ConfigBase
{
    [LabelText("����")] public string Name;
    [LabelText("����"),OnValueChanged(nameof(OnItemTypeChanged))] public ItemType ItemType;
    [LabelText("��ͼ��ƷID")] public int MapObjectConfigID;
    [LabelText("����"),MultiLineProperty] public string Description;
    [LabelText("ͼ��")] public Sprite Icon;

    [LabelText("����ר����Ϣ")] public IItemTypeInfo ItemTypeInfo;

    // �������޸ĵ�ʱ���Զ�����ͬ������Ӧ�е�ר����Ϣ
    private void OnItemTypeChanged()
    {
        switch (ItemType)
        {
            case ItemType.Weapon:
                ItemTypeInfo = new ItemWeaponInfo();
                break;
            case ItemType.Consumable:
                ItemTypeInfo = new ItemCosumableInfo();
                break;
            case ItemType.Material:
                ItemTypeInfo = new ItemMaterialInfo();
                break;
        }
    }
}

/// <summary>
/// ��Ʒ������Ϣ�Ľӿ�
/// </summary>
public interface IItemTypeInfo { };

/// <summary>
/// ����������Ϣ
/// </summary>
[Serializable]
public class ItemWeaponInfo : IItemTypeInfo
{
    [LabelText("��������")] public WeaponType WeaponType;
    [LabelText("��������Ԥ����")] public GameObject PrefabOnPlayer;
    [LabelText("��������")] public Vector3 PositionOnPlayer;
    [LabelText("������ת")] public Vector3 RotationOnPlayer;
    [LabelText("����״̬��")] public AnimatorOverrideController AnimatorController;
    [LabelText("������")] public float AttackValue;
    [LabelText("�������")] public float AttackDurabilityCost;  // һ�ι��������ĺ���
    [LabelText("��������")] public float AttackDistance;
    [LabelText("������Ч")] public AudioClip AttackAudio;
    [LabelText("������Ч")] public AudioClip HitAudio;
}

/// <summary>
/// �ɶѵ�����Ʒ�������ݻ���
/// </summary>
[Serializable]
public abstract class PileItemTypeInfoBase
{
    [LabelText("�ѻ�����")] public int MaxCount;
}

/// <summary>
/// ����Ʒ������Ϣ
/// </summary>
[Serializable]
public class ItemCosumableInfo : PileItemTypeInfoBase,IItemTypeInfo
{
    [LabelText("�ָ�����ֵ")] public float RecoverHP;
    [LabelText("�ָ�����ֵֵ")] public float RecoverHungry;
}

/// <summary>
/// ����Ʒ������Ϣ
/// </summary>
[Serializable]
public class ItemMaterialInfo : PileItemTypeInfoBase, IItemTypeInfo
{
}
