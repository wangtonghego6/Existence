using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;

/// <summary>
/// ���״̬����
/// ������������״̬����Ҫ��ͬ �ֶΡ�������
/// </summary>
public class PlayerStateBase : StateBase
{
    protected Player_Controller player;

    public override void Init(IStateMachineOwner owner, int stateType, StateMachine stateMachine)
    {
        base.Init(owner, stateType, stateMachine);
        player  = owner as Player_Controller;
    }




}
