using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BuildingBase : MapObjectBase,IBuildingPreview
{
    [SerializeField] protected new Collider collider;
    private List<Material> materialList = null;
    #region Ԥ��ģʽ
    public GameObject GameObject => gameObject;
    public Collider Collider => collider;
    public List<Material> MaterialList { get => materialList; set => materialList = value; }

    public virtual void OnPreview() { }
    #endregion

    #region ����ʱ

    [SerializeField] List<int> unlockScienceOnBuild;
    /// <summary>
    /// �������ﱻ���ѡ��ʱ
    /// </summary>
    public virtual void OnSelect() { }

    public override void Init(MapChunkController mapChunk, ulong id, bool isFormBuild)
    {
        base.Init(mapChunk, id, isFormBuild);
        if (isFormBuild)
        {
            for (int i = 0; i < unlockScienceOnBuild.Count; i++)
            {
                // ͬ���Ƽ�����
                ScienceManager.Instance.AddScience(unlockScienceOnBuild[i]);
            }
        }
    }


    /// <summary>
    /// ��ǰ��Ʒ���ӽ�����קʱѡ��
    /// </summary>
    public virtual bool OnSlotEndDrageSelect(int itemID) { return false; }

    #endregion
}
