using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JKFrame;
using static UnityEditor.ObjectChangeEventStream;
/// <summary>
/// �ϳɴ��ڵĶ����˵�
/// </summary>
public class UI_BuildWindow_SecondaryMenu : MonoBehaviour
{
    [SerializeField] Transform itemParent;
    [SerializeField] GameObject itemPrefab;
    [SerializeField] UI_BuildWindow_BuildPanel buildPanel;

    private Dictionary<BuildType, List<BuildConfig>> buildConfigDic;    // ���е�buildConfig�������ͷ���
    private UI_BuildWindow_SecondaryMenuItem currentSecondaryMenuItem;  // ��ǰѡ�еĶ����˵�ѡ��
    private List<UI_BuildWindow_SecondaryMenuItem> secondaryMenuItemList;// ��ǰ��Ф�е����ж����˵�ѡ��

    private List<BuildConfig> meetTheConditionList;         // ��������������
    private List<BuildConfig> failToMeetTheConditionList;   // ����������������
    private BuildType currentBuildType;
    public void Init()
    {
        // �����Լ��������ļ��ṹ
        buildConfigDic = new Dictionary<BuildType, List<BuildConfig>>(3);
        buildConfigDic.Add(BuildType.Weapon, new List<BuildConfig>());
        buildConfigDic.Add(BuildType.Building, new List<BuildConfig>());
        buildConfigDic.Add(BuildType.Crop, new List<BuildConfig>());
        Dictionary<int, ConfigBase> buildConfigs = ConfigManager.Instance.GetConfigs(ConfigName.Build);
        foreach (ConfigBase config in buildConfigs.Values)
        {
            BuildConfig buildConfig = (BuildConfig)config;
            buildConfigDic[buildConfig.BuildType].Add(buildConfig);
        }

        secondaryMenuItemList = new List<UI_BuildWindow_SecondaryMenuItem>(10);

        meetTheConditionList = new List<BuildConfig>();
        failToMeetTheConditionList = new List<BuildConfig>();

        // ��ʼ����������
        buildPanel.Init(this);
        Close();
    }

    /// <summary>
    /// ���ݺϳ����� ��ʾ��ͬ�����
    /// </summary>
    public void Show(BuildType buildType)
    {
        this.currentBuildType = buildType;
        // ���б��е�����ѡ���ȷŽ������
        for (int i = 0; i < secondaryMenuItemList.Count; i++)
        {
            secondaryMenuItemList[i].JKGameObjectPushPool();
        }
        secondaryMenuItemList.Clear();
        // ��ǰ���͵������б�
        List<BuildConfig> buildConfigList = buildConfigDic[buildType];

        meetTheConditionList.Clear();
        failToMeetTheConditionList.Clear();

        for (int i = 0; i < buildConfigList.Count; i++)
        {
            // �Ƽ��ж�
            bool scienceUnlock = true;
            if (buildConfigList[i].PreconditionScienceIDList!=null)
            {
                for (int j = 0; j < buildConfigList[i].PreconditionScienceIDList.Count; j++)
                {
                    if (!ScienceManager.Instance.CheckUnLock(buildConfigList[i].PreconditionScienceIDList[j]))
                    {
                        scienceUnlock = false;
                    }
                }
            }

            if (scienceUnlock)
            {
                bool isMeet = buildConfigList[i].CheckBuildConfigCondition();
                if (isMeet) meetTheConditionList.Add(buildConfigList[i]);
                else failToMeetTheConditionList.Add(buildConfigList[i]);
            }
        }

        for (int i = 0; i < meetTheConditionList.Count; i++)
        {
            AddSecondaryMenuItem(meetTheConditionList[i], true);
        }
        for (int i = 0; i < failToMeetTheConditionList.Count; i++)
        {
            AddSecondaryMenuItem(failToMeetTheConditionList[i], false);
        }

        gameObject.SetActive(true);
    }

    public void RefreshView()
    {
        Show(currentBuildType);
        for (int i = 0; i < secondaryMenuItemList.Count; i++)
        {
            if (secondaryMenuItemList[i].BuildConfig == currentSecondaryMenuItem.BuildConfig)
            {
                secondaryMenuItemList[i].Selecte();
            }
        }
    }

    /// <summary>
    /// ���һ�������˵�ѡ��
    /// </summary>
    private void AddSecondaryMenuItem(BuildConfig buildConfig,bool isMeetCondition)
    {
        // �Ӷ�����л�ȡ�˵�ѡ��
        UI_BuildWindow_SecondaryMenuItem menuItem = PoolManager.Instance.GetGameObject<UI_BuildWindow_SecondaryMenuItem>(itemPrefab, itemParent);
        secondaryMenuItemList.Add(menuItem);
        menuItem.Init(buildConfig, this, isMeetCondition);
    }

    /// <summary>
    /// ѡ����ĳ�������˵�ѡ��
    /// </summary>
    public void SelectSecondaryMenuItem(UI_BuildWindow_SecondaryMenuItem newSecondaryMenuItem)
    {
        if (currentSecondaryMenuItem != null) currentSecondaryMenuItem.UnSelect();
        currentSecondaryMenuItem = newSecondaryMenuItem;
        currentSecondaryMenuItem.Selecte();
        // ��ʾ��������
        buildPanel.Show(newSecondaryMenuItem.BuildConfig);
    }

    public void Close()
    {
        // �������ڹر�
        buildPanel.Close();
        gameObject.SetActive(false);
    }
}
