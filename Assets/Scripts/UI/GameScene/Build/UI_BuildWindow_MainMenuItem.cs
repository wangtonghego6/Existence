using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JKFrame;

/// <summary>
/// UI ���촰�� һ���˵�ѡ��
/// </summary>
public class UI_BuildWindow_MainMenuItem : MonoBehaviour
{
    [SerializeField] Image bgImage;
    [SerializeField] Button button;
    [SerializeField] Image iconImage;
    [SerializeField] Sprite[] bgSprites;    // Ĭ�Ϻ�ѡ��ʱ��ͬ�ľ���ͼ

    public BuildType MenuType { get; private set; }
    private UI_BuildWindow ownerWindow;
    public void Init(BuildType buildType,UI_BuildWindow ownerWindow)
    {
        MenuType = buildType;
        this.ownerWindow = ownerWindow;
        UITool.BindMouseEffect(this);
        button.onClick.AddListener(OnClick);
        OnUnSelect();
    }
    private void OnClick()
    {
        ownerWindow.SelectMainMenuItem(this);
    }

    public void OnSelect()
    {
        bgImage.sprite = bgSprites[1];
    }
    public void OnUnSelect()
    {
        bgImage.sprite = bgSprites[0];
    }
}
