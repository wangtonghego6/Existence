using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;

/// <summary>
/// ��Ҵ���״̬
/// </summary>
public class Player_Idle : PlayerStateBase
{
    public override void Enter()
    {
         player.PlayAnimation("Idle");
    }

    public override void Update()
    {
        //  ��������κ��ƶ���صİ���������ȥMove״̬
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        if (h != 0 || v != 0) player.ChangeState(PlayerState.Move);
    }
}
