using UnityEngine;

/// <summary>
/// AI的死亡状态
/// </summary>
public class AI_DeadState:AIStateBase
{
    public override void Enter()
    {
        AI.InputCheckCollider.enabled = false;
        AI.PlayAnimation("Dead");
        AI.AddAnimationEvent("DeadOver", DeadOver);
    }

    public override void Exit()
    {
        AI.RemoveAnimationEvent("DeadOver", DeadOver);
        AI.InputCheckCollider.enabled = true;
    }

    private void DeadOver()
    {
        AI.Dead();
    }
}