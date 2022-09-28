using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
using System;

/// <summary>
/// 玩家状态
/// </summary>
public enum PlayerState
{ 
    Idle,
    Move,
    Attack,
    BeAttack,
    Dead
}
/// <summary>
/// 玩家控制器
/// </summary>
public class Player_Controller : SingletonMono<Player_Controller>,IStateMachineOwner
{
    [SerializeField] Player_Model player_Model;
    [SerializeField] Animator animator;
    public CharacterController characterController;

    private StateMachine stateMachine;
    public Transform playerTransform { get; private set; }

    private PlayerConfig playerConfig;
    public float rotateSpeed { get=> playerConfig.RotateSpeed; } 
    public float moveSpeed { get=> playerConfig.MoveSpeed;  }
    public Vector2 positionXScope { get; private set; }// X的范围
    public Vector2 positionZScope { get; private set; }// Z的范围
    public bool CanUseItem { get; private set; } = true;    // 当前是否可以使用物品，包括消耗品和武器

    #region 存档相关的数据
    private PlayerTransformData playerTransformData;
    private PlayerMainData playerMainData;
    #endregion

    #region 初始化
    public void Init(float mapSizeOnWorld)
    {
        // 确定配置
        playerConfig = ConfigManager.Instance.GetConfig<PlayerConfig>(ConfigName.Player);
        // 确定存档
        playerTransformData = ArchiveManager.Instance.PlayerTransformData;
        playerMainData = ArchiveManager.Instance.PlayerMainData;

        player_Model.Init(PlayAudioOnFootstep,OnStartHit,OnStopHit,OnAttackOver);
        playerTransform = transform;

        stateMachine = ResManager.Load<StateMachine>();
        stateMachine.Init(this);
        // 初始状态为待机
        stateMachine.ChangeState<Player_Idle>((int)PlayerState.Idle);
        InitPositionScope(mapSizeOnWorld);

        // 初始化存档相关的数据
        playerTransform.localPosition = playerTransformData.Position;
        playerTransform.localRotation = Quaternion.Euler(playerTransformData.Rotation);

        // 触发初始化相关事件
        TriggerUpdateHPEvent();
        TriggerUpdateHungryEvent();
    }

    // 初始化坐标范围
    private void InitPositionScope(float mapSizeOnWorld)
    {
        positionXScope = new Vector2(1, mapSizeOnWorld - 1);
        positionZScope = new Vector2(1, mapSizeOnWorld - 1);
    }
    #endregion

    #region 辅助/工具函数
    private void PlayAudioOnFootstep(int index)
    {
        AudioManager.Instance.PlayOnShot(playerConfig.FootstepAudioClis[index], playerTransform.position, 0.2f);
    }
    /// <summary>
    /// 修改状态
    /// </summary>
    public void ChangeState(PlayerState playerState)
    {
        switch (playerState)
        {
            case PlayerState.Idle:
                stateMachine.ChangeState<Player_Idle>((int)playerState);
                break;
            case PlayerState.Move:
                stateMachine.ChangeState<Player_Move>((int)playerState);
                break;
            case PlayerState.Attack:
                stateMachine.ChangeState<Player_Attack>((int)playerState);
                break;
            case PlayerState.BeAttack:
                break;
            case PlayerState.Dead:
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

    #endregion

    #region 核心数值
    // 计算饥饿值
    private void CalculateHungryOnUpdate()
    {
        // 有饥饿值就掉饥饿值，没有则减少生命值
        if (playerMainData.Hungry > 0)
        {
            playerMainData.Hungry -= Time.deltaTime * playerConfig.HungryReduceSeed;
            if (playerMainData.Hungry < 0) playerMainData.Hungry = 0;
            // 更新UI
            TriggerUpdateHungryEvent();
        }
        else
        {
            if (playerMainData.Hp > 0)
            {
                playerMainData.Hp -= Time.deltaTime * playerConfig.HpReduceSpeedOnHungryIsZero;
                if (playerMainData.Hp < 0) playerMainData.Hp = 0;
                TriggerUpdateHPEvent();
                // TODO:玩家已经死亡
            }
        }
    }

    private void TriggerUpdateHPEvent()
    {
        EventManager.EventTrigger(EventName.UpdatePlayerHP, playerMainData.Hp);
    }
    private void TriggerUpdateHungryEvent()
    {
        EventManager.EventTrigger(EventName.UpdatePlayerHungry, playerMainData.Hungry);

    }

    /// <summary>
    /// 恢复生命值
    /// </summary>
    public void RecoverHP(float value)
    {
        playerMainData.Hp = Mathf.Clamp(playerMainData.Hp + value, 0, playerConfig.MaxHp);
        TriggerUpdateHPEvent();
    }

    /// <summary>
    /// 恢复饥饿值
    /// </summary>
    public void RecoverHungry(float value)
    {
        playerMainData.Hungry = Mathf.Clamp(playerMainData.Hungry + value, 0, playerConfig.MaxHungry);
        TriggerUpdateHungryEvent();
    }
    #endregion

    #region 武器相关
    private ItemData currentWeaponItemData;
    private GameObject currentWeaponGameObject;

    /// <summary>
    /// 修改武器
    /// </summary>
    public void ChangeWeapon(ItemData newWeapon)
    {
        // 压根没切换武器
        if (currentWeaponItemData == newWeapon)
        {
            currentWeaponItemData = newWeapon;
            return;
        }

        // 旧武器如果有数据，把旧武器模型回收到对象池
        if (currentWeaponItemData!=null)
        {
            // 塞进对象池时，是基于GameObject.name的，所以不能同名
            currentWeaponGameObject.JKGameObjectPushPool(); 
        }

        // 新武器如果不是Null
        if (newWeapon!=null)
        {
            ItemWeaponInfo newWeaponInfo = newWeapon.Config.ItemTypeInfo as ItemWeaponInfo;
            currentWeaponGameObject = PoolManager.Instance.GetGameObject(newWeaponInfo.PrefabOnPlayer,player_Model.WeaponRoot);
            currentWeaponGameObject.transform.localPosition = newWeaponInfo.PositionOnPlayer;
            currentWeaponGameObject.transform.localRotation = Quaternion.Euler(newWeaponInfo.RotationOnPlayer);
            animator.runtimeAnimatorController = newWeaponInfo.AnimatorController;
        }
        // 新武器是Null，意味着空手
        else
        {
            animator.runtimeAnimatorController = playerConfig.NormalAnimatorController;
        }
        // 由于动画是逻辑状态机驱动 如果不重新激活一次动画，动画会错误（比如在移动中，突然切换AnimatorController会不播放走路动画）
        stateMachine.ChangeState<Player_Idle>((int)PlayerState.Idle, true);
        currentWeaponItemData = newWeapon;
    }

    #endregion

    #region 战斗、伐木采摘
    private bool canAttack = true;
    public Quaternion attackDir { get; private set; }
    private List<MapObjectBase> lastAttackedMaoObjectList = new List<MapObjectBase>();

    // 最后攻击的地图对象
    private MapObjectBase lastHitMapObject;
    /// <summary>
    /// 当选择地图对象或者AI时
    /// </summary>
    public void OnSelectMapObject(RaycastHit hitInfo,bool isMouseButtonDown)
    {
        if (hitInfo.collider.TryGetComponent<MapObjectBase>(out MapObjectBase mapObject))
        {
            float dis = Vector3.Distance(playerTransform.position, mapObject.transform.position);
            // 说明不在交互范围内
            if (dis > mapObject.TouchDinstance)
            {
                if (isMouseButtonDown)
                {
                    UIManager.Instance.AddTips("离近一点哦！");
                    ProjectTool.PlayAudio(AudioType.Fail);
                }
                return;
            }
            // 判断拾取
            if (mapObject.CanPickUp)
            {
                if (!isMouseButtonDown) return;
                lastHitMapObject = null;
                // 获取捡到的物品
                int itemConfigID = mapObject.PickUpItemConfigID;
                if (itemConfigID != -1)
                {
                    // 背包里面如果数据添加成功，则销毁地图物体
                    if (InventoryManager.Instance.AddMainInventoryItem(itemConfigID))
                    {
                        mapObject.OnPickUp();
                        // 播放捡东西起来的动画 这里没有切换状态，依然是Idle状态
                        PlayAnimation("PickUp");
                        ProjectTool.PlayAudio(AudioType.Bag);
                    }
                    else
                    {
                        if (isMouseButtonDown)
                        {
                            UIManager.Instance.AddTips("背包已经满了！");
                            ProjectTool.PlayAudio(AudioType.Fail);
                        }
                    }
                }
                return;
            }
            if (!canAttack) return;
            // 现在交互的对象不是上一个对象，新对象只接受鼠标单击时，但是如果是重复对一个对象进行操作，则允许保持按下状态进行交互
            if (lastHitMapObject != mapObject && !isMouseButtonDown) return;
            lastHitMapObject = mapObject;
            // 判断攻击
            // 根据玩家选中的地图对象类型以及当前角色的武器来判断做什么
            switch (mapObject.ObjectType)
            {
                case MapObjectType.Tree:
                    if (!CheckHitMapObject(mapObject, WeaponType.Axe) && isMouseButtonDown)
                    {
                        UIManager.Instance.AddTips("你需要装备斧头！");
                        ProjectTool.PlayAudio(AudioType.Fail);
                    }
                    break;
                case MapObjectType.Stone:
                    if (!CheckHitMapObject(mapObject, WeaponType.PickAxe) && isMouseButtonDown)
                    {
                        UIManager.Instance.AddTips("你需要装备镐！");
                        ProjectTool.PlayAudio(AudioType.Fail);
                    }
                    break;
                case MapObjectType.Bush:
                    if (!CheckHitMapObject(mapObject, WeaponType.Sickle) && isMouseButtonDown)
                    {
                        UIManager.Instance.AddTips("你需要装备镰刀！");
                        ProjectTool.PlayAudio(AudioType.Fail);
                    }
                    break;
            }
            return;
        }

        // 攻击AI的逻辑
        if (canAttack&&currentWeaponItemData != null && hitInfo.collider.TryGetComponent<AIBase>(out AIBase aiObject))
        {
            float dis = Vector3.Distance(playerTransform.position, aiObject.transform.position);
            ItemWeaponInfo itemWeaponInfo = currentWeaponItemData.Config.ItemTypeInfo as ItemWeaponInfo;
            // 交互距离：武器的长度 + AI的半径
            if (dis< itemWeaponInfo.AttackDistance + aiObject.Radius)
            {
                // 防止立刻又进行攻击
                canAttack = false;
                // 计算方向
                attackDir = Quaternion.LookRotation(aiObject.transform.position - transform.position);
                // 播放攻击音效
                AudioManager.Instance.PlayOnShot(itemWeaponInfo.AttackAudio,transform.position);
                // 切换状态
                ChangeState(PlayerState.Attack);
                // 禁止使用物品
                CanUseItem = false;
            }
        }
    }

    // 检测击打地图物体
    public bool CheckHitMapObject(MapObjectBase mapObject, WeaponType weaponType)
    {
        if (currentWeaponItemData == null) return false;

        ItemWeaponInfo itemWeaponInfo = currentWeaponItemData.Config.ItemTypeInfo as ItemWeaponInfo;
        // 有武器且是武器符合要求
        if (itemWeaponInfo.WeaponType == weaponType)
        {
            // 防止立刻又进行攻击
            canAttack = false;
            // 计算方向
            attackDir = Quaternion.LookRotation(mapObject.transform.position - transform.position);
            // 播放攻击音效
            AudioManager.Instance.PlayOnShot(itemWeaponInfo.AttackAudio, transform.position);
            // 切换状态
            ChangeState(PlayerState.Attack);
            // 禁止使用物品
            CanUseItem = false;
            return true;
        }
        return false;
    }

    // 让武器开启伤害检测
    private void OnStartHit()
    {
        attackSucceedCount = 0;
        currentWeaponGameObject.transform.OnTriggerEnter(OnWeaponTriggerEnter);
    }

    // 让武器停止伤害检测
    private void OnStopHit()
    {
        currentWeaponGameObject.transform.RemoveTriggerEnter(OnWeaponTriggerEnter);
        lastAttackedMaoObjectList.Clear();
    }

    // 整个攻击状态的结束
    private void OnAttackOver()
    {
        // 成功命中过几次，就消耗几次耐久度
        for (int i = 0; i < attackSucceedCount; i++)
        {
            // 让武器耗损
            EventManager.EventTrigger(EventName.PlayerWeaponAttackSucceed);
        }

        canAttack = true;
        // 切换状态到待机
        ChangeState(PlayerState.Idle);
        // 允许使用物品
        CanUseItem = true;
    }
    // 攻击成功的数量
    private int attackSucceedCount;
    // 当武器碰到其他游戏物体时
    private void OnWeaponTriggerEnter(Collider other, object[] arg2)
    {
        // 对方得是地图对象才有意义
        if (other.TryGetComponent<HitMapObjectBase>(out HitMapObjectBase mapObject))
        {
            // 已经攻击过的，防止二次伤害
            if (lastAttackedMaoObjectList.Contains(mapObject)) return;
            lastAttackedMaoObjectList.Add(mapObject);
            // 检测对方是什么类型 以及 自己手上是什么武器
            switch (mapObject.ObjectType)
            {
                case MapObjectType.Tree:
                    CheckMapObjectHurt(mapObject, WeaponType.Axe);
                    break;
                case MapObjectType.Stone:
                    CheckMapObjectHurt(mapObject, WeaponType.PickAxe);
                    break;
                case MapObjectType.Bush:
                    CheckMapObjectHurt(mapObject, WeaponType.Sickle);
                    break;
            }
        }
        else if (other.TryGetComponent<AIBase>(out AIBase aiObject))
        {
            ItemWeaponInfo itemWeaponInfo = currentWeaponItemData.Config.ItemTypeInfo as ItemWeaponInfo;
            AudioManager.Instance.PlayOnShot(itemWeaponInfo.HitAudio, transform.position);
            aiObject.Hurt(itemWeaponInfo.AttackValue);
            attackSucceedCount += 1;
        }
    }

    /// <summary>
    /// 检查地图对象能否受伤
    /// </summary>
    private void CheckMapObjectHurt(HitMapObjectBase hitMapObject, WeaponType weaponType)
    {
        ItemWeaponInfo itemWeaponInfo = currentWeaponItemData.Config.ItemTypeInfo as ItemWeaponInfo;
        if (itemWeaponInfo.WeaponType == weaponType)
        {
            // 让树受伤
            AudioManager.Instance.PlayOnShot(itemWeaponInfo.HitAudio, transform.position);
            hitMapObject.Hurt(itemWeaponInfo.AttackValue);
            attackSucceedCount += 1;
        }
    }


    public void Hurt(float damage)
    {
        Debug.Log("玩家受伤:"+damage);
    }

    #endregion

    private void Update()
    {
        if (GameSceneManager.Instance.IsInitialized == false) return;
        CalculateHungryOnUpdate();
    }

    private void OnDestroy()
    {
        // 把存档数据实际写入磁盘
        playerTransformData.Position = playerTransform.localPosition;
        playerTransformData.Rotation = playerTransform.localRotation.eulerAngles;
        ArchiveManager.Instance.SavePlayerTransformData();
        ArchiveManager.Instance.SavePlayerMainData();
    }
}
