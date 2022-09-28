using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// ���λ����ص�����
/// </summary>
[Serializable]
public class PlayerTransformData
{
    // ����
    private Serialization_Vector3 position;
    public Vector3 Position
    {
        get => position.ConverToVector3();
        set => position = value.ConverToSVector3();
    }

    // ��ת
    private Serialization_Vector3 rotation;
    public Vector3 Rotation
    {
        get => rotation.ConverToVector3();
        set => rotation = value.ConverToSVector3();
    }
}

[Serializable]
public class PlayerMainData
{
    public float Hp;
    public float Hungry;
}
