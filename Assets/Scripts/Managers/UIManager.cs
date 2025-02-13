using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전역에서 접근 가능한 UI 관리 시스템을 구현
/// UIManager.Instacne를 통해 UI 조작
/// </summary>
public class UIManager : SingleTonMonoBehaviour<UIManager>
{
    /// <summary>
    /// UI_HUD 동시에 한개밖에 켤 수 없음
    /// UI_Popup은 동시에 여러개 킬 수 있지만, UI 계층 구조에 영향을 받아 보이지 않는 UI가 있을 수 있음
    /// </summary>
    private Dictionary<System.Type, UIBase> _uiDict = new Dictionary<System.Type, UIBase>();
    private UIHUD _currentHUD;

    public void RegisterUI(UIBase ui)
    {
        var type = ui.GetType();
        if(!_uiDict.ContainsKey(type))
        {
            _uiDict.Add(type, ui);
            ui.Deactive();

            if(transform.childCount == _uiDict.Count)
            {
                EventHandler.CallAfterRegisterUIManager();
            }
        }
    }

    /// <summary>
    /// UI Manager에 등록된 UIBase를 상속받는 컴포넌트 객체를 리턴한다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public UIBase GetUICopmonent<T>()
    {
        System.Type type = typeof(T);
        if(_uiDict.TryGetValue(type, out UIBase ui))
        {
            return ui;
        }
        return null;
    }

    /// <summary>
    /// HUD UI를 변경합니다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void ChangeHUDUI<T>() where T : UIHUD
    {
        System.Type type = typeof(T);
        if(_uiDict.TryGetValue(type, out UIBase ui))
        {
            if(_currentHUD != null)
            {
                _currentHUD.Deactive();
            }

            GameObject go = ui.gameObject;
            UIHUD hud = go.GetComponent<UIHUD>();
            _currentHUD = hud;
            hud.Active();
        }
    }

    /// <summary>
    /// Popup UI를 켭니다. 이미 켜져있다면, 무시
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void ShowPopup<T>() where T : UIPopup
    {
        var type = typeof(T);
        if(_uiDict.TryGetValue(type, out UIBase ui))
        {
            GameObject go = ui.gameObject;
            if(go.TryGetComponent<UIPopup>(out UIPopup popup))
            {
                if(!popup.IsActive)
                {
                    go.SetActive(true);
                }
            }
        }
        else
        {
            Debug.LogError($"타입 {type}이 등록되지 않았습니다.");
        }
    }

    /// <summary>
    /// Popup UI를 끕니다. 이미 꺼져있으면, 무시
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="popup"></param>
    public void ClosePopup<T>() where T : UIPopup
    {
        var type = typeof(T);

        if( _uiDict.TryGetValue(type,out UIBase ui))
        {
            ui.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError($"타입 {type}이 등록되지 않았습니다.");
        }
    }
}
