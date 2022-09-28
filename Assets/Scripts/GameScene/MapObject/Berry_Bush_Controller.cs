using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
using Unity.VisualScripting;
/// <summary>
/// 浆果控制器
/// </summary>
public class Berry_Bush_Controller : Bush_Controller,IBuildingPreview
{
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] Material[] materials;  // 0是有果子 1是没有果子
    [SerializeField] int berryGrowDayNum = 2;   // 浆果成长天数
    private Berry_BushTypeData typeData;
    #region 建筑物
    [SerializeField] new Collider collider;
    public GameObject GameObject => gameObject;
    public Collider Collider => collider;
    private List<Material> materialList;
    public List<Material> MaterialList { get => materialList; set => materialList = value; }
    public void OnPreview()
    {
    }
    #endregion

    public override void Init(MapChunkController mapChunk, ulong id, bool isFormBuild)
    {
        base.Init(mapChunk, id, isFormBuild);
        // 尝试获取类型数据的存档
        if (ArchiveManager.Instance.TryGetMapObjectTypeData(id,out IMapObjectTypeData tempData))
        {
            typeData = tempData as Berry_BushTypeData;
        }
        else
        {
            typeData = new Berry_BushTypeData();
            ArchiveManager.Instance.AddMapObjectTypeData(id, typeData);
        }
        if (isFormBuild)
        {
            // 来自建筑物建造的情况下，直接视为刚刚采摘（这件事情也需要持久化）
            typeData.lastPickUpDayNum = TimeManager.Instance.CurrentDayNum;
        }
        CheckAndSetState();
        EventManager.AddEventListener(EventName.OnMorn, OnMorn);
    }
    public override int OnPickUp()
    {
        // 修改外表
        meshRenderer.sharedMaterial = materials[1];
        canPickUp = false;
        typeData.lastPickUpDayNum = TimeManager.Instance.CurrentDayNum;
        return pickUpItemConfigID;
    }
    private void CheckAndSetState()
    {
        // 有没有采摘过
        if (typeData.lastPickUpDayNum == -1)
        {
            meshRenderer.sharedMaterial = materials[0];
            canPickUp = true;
        }
        else
        {
            // 根据时间决定状态
            if (TimeManager.Instance.CurrentDayNum - typeData.lastPickUpDayNum >= berryGrowDayNum)
            {
                meshRenderer.sharedMaterial = materials[0];
                canPickUp = true;
            }
            else
            {
                meshRenderer.sharedMaterial = materials[1];
                canPickUp = false;
            }
        }
    }
    private void OnMorn()
    {
        // 如果已经成熟，无需检测
        if (canPickUp == false) CheckAndSetState();

    }


}
