using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
using Unity.VisualScripting;
/// <summary>
/// ����������
/// </summary>
public class Berry_Bush_Controller : Bush_Controller,IBuildingPreview
{
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] Material[] materials;  // 0���й��� 1��û�й���
    [SerializeField] int berryGrowDayNum = 2;   // �����ɳ�����
    private Berry_BushTypeData typeData;
    #region ������
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
        // ���Ի�ȡ�������ݵĴ浵
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
            // ���Խ����ｨ�������£�ֱ����Ϊ�ող�ժ���������Ҳ��Ҫ�־û���
            typeData.lastPickUpDayNum = TimeManager.Instance.CurrentDayNum;
        }
        CheckAndSetState();
        EventManager.AddEventListener(EventName.OnMorn, OnMorn);
    }
    public override int OnPickUp()
    {
        // �޸����
        meshRenderer.sharedMaterial = materials[1];
        canPickUp = false;
        typeData.lastPickUpDayNum = TimeManager.Instance.CurrentDayNum;
        return pickUpItemConfigID;
    }
    private void CheckAndSetState()
    {
        // ��û�в�ժ��
        if (typeData.lastPickUpDayNum == -1)
        {
            meshRenderer.sharedMaterial = materials[0];
            canPickUp = true;
        }
        else
        {
            // ����ʱ�����״̬
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
        // ����Ѿ����죬������
        if (canPickUp == false) CheckAndSetState();

    }


}
