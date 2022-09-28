using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
using Sirenix.OdinInspector;
using System;

/// <summary>
/// ʱ��״̬����
/// </summary>
[Serializable]
public class TimeStateConfig
{
    // ����ʱ��
    public float durationTime;
    // ����ǿ��
    public float sunIntensity;
    // ������ɫ
    public Color sunColor;
    // ̫���ĽǶ�
    [OnValueChanged(nameof(SetRotation))]
    public Vector3 sunRotation;
    [HideInInspector]
    public Quaternion sunQuaternion;

    // �� ���Ǵ�0.1����˥����û�п��Ƕ��ʱ��㶼����������Ҳ����״̬����
    public bool fog;
    // ��������
    public AudioClip bgAudioClip;

    private void SetRotation()
    {
        sunQuaternion = Quaternion.Euler(sunRotation);
    }

    /// <summary>
    /// ��鲢�Ҽ���ʱ��
    /// </summary>
    /// <returns>�Ƿ��ڵ�ǰ״̬</returns>
    public bool CheckAndCalTime(float currTime, TimeStateConfig nextState, out Quaternion rotation, out Color color, out float sunIntensity)
    {
        // 0~1֮��
        float ratio = 1f - (currTime / durationTime);
        rotation = Quaternion.Slerp(this.sunQuaternion, nextState.sunQuaternion, ratio);
        color = Color.Lerp(this.sunColor, nextState.sunColor, ratio);
        sunIntensity = UnityEngine.Mathf.Lerp(this.sunIntensity, nextState.sunIntensity, ratio);

        if (fog)
        {
            RenderSettings.fogDensity = 0.1f * (1 - ratio);
        }

        // ���ʱ�����0���Ի��ڱ�״̬
        return currTime > 0;
    }
}

[CreateAssetMenu(fileName = "ʱ������", menuName = "Config/ʱ������")]
public class TimeConfig : ConfigBase
{
    [LabelText("ʱ��״̬/�׶�����")]
    // 0��ζ���µ�һ��
    // 0-1�ǰ��� ����������
    public TimeStateConfig[] TimeStateConfigs;    // ʱ������ 
}
