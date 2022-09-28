using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;

/// <summary>
/// 科技管理器
/// </summary>
public class ScienceManager : SingletonMono<ScienceManager>
{
    private ScienceData scienceData;
    public void Init()
    {
        scienceData = ArchiveManager.Instance.ScienceData;
    }
    /// <summary>
    /// 检测解锁
    /// </summary>
    public bool CheckUnLock(int ID)
    {
        return scienceData.CheckUnLock(ID);
    }

    /// <summary>
    /// 添加科技
    /// </summary>
    public void AddScience(int ID)
    {
        scienceData.AddScience(ID);
    }
    private void OnDestroy()
    {
        ArchiveManager.Instance.SaveScienceData();
    }
}
