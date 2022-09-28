using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��ѧ���� ������
/// </summary>
public class ScienceMachine_Controller : BuildingBase
{
    public override void Init(MapChunkController mapChunk, ulong id, bool isFormBuild)
    {
        base.Init(mapChunk, id, isFormBuild);
        if (isFormBuild)
        {
            // ֻ�н���ɹ�ʱ����ͬ���Ƽ�����
            ScienceManager.Instance.AddScience(27);
        }
    }
}
