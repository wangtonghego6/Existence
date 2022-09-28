using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JKFrame;

public class UI_BuildWindow_BuildPanel : MonoBehaviour
{
    [SerializeField] UI_BuildWindow_BuildPanelItem[] buildPanelItems;
    [SerializeField] Text descriptionText;
    [SerializeField] Button buildButton;
    private BuildConfig buildConfig;
    private UI_BuildWindow_SecondaryMenu ownerWindow;

    public void Init(UI_BuildWindow_SecondaryMenu ownerWindow)
    {
        this.ownerWindow = ownerWindow;
        buildButton.onClick.AddListener(BuildButtonClick);
        Close();
    }
    private void BuildButtonClick()
    {
        if (buildConfig.BuildType == BuildType.Weapon)
        {
            if (InventoryManager.Instance.AddMainInventoryItemAndPlayAudio(buildConfig.TargetID))
            {
                // ���ݽ������ã����ٱ����е���Ʒ
                InventoryManager.Instance.UpdateMainInventoryItemsForBuild(buildConfig);
                // ˢ�µ�ǰ����״̬
                RefreshView();
            }
            else UIManager.Instance.AddTips("�����������޷�����!");
        }
        else
        {
            // ���뽨��ģʽ
            EventManager.EventTrigger<BuildConfig>(EventName.BuildBuilding, buildConfig);
            ownerWindow.Close();
        }


    }

    public void Show(BuildConfig buildConfig)
    { 
        this.buildConfig = buildConfig;
        // ��ʾ��ȷ�ĺϳ���Ҫ����Ʒ
        // Լ�����Ϊ3��
        for (int i = 0; i < buildConfig.BuildConfigConditionList.Count; i++)
        {
            int id = buildConfig.BuildConfigConditionList[i].ItemId;
            int currentCount = InventoryManager.Instance.GetMainInventoryItemCount(id);
            int needCount = buildConfig.BuildConfigConditionList[i].Count;
            buildPanelItems[i].Show(id, currentCount, needCount);
        }
        if (buildConfig.BuildType == BuildType.Weapon)
        {
            descriptionText.text = ConfigManager.Instance.GetConfig<ItemConfig>(ConfigName.Item, buildConfig.TargetID).Description;
        }
        else
        {
            descriptionText.text = ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.MapObject, buildConfig.TargetID).Description;
        }
        buildButton.interactable = buildConfig.CheckBuildConfigCondition();
        gameObject.SetActive(true);
    }

    public void RefreshView()
    {
        Show(buildConfig);
        ownerWindow.RefreshView();
    }

    public void Close()
    {
        for (int i = 0; i < buildPanelItems.Length; i++)
        {
            buildPanelItems[i].Close();
        }

        gameObject.SetActive(false);
    }
}
