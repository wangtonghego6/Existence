using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JKFrame;
/// <summary>
/// 合成窗口的二级菜单选项
/// </summary>
public class UI_BuildWindow_SecondaryMenuItem : MonoBehaviour
{
    [SerializeField] Image bgImage;
    [SerializeField] Button button;
    [SerializeField] Image iconImage;
    [SerializeField] Sprite[] bgSprites;    // 默认和选中时不同的精灵图

    public BuildConfig BuildConfig { get; private set; }    // 当前选项代表的建造配置
    private UI_BuildWindow_SecondaryMenu ownerWindow;

private void Start()
    {
        UITool.BindMouseEffect(this);
        button.onClick.AddListener(OnClick);
    }

    public void Init(BuildConfig buildConfig, UI_BuildWindow_SecondaryMenu ownerWindow, bool isMeetCondition)
    {
        this.ownerWindow = ownerWindow;
        this.BuildConfig = buildConfig;
        if (buildConfig.BuildType == BuildType.Weapon)
        {
            iconImage.sprite = ConfigManager.Instance.GetConfig<ItemConfig>(ConfigName.Item, buildConfig.TargetID).Icon;
        }
        else
        {
            iconImage.sprite = ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.MapObject, buildConfig.TargetID).MapIconSprite;
        }

        if (isMeetCondition) iconImage.color = Color.white;
        else iconImage.color = Color.black;
        UnSelect();
    }

    private void OnClick()
    {
        ownerWindow.SelectSecondaryMenuItem(this);
    }

    public void Selecte()
    {
        bgImage.sprite = bgSprites[1];
    }
    public void UnSelect()
    { 
        bgImage.sprite = bgSprites[0];
    }
}
