using JKFrame;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "��ͼ��������", menuName = "Config/��ͼ��������")]
public class MapObjectConfig : ConfigBase
{
    [LabelText("�յ� ��������Ʒ")]
    public bool IsEmpty = false;
    [LabelText("���ڵĵ�ͼ��������")]
    public MapVertexType MapVertexType;
    [LabelText("���ɵ�Ԥ����")]
    public GameObject Prefab;
    [LabelText("Icon")]
    public Sprite MapIconSprite;
    [LabelText("UI��ͼIcon�ߴ�,0��������Icon")]
    public float IconSize = 1;
    [LabelText("��������,-1����������")]
    public int DestoryDays = -1;
    [LabelText("���ɸ��� Ȩ������")]
    public int Probability;
    [LabelText("����"), MultiLineProperty] public string Description;
}
