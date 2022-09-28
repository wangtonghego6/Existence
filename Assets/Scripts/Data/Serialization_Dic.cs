using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization;

/// <summary>
/// �����л����ֵ�
/// </summary>
[Serializable]
public class Serialization_Dic<K, V>
{
    private List<K> keyList;
    private List<V> valueList;

    [NonSerialized] // �����л� ���ⱨ��
    private Dictionary<K, V> dictionary;
    public Dictionary<K, V> Dictionary { get => dictionary; }
    public Serialization_Dic()
    {
        dictionary = new Dictionary<K, V>();
    }

    public Serialization_Dic(Dictionary<K, V> dictionary)
    {
        this.dictionary = dictionary;
    }

    // ���л���ʱ����ֵ���������ݷŽ�list
    [OnSerializing]
    private void OnSerializing(StreamingContext context)
    {
        keyList = new List<K>(dictionary.Count);
        valueList = new List<V>(dictionary.Count);
        foreach (var item in dictionary)
        {
            keyList.Add(item.Key);
            valueList.Add(item.Value);
        }
    }

    // �����л�ʱ���Զ�����ֵ�ĳ�ʼ��
    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
        dictionary = new Dictionary<K, V>(keyList.Count);
        for (int i = 0; i < keyList.Count; i++)
        {
            dictionary.Add(keyList[i], valueList[i]);
        }
    }

}
