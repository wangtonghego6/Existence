using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
public class Camera_Controller : SingletonMono<Camera_Controller>
{
    private Transform mTransform;
    [SerializeField] Transform target;// ����Ŀ��
    [SerializeField] Vector3 offset;  // ����ƫ����
    [SerializeField] float moveSpeed; // �����ٶ�
    [SerializeField] new Camera camera; // �����ٶ�
    public Camera Camera { get => camera; } // �����ٶ�

    private Vector2 positionXScope; // X�ķ�Χ
    private Vector2 positionZScope; // Z�ķ�Χ

    public void Init(float mapSizeOnWorld)
    {
        mTransform = transform;
        InitPositionScope(mapSizeOnWorld);
    }

    // ��ʼ�����귶Χ
    private void InitPositionScope(float mapSizeOnWorld)
    {
        positionXScope = new Vector2(5, mapSizeOnWorld - 5);
        positionZScope = new Vector2(-1, mapSizeOnWorld - 5);
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            Vector3 targetPosition = target.position + offset;
            targetPosition.x = Mathf.Clamp(targetPosition.x, positionXScope.x, positionXScope.y);
            targetPosition.z = Mathf.Clamp(targetPosition.z, positionZScope.x, positionZScope.y);
            mTransform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
        }
    }
}
