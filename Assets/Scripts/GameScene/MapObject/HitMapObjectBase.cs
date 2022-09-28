using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;

/// <summary>
/// �ɻ���ĵ�ͼ�������
/// </summary>
public abstract class HitMapObjectBase : MapObjectBase
{
    [SerializeField] Animator animator;
    [SerializeField] AudioClip[] hurtAudioClips;
    [SerializeField] float maxHp;
    [SerializeField] int lootConfigID = -1; // ����ʱ���������ID
    private float hp;

    public override void Init(MapChunkController mapChunk, ulong id, bool isFormBuild)
    {
        base.Init(mapChunk, id, isFormBuild);
        hp = maxHp;
    }
    /// <summary>
    /// ����
    /// </summary>
    public void Hurt(float damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            // ����
            Dead();
        }
        else
        {
            animator.SetTrigger("Hurt");
        }
 
        AudioManager.Instance.PlayOnShot(hurtAudioClips[Random.Range(0, hurtAudioClips.Length)], transform.position);
    }

    // ����
    private void Dead()
    {
        RemoveOnMap();
        // ��Ʒ�����߼�
        if (lootConfigID == -1) return;
        LootConfig lootConfig = ConfigManager.Instance.GetConfig<LootConfig>(ConfigName.Loot,lootConfigID);
        if (lootConfig != null) lootConfig.GeneaterMapObject(mapChunk,transform.position + new Vector3(0,1,0));
    }
}
