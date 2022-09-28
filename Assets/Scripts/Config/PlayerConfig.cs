using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName ="Config/�������",fileName = "�������")]
public class PlayerConfig : ConfigBase
{
    #region ��ɫ����
    [FoldoutGroup("��ɫ����"), LabelText("��ת�ٶ�")]
    public float RotateSpeed = 10;
    [FoldoutGroup("��ɫ����"), LabelText("�ƶ��ٶ�")]
    public float MoveSpeed  = 4;
    [FoldoutGroup("��ɫ����"), LabelText("�������ֵ")]
    public float MaxHp = 100;
    [FoldoutGroup("��ɫ����"), LabelText("��󼢶�ֵ")]
    public float MaxHungry = 100;
    [FoldoutGroup("��ɫ����"), LabelText("����ֵ˥���ٶ�")]
    public float HungryReduceSeed = 0.2f;
    [FoldoutGroup("��ɫ����"), LabelText("������ֵΪ0��ʱ ����ֵ��˥���ٶ�")]
    public float HpReduceSpeedOnHungryIsZero = 2;
    [FoldoutGroup("��ɫ����"), LabelText("��·��Ч")]
    public AudioClip[] FootstepAudioClis;
    [FoldoutGroup("��ɫ����"), LabelText("Ĭ�϶���״̬��")]
    public RuntimeAnimatorController NormalAnimatorController;
    #endregion

    #region ����
    [FoldoutGroup("����"), LabelText("��Ч����")]
    public Dictionary<AudioType, AudioClip> AudioClipDic;
    #endregion
}
