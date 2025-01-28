using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//�̱��� ������ �����ϱ� ���� Ŭ���� (MonoBehavior�� ������� ��)
public class SingleTonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    // Singleton �ν��Ͻ��� ������ ����
    private static T _instance;

    /// Singleton �ν��Ͻ��� �����ϱ� ���� ������Ƽ
    /// ����, ����(static), �б� ����(read-only)
    /// protected �Ӽ��� �����ϱ� ���� get �Լ�
    public static T Instance { get { return _instance; } }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }
}
