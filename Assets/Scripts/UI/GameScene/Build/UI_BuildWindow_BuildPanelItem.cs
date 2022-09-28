using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JKFrame;

public class UI_BuildWindow_BuildPanelItem : MonoBehaviour
{
    [SerializeField] Image iconImage;
    [SerializeField] Text countText;
    static Color isMeetColor = Color.white;
    static Color noMeetColor = new Color(0.9528f,0.4809f,0.4809f);

    public void Show(int configID, int currCount, int needCount)
    {
        ItemConfig itemConfig = ConfigManager.Instance.GetConfig<ItemConfig>(ConfigName.Item, configID);
        iconImage.sprite = itemConfig.Icon;
        countText.text = currCount.ToString() + "/" + needCount.ToString();
        if (currCount >= needCount) countText.color = isMeetColor;
        else countText.color = noMeetColor;
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

}
