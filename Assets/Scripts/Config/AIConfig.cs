using JKFrame;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "��������", menuName = "Config/��������")]
public class AIConfig : ConfigBase
{
    [LabelText("�յ� ������AI")]
    public bool IsEmpty = false;
    [LabelText("���ڵĵ�ͼ��������")]
    public MapVertexType MapVertexType;
    [LabelText("���ɵ�Ԥ����")]
    public GameObject Prefab;
    [LabelText("���ɸ��� Ȩ������")]
    public int Probability;
}
