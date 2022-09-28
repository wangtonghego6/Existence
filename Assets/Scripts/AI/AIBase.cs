using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
using UnityEngine.AI;
using Sirenix.OdinInspector;
using System;
/// <summary>
/// AI的基类
/// </summary>
public abstract class AIBase : SerializedMonoBehaviour,IStateMachineOwner
{
    #region 组件
    [SerializeField] protected Animator animator;
    [SerializeField] protected NavMeshAgent navMeshAgent;
    [SerializeField] protected Collider inputCheckCollider;
    [SerializeField] protected Transform weapon;
    public Collider InputCheckCollider { get => inputCheckCollider; }
    public float AttackDinstance { get => attackDinstance; }
    public Transform Weapon { get => weapon; }
    public NavMeshAgent NavMeshAgent { get => navMeshAgent; }
    #endregion
    #region 数值
    [SerializeField] protected MapVertexType mapVertexType;   // AI活动的地图类型范围
    [SerializeField] protected float maxHP = 10;
    [SerializeField] protected float attackDinstance = 0.5f;
    [SerializeField] protected float attackValue = 10;
    [SerializeField] protected float radius;
    [SerializeField] int lootConfigID = -1; // 死亡时掉落的配置ID
    protected float hp;
    public float AttackValue { get => attackValue; }
    public float Radius { get => radius; }
    #endregion
    #region 数据

    protected AIState currentState;
    protected StateMachine stateMachine;
    public StateMachine StateMachine
    {
        get
        {
            if (stateMachine == null)
            {
                stateMachine = PoolManager.Instance.GetObject<StateMachine>();
                StateMachine.Init(this);
            }
            return stateMachine;
        }
    }
    [SerializeField] protected Dictionary<string, AudioClip> audioDic = new Dictionary<string, AudioClip>();
    protected MapChunkController mapChunk;  // 当前所在的地图块
    public MapChunkController MapChunk { get => mapChunk; }
    private MapObjectData aiData;
    public MapObjectData AiData { get => aiData; }
    #endregion

    public virtual void Init(MapChunkController mapChunk, MapObjectData aiData)
    {
        this.mapChunk = mapChunk;
        this.aiData = aiData;
        transform.position = aiData.Position;
        hp = maxHP;
        ChangState(AIState.Idle);
    }

    public virtual void InitOnTransfer(MapChunkController mapChunk)
    {
        this.mapChunk = mapChunk;
    }

    public virtual void ChangState(AIState state)
    {
        currentState = state;
        switch (state)
        {
            case AIState.Idle:
                StateMachine.ChangeState<AI_IdleState>((int)state);
                break;
            case AIState.Patrol:
                StateMachine.ChangeState<AI_PatrolState>((int)state);
                break;
            case AIState.Hurt:
                StateMachine.ChangeState<AI_HurtState>((int)state);
                break;
            case AIState.Pursue:
                StateMachine.ChangeState<AI_PursueState>((int)state);
                break;
            case AIState.Attack:
                StateMachine.ChangeState<AI_AttackState>((int)state);
                break;
            case AIState.Dead:
                StateMachine.ChangeState<AI_DeadState>((int)state);
                break;
        }
    }


    /// <summary>
    /// 播放动画
    /// </summary>
    public void PlayAnimation(string animationName, float fixedTime = 0.25f)
    {
        animator.CrossFadeInFixedTime(animationName, fixedTime);
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    public void PlayAudio(string audioName,float volumeScale = 1)
    {
        if (audioDic.TryGetValue(audioName,out AudioClip audioClip))
        {
            AudioManager.Instance.PlayOnShot(audioClip,transform.position,volumeScale);
        }
    }

    /// <summary>
    /// 获取AI可以到的随机坐标
    /// </summary>
    /// <returns></returns>
    public Vector3 GetAIRandomPoint()
    {
        return mapChunk.GetAIRandomPoint(mapVertexType);
    }

    /// <summary>
    /// 保存坐标
    /// </summary>
    public void SavePostion()
    {
        aiData.Position = transform.position;
    }

    public virtual void Hurt(float damage)
    {
        if (hp <= 0) return;
        hp -= damage;
        if (hp<=0)
        {
            ChangState(AIState.Dead);
        }
        else
        {
            ChangState(AIState.Hurt);
        }
    }

    /// <summary>
    /// 仅考虑自身游戏物体的销毁，而不考虑存档层面的问题
    /// </summary>
    public void Destroy()
    {
        this.JKGameObjectPushPool();
        currentState = AIState.None;
        stateMachine.Stop();
    }
    /// <summary>
    /// 死亡
    /// 数据层面、游戏物体都要销毁
    /// </summary>
    public void Dead()
    {
        // 告知地图块移除自己
        mapChunk.RemoveAIObject(aiData.ID);
        // 物品掉落逻辑
        if (lootConfigID == -1) return;
        LootConfig lootConfig = ConfigManager.Instance.GetConfig<LootConfig>(ConfigName.Loot, lootConfigID);
        if (lootConfig != null) lootConfig.GeneaterMapObject(mapChunk, transform.position + new Vector3(0, 1, 0));
    }


    #region 动画事件
    private Dictionary<string, Action> animationEventDic = new Dictionary<string, Action>(5);
    private void AnimationEvent(string eventName)
    {
        if (animationEventDic.TryGetValue(eventName,out Action action))
        {
            action?.Invoke();
        }
    }

    public void AddAnimationEvent(string eventName,Action action)
    {
        if (animationEventDic.TryGetValue(eventName,out Action _action))
        {
            _action += action;
        }
        else
        {
            animationEventDic.Add(eventName,action);
        }
    }

    public void RemoveAnimationEvent(string eventName,Action action)
    {
        if (animationEventDic.TryGetValue(eventName, out Action _action))
        {
            _action -= action;
        }
    }

    public void RemoveAnimationEvent(string eventName)
    {
        animationEventDic.Remove(eventName);
    }

    public void CleanAllAnimationEvent()
    {
        animationEventDic.Clear();
    }

    #endregion
}
