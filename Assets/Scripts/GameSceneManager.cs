using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;

/// <summary>
/// 游戏场景管理器
/// </summary>
public class GameSceneManager : LogicManagerBase<GameSceneManager>
{
    #region 测试逻辑
    public bool IsTest = true;
    public bool IsCreateNewArchive;
    #endregion

    private bool isGameOver = false;
    public bool IsGameOver { get => isGameOver; }

    protected override void CancelEventListener() { }
    protected override void RegisterEventListener() { }

    private void Start()
    {
        #region 测试逻辑
        if (IsTest)
        {
            if (IsCreateNewArchive)
            {
                ArchiveManager.Instance.CreateNewArchive(10, 1, 1, 0.75f);
            }
            else
            {
                ArchiveManager.Instance.LoadCurrentArchive();
            }
        }
        #endregion
        UIManager.Instance.CloseAll();
        StartGame();
    }

    private void StartGame()
    {
        // 如果运行到这里，那么一定所有的存档都准备好了
        IsInitialized = false;
        // 加载进度条
        loadingWindow = UIManager.Instance.Show<UI_GameLoadingWindow>();
        loadingWindow.UpdateProgress(0);

        // 确定地图初始化配置数据
        MapConfig mapConfig = ConfigManager.Instance.GetConfig<MapConfig>(ConfigName.Map);
        float mapSizeOnWolrd = ArchiveManager.Instance.MapInitData.mapSize * mapConfig.mapChunkSize * mapConfig.cellSize;

        // 显示主信息面板:
        // 依赖于TimeManager的信息发送
        // 依赖于Player_Controller的信息发送
        UIManager.Instance.Show<UI_MainInfoWindow>();

        // 初始化角色、相机
        Player_Controller.Instance.Init(mapSizeOnWolrd);
        Camera_Controller.Instance.Init(mapSizeOnWolrd);

        // 初始化时间
        TimeManager.Instance.Init();

        // 初始化地图
        MapManager.Instance.UpdateViewer(Player_Controller.Instance.transform);
        MapManager.Instance.Init();

        // 初始化背包
        InventoryManager.Instance.Init();

        // 初始化输入管理器
        InputManager.Instance.Init();

        // 初始化建造管理器
        BuildManager.Instance.Init();

        // 初始化科技管理器
        ScienceManager.Instance.Init();

    }

    #region 加载进度
    private UI_GameLoadingWindow loadingWindow;
    public bool IsInitialized { get; private set; }

    // 初始化需要的当前和最大值
    private int currentMapChunkCount = 0;
    private int maxMapChunkCount = 0;
    /// <summary>
    /// 更新进度
    /// </summary>
    public void UpdateMapProgress(int current, int max)
    {
        float currentProgress = (100/max)*current;
        if (current == max)
        {
            loadingWindow.UpdateProgress(100);
            IsInitialized = true;
            loadingWindow.Close();
            loadingWindow = null;

        }
        else
        {
            loadingWindow.UpdateProgress(currentProgress);
        }
    }

    public void SetProgressMapChunkCount(int max)
    {
        maxMapChunkCount = max;
    }
    public void OnGenerateMapChunkSucceed()
    {
        currentMapChunkCount++;
        UpdateMapProgress(currentMapChunkCount, maxMapChunkCount);
    }

    #endregion

}
