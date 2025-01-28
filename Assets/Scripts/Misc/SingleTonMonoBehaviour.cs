using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//싱글톤 패턴을 구현하기 위한 클래스 (MonoBehavior를 기반으로 함)
public class SingleTonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    // Singleton 인스턴스를 저장할 변수
    private static T _instance;

    /// Singleton 인스턴스에 접근하기 위한 프로퍼티
    /// 공개, 정적(static), 읽기 전용(read-only)
    /// protected 속성에 접근하기 위한 get 함수
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
