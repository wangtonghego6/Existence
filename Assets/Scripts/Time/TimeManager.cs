using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
using System;
using Sirenix.OdinInspector;

/// <summary>
/// ʱ�������
/// </summary>
public class TimeManager : LogicManagerBase<TimeManager>
{
    [SerializeField] private Light mainLight;                   // ̫��
    private TimeData timeData;
    private TimeConfig timeConfig;
    [SerializeField,Range(0,30)] public float timeScale = 1;
    private int nextIndex;
    public int CurrentDayNum { get => timeData.DayNum; }
    protected override void RegisterEventListener() { }

    protected override void CancelEventListener() { }

    public void Init()
    {
        timeConfig = ConfigManager.Instance.GetConfig<TimeConfig>(ConfigName.Time);
        timeData = ArchiveManager.Instance.TimeData;
        InitState();
    }

    private void Update()
    {
        if (GameSceneManager.Instance.IsInitialized == false) return;
        UpdateTime();
    }

    // �״�Ӧ�ô浵��ʱ����Ҫ������������ʼ������
    private void InitState()
    {
        // ���ó�ʼ��nextIndex
        nextIndex = timeData.StateIndex + 1 >= timeConfig.TimeStateConfigs.Length ? 0 : timeData.StateIndex + 1;
        // ���ó�ʼ��������
        RenderSettings.fog = timeConfig.TimeStateConfigs[timeData.StateIndex].fog;
        // ��������
        if (timeConfig.TimeStateConfigs[timeData.StateIndex].bgAudioClip != null) StartCoroutine(ChangeBGAudio(timeConfig.TimeStateConfigs[timeData.StateIndex].bgAudioClip));
        // �����Ƿ�Ϊ̫�����¼�
        EventManager.EventTrigger<bool>(EventName.UpdateTimeState, timeData.StateIndex <= 1);
        // ���͵�ǰ�ǵڼ�����¼�
        EventManager.EventTrigger<int>(EventName.UpdateDayNum, timeData.DayNum);
    }
    private void UpdateTime()
    {
        timeData.CalculateTime -= Time.deltaTime * timeScale;
        // ���㲢�ҵõ�������ص�����
        if (!timeConfig.TimeStateConfigs[timeData.StateIndex].CheckAndCalTime(timeData.CalculateTime, timeConfig.TimeStateConfigs[nextIndex], out Quaternion quaternion, out Color color, out float sunIntensity))
        {
            EnterNextState();
        }
        SetLight(sunIntensity, quaternion, color);
    }
    // �����µ��¼�״̬
    private void EnterNextState()
    {
        // �л�����һ��״̬
        timeData.StateIndex = nextIndex;
        // ���߽磬�����ʹ�0��ʼ
        nextIndex = timeData.StateIndex + 1 >= timeConfig.TimeStateConfigs.Length ? 0 : timeData.StateIndex + 1;
        // ������������ϣ�Ҳ����currentStateIndex==0����ô��ζ�ţ�������1
        if (timeData.StateIndex == 0)
        {
            timeData.DayNum++;
            // ���͵�ǰ�ǵڼ�����¼�
            EventManager.EventTrigger<int>(EventName.UpdateDayNum, timeData.DayNum);
            // ���͵�ǰʱ�糿���¼�
            EventManager.EventTrigger(EventName.OnMorn);
        }
        timeData.CalculateTime = timeConfig.TimeStateConfigs[timeData.StateIndex].durationTime;

        // ��
        RenderSettings.fog = timeConfig.TimeStateConfigs[timeData.StateIndex].fog;

        // ��������
        if (timeConfig.TimeStateConfigs[timeData.StateIndex].bgAudioClip != null)
            StartCoroutine(ChangeBGAudio(timeConfig.TimeStateConfigs[timeData.StateIndex].bgAudioClip));

        // �����Ƿ�Ϊ̫�����¼�
        EventManager.EventTrigger<bool>(EventName.UpdateTimeState, timeData.StateIndex <= 1);
    }
    private void SetLight(float intensity,Quaternion rotation,Color color)
    { 
        // ���û����������
        RenderSettings.ambientIntensity = intensity;
        mainLight.intensity = intensity;
        mainLight.transform.rotation = rotation;
        mainLight.color = color;
    }

    /// <summary>
    /// �л���������
    /// </summary>
    private IEnumerator ChangeBGAudio(AudioClip audioClip)
    {
        float old = AudioManager.Instance.BGVolume;
        if (old <= 0) yield break;
        float current = old;
        // ��������
        while (current>0)
        {
            yield return null;
            current -= Time.deltaTime / 2;
            AudioManager.Instance.BGVolume = current;
        }
        AudioManager.Instance.PlayBGAudio(audioClip);
        // �������
        while (current < old)
        {
            yield return null;
            current += Time.deltaTime / 2;
            AudioManager.Instance.BGVolume = current;
        }
        AudioManager.Instance.BGVolume = old;
    }

    private void OnDestroy()
    {
        ArchiveManager.Instance.SaveTimeData();
    }
}
