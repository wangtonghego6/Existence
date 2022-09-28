using JKFrame;
using System.Collections;
using UnityEngine;

/// <summary>
/// AI的待机状态
/// </summary>
public class AI_PatrolState : AIStateBase
{
    private Vector3 target;
    public override void Enter()
    {
        AI.NavMeshAgent.enabled = true;
        target = AI.GetAIRandomPoint();
        AI.PlayAnimation("Move");
        AI.NavMeshAgent.SetDestination(target);
        // 添加脚步声事件
        AI.AddAnimationEvent("FootStep", FootStep);
    }
    public override void Update()
    {
        AI.SavePostion();
        // 检测是否到达目标
        if (Vector3.Distance(AI.transform.position,target)<0.5f)
        {
            AI.ChangState(AIState.Idle);
        }
    }
    public override void Exit()
    {
        AI.NavMeshAgent.enabled = false;
        // 移除脚步声事件
        AI.RemoveAnimationEvent("FootStep", FootStep);
    }

    private void FootStep()
    {
        int index = Random.Range(1, 3);
        AI.PlayAudio("FootStep" + index.ToString(),0.15f);
    }
}