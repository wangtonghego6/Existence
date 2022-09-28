using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;

/// <summary>
/// �Ƽ�������
/// </summary>
public class ScienceManager : SingletonMono<ScienceManager>
{
    private ScienceData scienceData;
    public void Init()
    {
        scienceData = ArchiveManager.Instance.ScienceData;
    }
    /// <summary>
    /// ������
    /// </summary>
    public bool CheckUnLock(int ID)
    {
        return scienceData.CheckUnLock(ID);
    }

    /// <summary>
    /// ��ӿƼ�
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
