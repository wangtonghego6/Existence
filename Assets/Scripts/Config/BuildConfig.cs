using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
using Sirenix.OdinInspector;
[CreateAssetMenu(fileName ="����ϳ�����",menuName ="Config/������ϳ�����")]
public class BuildConfig : ConfigBase
{
    [LabelText("�ϳ�����")] public BuildType BuildType;
    [LabelText("ǰ�ÿƼ�")] public List<int> PreconditionScienceIDList;
    [LabelText("�ϳ�����")] public List<BuildConfigCondition> BuildConfigConditionList = new List<BuildConfigCondition>();
    [LabelText("�ϳɲ���")] public int TargetID;

    public bool CheckBuildConfigCondition()
    {
        for (int j = 0; j < BuildConfigConditionList.Count; j++)
        {
            int currentCount = InventoryManager.Instance.GetMainInventoryItemCount(BuildConfigConditionList[j].ItemId);
            // ��鵱ǰ�����Ƿ������������
            if (currentCount < BuildConfigConditionList[j].Count)
            {
                return false;
            }
        }
        return true;
    }
}

public class BuildConfigCondition
{
    [LabelText("��ƷID"),HorizontalGroup] public int ItemId;
    [LabelText("��Ʒ����"), HorizontalGroup] public int Count;
}
