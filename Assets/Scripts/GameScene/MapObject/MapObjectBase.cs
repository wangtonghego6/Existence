using JKFrame;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// ��ͼ��������
/// </summary>
public enum MapObjectType
{ 
    Tree,
    Stone,
    Bush,
    Material,
    Consumable,
    Weapon,
    Building
}

/// <summary>
/// ��ͼ�������
/// </summary>
public abstract class MapObjectBase : MonoBehaviour
{
    [SerializeField] MapObjectType objectType;
    public MapObjectType ObjectType { get => objectType; }

    // ��������
    [SerializeField] protected float touchDinstance;
    public float TouchDinstance { get => touchDinstance; }

    // �ܷ��ժ
    [SerializeField] protected bool canPickUp;
    public bool CanPickUp { get => canPickUp;}

    [SerializeField] protected int pickUpItemConfigID = -1; // -1��ζ����Ч
    public int PickUpItemConfigID { get => pickUpItemConfigID;}

    protected MapChunkController mapChunk;  // ��ǰ���ڵĵ�ͼ��
    protected ulong id;

    public virtual void Init(MapChunkController mapChunk, ulong id,bool isFormBuild)
    {
        this.mapChunk = mapChunk;
        this.id = id;
    }

    /// <summary>
    /// �ӵ�ͼ���Ƴ�
    /// </summary>
    public virtual void RemoveOnMap()
    {
        mapChunk.RemoveMapObject(id);
    }

    /// <summary>
    /// ����������
    /// </summary>
    public virtual int OnPickUp()
    {
        RemoveOnMap();  // �ӵ�ͼ����ʧ
        return pickUpItemConfigID;
    }

    #region Editor
#if UNITY_EDITOR
    [Sirenix.OdinInspector.Button]
    public void AddNavMeshObstacle()
    {
        NavMeshObstacle obstacle = transform.AddComponent<NavMeshObstacle>();
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
        if (boxCollider != null)
        {
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.center = boxCollider.center;
            obstacle.size = boxCollider.size;
            obstacle.carving = true;
        }
        else if (capsuleCollider != null)
        {
            obstacle.shape = NavMeshObstacleShape.Capsule;
            obstacle.center = capsuleCollider.center;
            obstacle.height = capsuleCollider.height;
            obstacle.radius = capsuleCollider.radius;
            obstacle.carving = true;
        }
    }

#endif
    #endregion
}
