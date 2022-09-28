using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JKFrame;

[UIElement(false, "UI/UI_BuildWindow",1)]
public class UI_BuildWindow : UI_WindowBase
{
    // 所有的一级菜单选项
    [SerializeField] UI_BuildWindow_MainMenuItem[] mainMenuItems;
    [SerializeField] UI_BuildWindow_SecondaryMenu secondaryMenu;
    private UI_BuildWindow_MainMenuItem currentMenuItem;
    private bool isTouch = false;

    public override void Init()
    {
        // 初始化一级菜单全部选项
        for (int i = 0; i < mainMenuItems.Length; i++)
        {
            mainMenuItems[i].Init((BuildType)i, this);
        }

        secondaryMenu.Init();
    }

    private void Update()
    {
        if (isTouch
            && RectTransformUtility.RectangleContainsScreenPoint(transform as RectTransform, Input.mousePosition) == false)
        {
            isTouch = false;
            CloseMenu();
        }
    }

    private void CloseMenu()
    {
        secondaryMenu.Close();
    }

    /// <summary>
    /// 选择了某个一级菜单选项
    /// </summary>
    public void SelectMainMenuItem(UI_BuildWindow_MainMenuItem newMenuItem)
    {
        if (currentMenuItem != null) currentMenuItem.OnUnSelect();
        currentMenuItem = newMenuItem;
        currentMenuItem.OnSelect();

        // 开启二级菜单
        secondaryMenu.Show(newMenuItem.MenuType);
        isTouch = true;
    }
}
