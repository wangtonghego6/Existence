using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;

/// <summary>
/// ���������
/// </summary>
public class Campfire_Controller : BuildingBase
{
    [SerializeField] new Light light;
    [SerializeField] GameObject fire;
    private CampfireConfig campfireConfig;
    private CampfireData campfireData;
    private bool isOnGround;
    public override void OnPreview()
    {
        isOnGround = false;
        // �ر����Ӻͻ���Ч��
        SetLight(0);
    }
    public override void Init(MapChunkController mapChunk, ulong id, bool isFormBuild)
    {
        base.Init(mapChunk, id, isFormBuild);
        campfireConfig = ConfigManager.Instance.GetConfig<CampfireConfig>(ConfigName.Campfire);
        if (isFormBuild)
        {
            campfireData = new CampfireData();
            campfireData.CurrentFuel = campfireConfig.DefaultFuelValue;
            ArchiveManager.Instance.AddMapObjectTypeData(id, campfireData);
        }
        else
        {
            campfireData = ArchiveManager.Instance.GetMapObjectTypeData(id) as CampfireData;
        }
        SetLight(campfireData.CurrentFuel);
        isOnGround = true;
    }

    private void Update()
    {
        if (GameSceneManager.Instance.IsInitialized == false) return;
        if (isOnGround)
        {
            UpdateFuel();
        }
    }

    private void UpdateFuel()
    {
        if (campfireData.CurrentFuel == 0) return;
        // ����ȼ��
        campfireData.CurrentFuel = Mathf.Clamp(campfireData.CurrentFuel - TimeManager.Instance.timeScale*Time.deltaTime * campfireConfig.BurningSpeed, 0, campfireConfig.MaxFuelValue);
        SetLight(campfireData.CurrentFuel);
    }

    /// <summary>
    /// ����ȼ��
    /// </summary>
    private void SetLight(float fuelValue)
    {
        light.gameObject.SetActive(fuelValue != 0);
        fire.gameObject.SetActive(fuelValue != 0);
        if (fuelValue!=0)
        {
            float value = fuelValue / campfireConfig.MaxFuelValue;
            // ��������
            light.intensity = Mathf.Lerp(0, campfireConfig.MaxLightIntensity, value);
            // ���÷�Χ
            light.range = Mathf.Lerp(0, campfireConfig.MaxLightRange, value);
        }
    }

    public override bool OnSlotEndDrageSelect(int itemID)
    {
        // ľ�ġ�ȼ�ϵ���Ϊȼ����Ʒ
        if (campfireConfig.TryGetFuelValueByItemID(itemID,out float fuelValue))
        {
            campfireData.CurrentFuel = Mathf.Clamp(campfireData.CurrentFuel + fuelValue, 0, campfireConfig.MaxFuelValue);
            SetLight(campfireData.CurrentFuel);
            return true;
        }
        // �決���
        if (campfireConfig.TryGetBakedItemByItemID(itemID,out int bakedItemID))
        {
            // ��û��ȼ��
            if (campfireData.CurrentFuel<=0)
            {
                UIManager.Instance.AddTips("��Ҫ��ȼ����");
                return false;
            }
            // ��������һ����Ʒ
            InventoryManager.Instance.AddMainInventoryItemAndPlayAudio(bakedItemID);
            return true;
        }
        return false;
    }
}
