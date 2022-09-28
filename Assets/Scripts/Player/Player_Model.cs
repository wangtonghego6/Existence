using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class Player_Model : MonoBehaviour
{
    [SerializeField] Transform weaponRoot;
    private Action<int> footstepAction;
    private Action startHitAction;
    private Action stopHitAction;
    private Action attackOverAction;
    public Transform WeaponRoot { get => weaponRoot; }

    public void Init(Action<int> footstepAction, Action startHitAction, Action stopHitAction, Action attackOverAction)
    { 
        this.footstepAction = footstepAction;
        this.startHitAction = startHitAction;
        this.stopHitAction = stopHitAction;
        this.attackOverAction = attackOverAction;
    }

    #region �����¼�
    // �Ų���
    private void Footstep(int index)
    {
        footstepAction?.Invoke(index);
    }

    // ��ʼ���˺�
    private void StartHit()
    {
        startHitAction?.Invoke();
    }
    
    // ����֮��û���˺�
    private void StopHit()
    {
        stopHitAction?.Invoke();
    }

    // ���������Ľ���
    private void AttackOver()
    { 
        attackOverAction?.Invoke();
    }

    #endregion
}
