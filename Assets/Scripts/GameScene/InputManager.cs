using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
using UnityEngine.EventSystems;
/// <summary>
/// ���������
/// </summary>
public class InputManager : SingletonMono<InputManager>
{
    private List<RaycastResult> raycastResultList = new List<RaycastResult>();
    [SerializeField] LayerMask bigMapObjectLayer;       // ���͵�ͼ�����
    [SerializeField] LayerMask mouseMapObjectLayer;     // ��꽻����ͼ�����
    [SerializeField] LayerMask groundLayer;             // �����
    [SerializeField] LayerMask buildingLayer;           // �������
    private bool wantCheck = false;                     // ��Ҫ���

    /// <summary>
    /// ��ʼ��
    /// </summary>
    public void Init()
    {
        SetCheckState(true);
    }
    private void Update()
    {
        if (GameSceneManager.Instance.IsInitialized == false) return;
        CheckSelectMapObject();
    }
    public void SetCheckState(bool wantCheck)
    {
        this.wantCheck = wantCheck;
    }

    /// <summary>
    /// ���ѡ�е�ͼ����
    /// </summary>
    private void CheckSelectMapObject()
    {
        if (wantCheck == false) return;
        bool mousetButtonDown = Input.GetMouseButtonDown(0);
        bool mousetButton = Input.GetMouseButton(0);

        if (mousetButtonDown || mousetButton)
        {
            // �����⵽UI������
            if (CheckMouseOnUI()) return;
            // ���߼���ͼ�ϵ�3D����
            Ray ray = Camera_Controller.Instance.Camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray,out RaycastHit hitInfo,100,mouseMapObjectLayer))
            {
                // ����Player_Controllerȥ����
                Player_Controller.Instance.OnSelectMapObject(hitInfo, mousetButtonDown);
            }
            // ���������߼�
            if (mousetButtonDown&&Physics.Raycast(ray, out hitInfo, 100, buildingLayer))
            {
                BuildingBase building = hitInfo.collider.GetComponent<BuildingBase>();

                // ������ֵ��Ч��
                if (building.TouchDinstance > 0)
                {
                    // �������֤
                    if (Vector3.Distance(Player_Controller.Instance.transform.position, building.transform.position) < building.TouchDinstance)
                    {
                        building.OnSelect();
                    }
                    else
                    {
                        UIManager.Instance.AddTips("���һ�㣡");
                        ProjectTool.PlayAudio(AudioType.Fail);
                    }
                }
            }
        }
    }

    /// <summary>
    /// �������Ƿ���UI��
    /// </summary>
    /// <returns></returns>
    public bool CheckMouseOnUI()
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;
        // ����ȥ�����û�г���Mask������κ�UI����
        EventSystem.current.RaycastAll(pointerEventData, raycastResultList);
        for (int i = 0; i < raycastResultList.Count; i++)
        {
            RaycastResult raycastResult = raycastResultList[i];
            // ��UI��ͬʱ������Mask���õ�����
            if (raycastResult.gameObject.TryGetComponent<RectTransform>(out var _temp1) && raycastResult.gameObject.name != "Mask")
            {
                raycastResultList.Clear();
                return true;
            }
        }
        raycastResultList.Clear();
        return false;
    }


    /// <summary>
    /// �������Ƿ��ڽϴ�ĵ�ͼ������
    /// </summary>
    public bool CheckMouseOnBigMapObject()
    {
        return Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), 1000, bigMapObjectLayer);
    }

    /// <summary>
    /// ��ȡ����ڵ����ϵ���������
    /// </summary>
    /// <returns></returns>
    public bool GetMouseWorldPositionOnGround(Vector3 mousePos,out Vector3 worldPos)
    {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(mousePos),out RaycastHit hitInfo,1000, groundLayer))
        {
            worldPos = hitInfo.point;
            return true;
        }
        worldPos = Vector3.zero;
        return false;
    }

    /// <summary>
    /// ��鵱����ֹͣ��קʱ �Ƿ��ڽ�������
    /// </summary>
    public bool CheckSlotEndDragOnBuilding(int itemID)
    {
        Ray ray = Camera_Controller.Instance.Camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 100, buildingLayer))
        {
            BuildingBase building = hitInfo.collider.GetComponent<BuildingBase>();
            return building.OnSlotEndDrageSelect(itemID);
        }
        return false;
    }
         
}
