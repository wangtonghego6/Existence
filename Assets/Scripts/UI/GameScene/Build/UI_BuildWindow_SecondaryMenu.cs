using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JKFrame;
using static UnityEditor.ObjectChangeEventStream;
/// <summary>
/// 合成窗口的二级菜单
/// </summary>
public class UI_BuildWindow_SecondaryMenu : MonoBehaviour
{
    [SerializeField] Transform itemParent;
    [SerializeField] GameObject itemPrefab;
    [SerializeField] UI_BuildWindow_BuildPanel buildPanel;

    private Dictionary<BuildType, List<BuildConfig>> buildConfigDic;    // 所有的buildConfig按照类型分类
    private UI_BuildWindow_SecondaryMenuItem currentSecondaryMenuItem;  // 当前选中的二级菜单选项
    private List<UI_BuildWindow_SecondaryMenuItem> secondaryMenuItemList;// 当前生肖中的所有二级菜单选项

    private List<BuildConfig> meetTheConditionList;         // 满足条件的配置
    private List<BuildConfig> failToMeetTheConditionList;   // 不满足条件的配置
    private BuildType currentBuildType;
    public void Init()
    {
        // 构建自己的配置文件结构
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

        // 初始化三级窗口
        buildPanel.Init(this);
        Close();
    }

    /// <summary>
    /// 根据合成类型 显示不同的泪飙
    /// </summary>
    public void Show(BuildType buildType)
    {
        this.currentBuildType = buildType;
        // 旧列表中的所有选项先放进对象池
        for (int i = 0; i < secondaryMenuItemList.Count; i++)
        {
            secondaryMenuItemList[i].JKGameObjectPushPool();
        }
        secondaryMenuItemList.Clear();
        // 当前类型的配置列表
        List<BuildConfig> buildConfigList = buildConfigDic[buildType];

        meetTheConditionList.Clear();
        failToMeetTheConditionList.Clear();

        for (int i = 0; i < buildConfigList.Count; i++)
        {
            // 科技判断
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
    /// 添加一个二级菜单选项
    /// </summary>
    private void AddSecondaryMenuItem(BuildConfig buildConfig,bool isMeetCondition)
    {
        // 从对象池中获取菜单选项
        UI_BuildWindow_SecondaryMenuItem menuItem = PoolManager.Instance.GetGameObject<UI_BuildWindow_SecondaryMenuItem>(itemPrefab, itemParent);
        secondaryMenuItemList.Add(menuItem);
        menuItem.Init(buildConfig, this, isMeetCondition);
    }

    /// <summary>
    /// 选中了某个二级菜单选项
    /// </summary>
    public void SelectSecondaryMenuItem(UI_BuildWindow_SecondaryMenuItem newSecondaryMenuItem)
    {
        if (currentSecondaryMenuItem != null) currentSecondaryMenuItem.UnSelect();
        currentSecondaryMenuItem = newSecondaryMenuItem;
        currentSecondaryMenuItem.Selecte();
        // 显示三级窗口
        buildPanel.Show(newSecondaryMenuItem.BuildConfig);
    }

    public void Close()
    {
        // 三级窗口关闭
        buildPanel.Close();
        gameObject.SetActive(false);
    }
}
