using JKFrame;
using System.Collections;
using UnityEngine;

/// <summary>
/// AI的待机状态
/// </summary>
public class AI_IdleState:AIStateBase
{
    private Coroutine goPatrolCoroutine;
    public override void Enter()
    {
        // 播放待机动画
        AI.PlayAnimation("Idle");
        // 休息一段事件后去巡逻
        goPatrolCoroutine = MonoManager.Instance.StartCoroutine(GoPatrol());
        // 有一定概率发生叫声
        if (Random.Range(0,30) == 0)
        {
            AI.PlayAudio("Idle", 0.5f);
        }
    }


    IEnumerator GoPatrol()
    {
        yield return CoroutineTool.WaitForSeconds(Random.Range(0,6f));  // 随机休息0-6秒
        AI.ChangState(AIState.Patrol);
    }

    public override void Exit()
    {
        if (goPatrolCoroutine!=null)
        {
            MonoManager.Instance.StopCoroutine(goPatrolCoroutine);
            goPatrolCoroutine = null;
        }
    }
}
