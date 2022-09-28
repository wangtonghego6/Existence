using JKFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AI×´Ì¬»ùÀà
/// </summary>
public abstract class AIStateBase : StateBase
{
    protected AIBase AI;
    public override void Init(IStateMachineOwner owner, int stateType, StateMachine stateMachine)
    {
        base.Init(owner, stateType, stateMachine);
        AI = owner as AIBase;
    }
}
