using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;

/// <summary>
/// ��Ϸ����������
/// </summary>
public class GameSceneManager : LogicManagerBase<GameSceneManager>
{
    #region �����߼�
    public bool IsTest = true;
    public bool IsCreateNewArchive;
    #endregion

    private bool isGameOver = false;
    public bool IsGameOver { get => isGameOver; }

    protected override void CancelEventListener() { }
    protected override void RegisterEventListener() { }

    private void Start()
    {
        #region �����߼�
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
        // ������е������ôһ�����еĴ浵��׼������
        IsInitialized = false;
        // ���ؽ�����
        loadingWindow = UIManager.Instance.Show<UI_GameLoadingWindow>();
        loadingWindow.UpdateProgress(0);

        // ȷ����ͼ��ʼ����������
        MapConfig mapConfig = ConfigManager.Instance.GetConfig<MapConfig>(ConfigName.Map);
        float mapSizeOnWolrd = ArchiveManager.Instance.MapInitData.mapSize * mapConfig.mapChunkSize * mapConfig.cellSize;

        // ��ʾ����Ϣ���:
        // ������TimeManager����Ϣ����
        // ������Player_Controller����Ϣ����
        UIManager.Instance.Show<UI_MainInfoWindow>();

        // ��ʼ����ɫ�����
        Player_Controller.Instance.Init(mapSizeOnWolrd);
        Camera_Controller.Instance.Init(mapSizeOnWolrd);

        // ��ʼ��ʱ��
        TimeManager.Instance.Init();

        // ��ʼ����ͼ
        MapManager.Instance.UpdateViewer(Player_Controller.Instance.transform);
        MapManager.Instance.Init();

        // ��ʼ������
        InventoryManager.Instance.Init();

        // ��ʼ�����������
        InputManager.Instance.Init();

        // ��ʼ�����������
        BuildManager.Instance.Init();

        // ��ʼ���Ƽ�������
        ScienceManager.Instance.Init();

    }

    #region ���ؽ���
    private UI_GameLoadingWindow loadingWindow;
    public bool IsInitialized { get; private set; }

    // ��ʼ����Ҫ�ĵ�ǰ�����ֵ
    private int currentMapChunkCount = 0;
    private int maxMapChunkCount = 0;
    /// <summary>
    /// ���½���
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
