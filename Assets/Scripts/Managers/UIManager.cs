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
                
            }
        }
    }
}
