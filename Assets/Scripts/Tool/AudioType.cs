using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// ��Ч����
/// </summary>
public enum AudioType
{ 
    [LabelText("��Ҳ���ʹ��")] PlayerConnot,
    [LabelText("��������")] TakeUpWeapon,
    [LabelText("��������")] TakeDownWeapon,
    [LabelText("����Ʒ�ɹ�")] ConsumablesOK,
    [LabelText("����Ʒʧ��")] ConsumablesFail,
    [LabelText("����")] Bag,
    [LabelText("ͨ��ʧ��")] Fail,
}
