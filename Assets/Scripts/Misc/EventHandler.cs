using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// 이벤트를 관리하는 전역 정적 클래스
public static class EventHandler
{
    /// 씬 전환 이벤트 (매개변수 없음)
    /// 이벤트를 정의하는 법은 On에다가 이름 붙임
    /// 이벤트 호출 - Invoke 함수 사용
    public static event Action OnSceneChangeEvent;

    // UI 매니저 등록 이후 호출되는 이벤트 (매개변수 없음)
    public static event Action AfterRegisterUIManager;

    // AfterRegisterUIManager 이벤트 호출 메서드
    public static void CallAfterRegisterUIManager()
    {
        /// 이벤트에 구독자가 있을 경우에만 호출
        /// Invoke 함수를 통해 이벤트를 호출하여 구독자 목록을 순차적으로 실행
        /// ?. 부분으로 null이 아니여야 엑세스 가능하게 하는 것
        AfterRegisterUIManager?.Invoke();
    }
}
