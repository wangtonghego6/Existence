using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;

/// <summary>
/// ���������
/// </summary>
public class BuildManager : SingletonMono<BuildManager>
{
    [SerializeField] float virtualCellSize = 0.25f;
    [SerializeField] LayerMask buildLayerMask;
    private Dictionary<string, IBuildingPreview> buildPreviewGameObjectDic = new Dictionary<string, IBuildingPreview>();
    public void Init()
    {
        UIManager.Instance.Show<UI_BuildWindow>();
        EventManager.AddEventListener<BuildConfig>(EventName.BuildBuilding, BuildBuilding);
    }

    private void BuildBuilding(BuildConfig buildConfig)
    {
        StartCoroutine(DoBuildBuilding(buildConfig));
    }

    IEnumerator DoBuildBuilding(BuildConfig buildConfig)
    {
        // ���뽨��״̬
        UIManager.Instance.DisableUIRaycaster();        // �ر�UI����
        InputManager.Instance.SetCheckState(false);
        // ����Ԥ������
        GameObject prefab = ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.MapObject, buildConfig.TargetID).Prefab;
        if (buildPreviewGameObjectDic.TryGetValue(prefab.name,out IBuildingPreview previewBuilding))
        {
            previewBuilding.GameObject.SetActive(true);
        }
        else
        {
            previewBuilding = GameObject.Instantiate(prefab, transform).GetComponent<IBuildingPreview>();
            previewBuilding.InitOnPreview();
            buildPreviewGameObjectDic.Add(prefab.name, previewBuilding);
        }
                
        while (true)
        {
            // ȡ������
            if (Input.GetMouseButtonDown(1))
            {
                previewBuilding.GameObject.SetActive(false);
                UIManager.Instance.EnableUIRaycaster();
                InputManager.Instance.SetCheckState(true);
                yield break;
            }

            // Ԥ������������-������
            if (InputManager.Instance.GetMouseWorldPositionOnGround(Input.mousePosition, out Vector3 mouseWorldPos))
            {
                // ����������ת��Ϊ������ӵ�����
                Vector3 virtualCellPos = mouseWorldPos;
                // Mathf.RoundToInt(mouseWorldPos.x / virtualCellSize) ����ڼ�������
                virtualCellPos.x = Mathf.RoundToInt(mouseWorldPos.x / virtualCellSize) * virtualCellSize;
                virtualCellPos.z = Mathf.RoundToInt(mouseWorldPos.z / virtualCellSize) * virtualCellSize;
                previewBuilding.GameObject.transform.position = virtualCellPos;
            }

            bool isOverlap = true;
            // ��ײ��� 
            if (previewBuilding.Collider is BoxCollider)
            {
                BoxCollider boxCollider = (BoxCollider)previewBuilding.Collider;
                isOverlap = Physics.CheckBox(boxCollider.transform.position + boxCollider.center, boxCollider.size / 2, boxCollider.transform.rotation, buildLayerMask);
            }
            else if (previewBuilding.Collider is CapsuleCollider)
            {
                CapsuleCollider capsuleCollider  = (CapsuleCollider)previewBuilding.Collider;
                Vector3 colliderCenterPos = capsuleCollider.transform.position + capsuleCollider.center;
                Vector3 startPos = colliderCenterPos;
                Vector3 endPos = colliderCenterPos;
                startPos.y = colliderCenterPos.y - (capsuleCollider.height / 2) + capsuleCollider.radius;
                endPos.y = colliderCenterPos.y + (capsuleCollider.height / 2) - capsuleCollider.radius;
                isOverlap = Physics.CheckCapsule(startPos, endPos, capsuleCollider.radius, buildLayerMask);
            }
            else if (previewBuilding.Collider is SphereCollider)
            {
                SphereCollider sphereCollider = (SphereCollider)previewBuilding.Collider;
                isOverlap = Physics.CheckSphere(sphereCollider.transform.position + sphereCollider.center, sphereCollider.radius, buildLayerMask);
            }
            // ������Խ��������Ϊ��ɫ������Ϊ��ɫ
            if (isOverlap)
            {
                previewBuilding.SetColorOnPreview(true);
            }
            else
            {
                previewBuilding.SetColorOnPreview(false);
                // ȷ������
                if (Input.GetMouseButtonDown(0))
                {
                    previewBuilding.GameObject.SetActive(false);
                    UIManager.Instance.EnableUIRaycaster();
                    InputManager.Instance.SetCheckState(true);
                    // ���ý�����
                    MapManager.Instance.SpawnMapObject(buildConfig.TargetID, previewBuilding.GameObject.transform.position,true);
                    // ���ݽ������ã����ٱ����е���Ʒ
                    InventoryManager.Instance.UpdateMainInventoryItemsForBuild(buildConfig);
                    yield break;
                }
            }

            yield return null;
        }
    }
}
