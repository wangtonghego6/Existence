using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
using System;

/// <summary>
/// ���״̬
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
/// ��ҿ�����
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
    public Vector2 positionXScope { get; private set; }// X�ķ�Χ
    public Vector2 positionZScope { get; private set; }// Z�ķ�Χ
    public bool CanUseItem { get; private set; } = true;    // ��ǰ�Ƿ����ʹ����Ʒ����������Ʒ������

    #region �浵��ص�����
    private PlayerTransformData playerTransformData;
    private PlayerMainData playerMainData;
    #endregion

    #region ��ʼ��
    public void Init(float mapSizeOnWorld)
    {
        // ȷ������
        playerConfig = ConfigManager.Instance.GetConfig<PlayerConfig>(ConfigName.Player);
        // ȷ���浵
        playerTransformData = ArchiveManager.Instance.PlayerTransformData;
        playerMainData = ArchiveManager.Instance.PlayerMainData;

        player_Model.Init(PlayAudioOnFootstep,OnStartHit,OnStopHit,OnAttackOver);
        playerTransform = transform;

        stateMachine = ResManager.Load<StateMachine>();
        stateMachine.Init(this);
        // ��ʼ״̬Ϊ����
        stateMachine.ChangeState<Player_Idle>((int)PlayerState.Idle);
        InitPositionScope(mapSizeOnWorld);

        // ��ʼ���浵��ص�����
        playerTransform.localPosition = playerTransformData.Position;
        playerTransform.localRotation = Quaternion.Euler(playerTransformData.Rotation);

        // ������ʼ������¼�
        TriggerUpdateHPEvent();
        TriggerUpdateHungryEvent();
    }

    // ��ʼ�����귶Χ
    private void InitPositionScope(float mapSizeOnWorld)
    {
        positionXScope = new Vector2(1, mapSizeOnWorld - 1);
        positionZScope = new Vector2(1, mapSizeOnWorld - 1);
    }
    #endregion

    #region ����/���ߺ���
    private void PlayAudioOnFootstep(int index)
    {
        AudioManager.Instance.PlayOnShot(playerConfig.FootstepAudioClis[index], playerTransform.position, 0.2f);
    }
    /// <summary>
    /// �޸�״̬
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
    /// ���Ŷ���
    /// </summary>
    public void PlayAnimation(string animationName, float fixedTime = 0.25f)
    {
        animator.CrossFadeInFixedTime(animationName, fixedTime);
    }

    #endregion

    #region ������ֵ
    // ���㼢��ֵ
    private void CalculateHungryOnUpdate()
    {
        // �м���ֵ�͵�����ֵ��û�����������ֵ
        if (playerMainData.Hungry > 0)
        {
            playerMainData.Hungry -= Time.deltaTime * playerConfig.HungryReduceSeed;
            if (playerMainData.Hungry < 0) playerMainData.Hungry = 0;
            // ����UI
            TriggerUpdateHungryEvent();
        }
        else
        {
            if (playerMainData.Hp > 0)
            {
                playerMainData.Hp -= Time.deltaTime * playerConfig.HpReduceSpeedOnHungryIsZero;
                if (playerMainData.Hp < 0) playerMainData.Hp = 0;
                TriggerUpdateHPEvent();
                // TODO:����Ѿ�����
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
    /// �ָ�����ֵ
    /// </summary>
    public void RecoverHP(float value)
    {
        playerMainData.Hp = Mathf.Clamp(playerMainData.Hp + value, 0, playerConfig.MaxHp);
        TriggerUpdateHPEvent();
    }

    /// <summary>
    /// �ָ�����ֵ
    /// </summary>
    public void RecoverHungry(float value)
    {
        playerMainData.Hungry = Mathf.Clamp(playerMainData.Hungry + value, 0, playerConfig.MaxHungry);
        TriggerUpdateHungryEvent();
    }
    #endregion

    #region �������
    private ItemData currentWeaponItemData;
    private GameObject currentWeaponGameObject;

    /// <summary>
    /// �޸�����
    /// </summary>
    public void ChangeWeapon(ItemData newWeapon)
    {
        // ѹ��û�л�����
        if (currentWeaponItemData == newWeapon)
        {
            currentWeaponItemData = newWeapon;
            return;
        }

        // ��������������ݣ��Ѿ�����ģ�ͻ��յ������
        if (currentWeaponItemData!=null)
        {
            // ���������ʱ���ǻ���GameObject.name�ģ����Բ���ͬ��
            currentWeaponGameObject.JKGameObjectPushPool(); 
        }

        // �������������Null
        if (newWeapon!=null)
        {
            ItemWeaponInfo newWeaponInfo = newWeapon.Config.ItemTypeInfo as ItemWeaponInfo;
            currentWeaponGameObject = PoolManager.Instance.GetGameObject(newWeaponInfo.PrefabOnPlayer,player_Model.WeaponRoot);
            currentWeaponGameObject.transform.localPosition = newWeaponInfo.PositionOnPlayer;
            currentWeaponGameObject.transform.localRotation = Quaternion.Euler(newWeaponInfo.RotationOnPlayer);
            animator.runtimeAnimatorController = newWeaponInfo.AnimatorController;
        }
        // ��������Null����ζ�ſ���
        else
        {
            animator.runtimeAnimatorController = playerConfig.NormalAnimatorController;
        }
        // ���ڶ������߼�״̬������ ��������¼���һ�ζ�������������󣨱������ƶ��У�ͻȻ�л�AnimatorController�᲻������·������
        stateMachine.ChangeState<Player_Idle>((int)PlayerState.Idle, true);
        currentWeaponItemData = newWeapon;
    }

    #endregion

    #region ս������ľ��ժ
    private bool canAttack = true;
    public Quaternion attackDir { get; private set; }
    private List<MapObjectBase> lastAttackedMaoObjectList = new List<MapObjectBase>();

    // ��󹥻��ĵ�ͼ����
    private MapObjectBase lastHitMapObject;
    /// <summary>
    /// ��ѡ���ͼ�������AIʱ
    /// </summary>
    public void OnSelectMapObject(RaycastHit hitInfo,bool isMouseButtonDown)
    {
        if (hitInfo.collider.TryGetComponent<MapObjectBase>(out MapObjectBase mapObject))
        {
            float dis = Vector3.Distance(playerTransform.position, mapObject.transform.position);
            // ˵�����ڽ�����Χ��
            if (dis > mapObject.TouchDinstance)
            {
                if (isMouseButtonDown)
                {
                    UIManager.Instance.AddTips("���һ��Ŷ��");
                    ProjectTool.PlayAudio(AudioType.Fail);
                }
                return;
            }
            // �ж�ʰȡ
            if (mapObject.CanPickUp)
            {
                if (!isMouseButtonDown) return;
                lastHitMapObject = null;
                // ��ȡ�񵽵���Ʒ
                int itemConfigID = mapObject.PickUpItemConfigID;
                if (itemConfigID != -1)
                {
                    // �����������������ӳɹ��������ٵ�ͼ����
                    if (InventoryManager.Instance.AddMainInventoryItem(itemConfigID))
                    {
                        mapObject.OnPickUp();
                        // ���ż��������Ķ��� ����û���л�״̬����Ȼ��Idle״̬
                        PlayAnimation("PickUp");
                        ProjectTool.PlayAudio(AudioType.Bag);
                    }
                    else
                    {
                        if (isMouseButtonDown)
                        {
                            UIManager.Instance.AddTips("�����Ѿ����ˣ�");
                            ProjectTool.PlayAudio(AudioType.Fail);
                        }
                    }
                }
                return;
            }
            if (!canAttack) return;
            // ���ڽ����Ķ�������һ�������¶���ֻ������굥��ʱ������������ظ���һ��������в������������ְ���״̬���н���
            if (lastHitMapObject != mapObject && !isMouseButtonDown) return;
            lastHitMapObject = mapObject;
            // �жϹ���
            // �������ѡ�еĵ�ͼ���������Լ���ǰ��ɫ���������ж���ʲô
            switch (mapObject.ObjectType)
            {
                case MapObjectType.Tree:
                    if (!CheckHitMapObject(mapObject, WeaponType.Axe) && isMouseButtonDown)
                    {
                        UIManager.Instance.AddTips("����Ҫװ����ͷ��");
                        ProjectTool.PlayAudio(AudioType.Fail);
                    }
                    break;
                case MapObjectType.Stone:
                    if (!CheckHitMapObject(mapObject, WeaponType.PickAxe) && isMouseButtonDown)
                    {
                        UIManager.Instance.AddTips("����Ҫװ���䣡");
                        ProjectTool.PlayAudio(AudioType.Fail);
                    }
                    break;
                case MapObjectType.Bush:
                    if (!CheckHitMapObject(mapObject, WeaponType.Sickle) && isMouseButtonDown)
                    {
                        UIManager.Instance.AddTips("����Ҫװ��������");
                        ProjectTool.PlayAudio(AudioType.Fail);
                    }
                    break;
            }
            return;
        }

        // ����AI���߼�
        if (canAttack&&currentWeaponItemData != null && hitInfo.collider.TryGetComponent<AIBase>(out AIBase aiObject))
        {
            float dis = Vector3.Distance(playerTransform.position, aiObject.transform.position);
            ItemWeaponInfo itemWeaponInfo = currentWeaponItemData.Config.ItemTypeInfo as ItemWeaponInfo;
            // �������룺�����ĳ��� + AI�İ뾶
            if (dis< itemWeaponInfo.AttackDistance + aiObject.Radius)
            {
                // ��ֹ�����ֽ��й���
                canAttack = false;
                // ���㷽��
                attackDir = Quaternion.LookRotation(aiObject.transform.position - transform.position);
                // ���Ź�����Ч
                AudioManager.Instance.PlayOnShot(itemWeaponInfo.AttackAudio,transform.position);
                // �л�״̬
                ChangeState(PlayerState.Attack);
                // ��ֹʹ����Ʒ
                CanUseItem = false;
            }
        }
    }

    // �������ͼ����
    public bool CheckHitMapObject(MapObjectBase mapObject, WeaponType weaponType)
    {
        if (currentWeaponItemData == null) return false;

        ItemWeaponInfo itemWeaponInfo = currentWeaponItemData.Config.ItemTypeInfo as ItemWeaponInfo;
        // ������������������Ҫ��
        if (itemWeaponInfo.WeaponType == weaponType)
        {
            // ��ֹ�����ֽ��й���
            canAttack = false;
            // ���㷽��
            attackDir = Quaternion.LookRotation(mapObject.transform.position - transform.position);
            // ���Ź�����Ч
            AudioManager.Instance.PlayOnShot(itemWeaponInfo.AttackAudio, transform.position);
            // �л�״̬
            ChangeState(PlayerState.Attack);
            // ��ֹʹ����Ʒ
            CanUseItem = false;
            return true;
        }
        return false;
    }

    // �����������˺����
    private void OnStartHit()
    {
        attackSucceedCount = 0;
        currentWeaponGameObject.transform.OnTriggerEnter(OnWeaponTriggerEnter);
    }

    // ������ֹͣ�˺����
    private void OnStopHit()
    {
        currentWeaponGameObject.transform.RemoveTriggerEnter(OnWeaponTriggerEnter);
        lastAttackedMaoObjectList.Clear();
    }

    // ��������״̬�Ľ���
    private void OnAttackOver()
    {
        // �ɹ����й����Σ������ļ����;ö�
        for (int i = 0; i < attackSucceedCount; i++)
        {
            // ����������
            EventManager.EventTrigger(EventName.PlayerWeaponAttackSucceed);
        }

        canAttack = true;
        // �л�״̬������
        ChangeState(PlayerState.Idle);
        // ����ʹ����Ʒ
        CanUseItem = true;
    }
    // �����ɹ�������
    private int attackSucceedCount;
    // ����������������Ϸ����ʱ
    private void OnWeaponTriggerEnter(Collider other, object[] arg2)
    {
        // �Է����ǵ�ͼ�����������
        if (other.TryGetComponent<HitMapObjectBase>(out HitMapObjectBase mapObject))
        {
            // �Ѿ��������ģ���ֹ�����˺�
            if (lastAttackedMaoObjectList.Contains(mapObject)) return;
            lastAttackedMaoObjectList.Add(mapObject);
            // ���Է���ʲô���� �Լ� �Լ�������ʲô����
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
    /// ����ͼ�����ܷ�����
    /// </summary>
    private void CheckMapObjectHurt(HitMapObjectBase hitMapObject, WeaponType weaponType)
    {
        ItemWeaponInfo itemWeaponInfo = currentWeaponItemData.Config.ItemTypeInfo as ItemWeaponInfo;
        if (itemWeaponInfo.WeaponType == weaponType)
        {
            // ��������
            AudioManager.Instance.PlayOnShot(itemWeaponInfo.HitAudio, transform.position);
            hitMapObject.Hurt(itemWeaponInfo.AttackValue);
            attackSucceedCount += 1;
        }
    }


    public void Hurt(float damage)
    {
        Debug.Log("�������:"+damage);
    }

    #endregion

    private void Update()
    {
        if (GameSceneManager.Instance.IsInitialized == false) return;
        CalculateHungryOnUpdate();
    }

    private void OnDestroy()
    {
        // �Ѵ浵����ʵ��д�����
        playerTransformData.Position = playerTransform.localPosition;
        playerTransformData.Rotation = playerTransform.localRotation.eulerAngles;
        ArchiveManager.Instance.SavePlayerTransformData();
        ArchiveManager.Instance.SavePlayerMainData();
    }
}
