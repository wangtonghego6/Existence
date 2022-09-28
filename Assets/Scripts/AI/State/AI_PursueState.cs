using JKFrame;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// AI追击状态
/// </summary>
public class AI_PursueState:AIStateBase
{
    public override void Enter()
    {
        AI.NavMeshAgent.enabled = true;
        AI.PlayAnimation("Move");
        // 添加脚步声事件
        AI.AddAnimationEvent("FootStep", FootStep);
    }

    public override void Update()
    {
        if (GameSceneManager.Instance.IsGameOver == false)
        {
            if (Vector3.Distance(AI.transform.position,Player_Controller.Instance.transform.position) < AI.Radius + AI.AttackDinstance)
            {
                 AI.ChangState(AIState.Attack);
            }
            else
            {
                AI.NavMeshAgent.SetDestination(Player_Controller.Instance.transform.position);
                AI.SavePostion();
                // 检测AI的归属地图块
                CheckAndTransferMapChunk();
            }   
        }
    }

    private void CheckAndTransferMapChunk()
    {
        // 通过AI所在坐标的地图块 和 AI归属的地图块做比较
        MapChunkController newMapChunk = MapManager.Instance.GetMapChunkByWorldPosition(AI.transform.position);
        if (newMapChunk != AI.MapChunk)
        {
            // 从当前地图块移除
            AI.MapChunk.RemoveAIObjectOnTransfer(AI.AiData.ID);
            // 加入新的地图块
            newMapChunk.AddAIObjectFormTransfer(AI.AiData, AI);
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
        AI.PlayAudio("FootStep" + index.ToString(), 0.15f);
    }
}
