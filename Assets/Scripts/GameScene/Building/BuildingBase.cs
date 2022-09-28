using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BuildingBase : MapObjectBase,IBuildingPreview
{
    [SerializeField] protected new Collider collider;
    private List<Material> materialList = null;
    #region 预览模式
    public GameObject GameObject => gameObject;
    public Collider Collider => collider;
    public List<Material> MaterialList { get => materialList; set => materialList = value; }

    public virtual void OnPreview() { }
    #endregion

    #region 运行时

    [SerializeField] List<int> unlockScienceOnBuild;
    /// <summary>
    /// 当建筑物被玩家选择时
    /// </summary>
    public virtual void OnSelect() { }

    public override void Init(MapChunkController mapChunk, ulong id, bool isFormBuild)
    {
        base.Init(mapChunk, id, isFormBuild);
        if (isFormBuild)
        {
            for (int i = 0; i < unlockScienceOnBuild.Count; i++)
            {
                // 同步科技数据
                ScienceManager.Instance.AddScience(unlockScienceOnBuild[i]);
            }
        }
    }


    /// <summary>
    /// 当前物品格子结束拖拽时选中
    /// </summary>
    public virtual bool OnSlotEndDrageSelect(int itemID) { return false; }

    #endregion
}
