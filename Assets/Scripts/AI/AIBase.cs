using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
using UnityEngine.AI;
using Sirenix.OdinInspector;
using System;
/// <summary>
/// AI�Ļ���
/// </summary>
public abstract class AIBase : SerializedMonoBehaviour,IStateMachineOwner
{
    #region ���
    [SerializeField] protected Animator animator;
    [SerializeField] protected NavMeshAgent navMeshAgent;
    [SerializeField] protected Collider inputCheckCollider;
    [SerializeField] protected Transform weapon;
    public Collider InputCheckCollider { get => inputCheckCollider; }
    public float AttackDinstance { get => attackDinstance; }
    public Transform Weapon { get => weapon; }
    public NavMeshAgent NavMeshAgent { get => navMeshAgent; }
    #endregion
    #region ��ֵ
    [SerializeField] protected MapVertexType mapVertexType;   // AI��ĵ�ͼ���ͷ�Χ
    [SerializeField] protected float maxHP = 10;
    [SerializeField] protected float attackDinstance = 0.5f;
    [SerializeField] protected float attackValue = 10;
    [SerializeField] protected float radius;
    [SerializeField] int lootConfigID = -1; // ����ʱ���������ID
    protected float hp;
    public float AttackValue { get => attackValue; }
    public float Radius { get => radius; }
    #endregion
    #region ����

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
    protected MapChunkController mapChunk;  // ��ǰ���ڵĵ�ͼ��
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
    /// ���Ŷ���
    /// </summary>
    public void PlayAnimation(string animationName, float fixedTime = 0.25f)
    {
        animator.CrossFadeInFixedTime(animationName, fixedTime);
    }

    /// <summary>
    /// ������Ч
    /// </summary>
    public void PlayAudio(string audioName,float volumeScale = 1)
    {
        if (audioDic.TryGetValue(audioName,out AudioClip audioClip))
        {
            AudioManager.Instance.PlayOnShot(audioClip,transform.position,volumeScale);
        }
    }

    /// <summary>
    /// ��ȡAI���Ե����������
    /// </summary>
    /// <returns></returns>
    public Vector3 GetAIRandomPoint()
    {
        return mapChunk.GetAIRandomPoint(mapVertexType);
    }

    /// <summary>
    /// ��������
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
    /// ������������Ϸ��������٣��������Ǵ浵���������
    /// </summary>
    public void Destroy()
    {
        this.JKGameObjectPushPool();
        currentState = AIState.None;
        stateMachine.Stop();
    }
    /// <summary>
    /// ����
    /// ���ݲ��桢��Ϸ���嶼Ҫ����
    /// </summary>
    public void Dead()
    {
        // ��֪��ͼ���Ƴ��Լ�
        mapChunk.RemoveAIObject(aiData.ID);
        // ��Ʒ�����߼�
        if (lootConfigID == -1) return;
        LootConfig lootConfig = ConfigManager.Instance.GetConfig<LootConfig>(ConfigName.Loot, lootConfigID);
        if (lootConfig != null) lootConfig.GeneaterMapObject(mapChunk, transform.position + new Vector3(0, 1, 0));
    }


    #region �����¼�
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
