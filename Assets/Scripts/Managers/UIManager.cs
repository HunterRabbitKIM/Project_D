using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �������� ���� ������ UI ���� �ý����� ����
/// UIManager.Instacne�� ���� UI ����
/// </summary>
public class UIManager : SingleTonMonoBehaviour<UIManager>
{
    /// <summary>
    /// UI_HUD ���ÿ� �Ѱ��ۿ� �� �� ����
    /// UI_Popup�� ���ÿ� ������ ų �� ������, UI ���� ������ ������ �޾� ������ �ʴ� UI�� ���� �� ����
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
    /// UI Manager�� ��ϵ� UIBase�� ��ӹ޴� ������Ʈ ��ü�� �����Ѵ�.
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
    /// HUD UI�� �����մϴ�.
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
    /// Popup UI�� �մϴ�. �̹� �����ִٸ�, ����
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
            Debug.LogError($"Ÿ�� {type}�� ��ϵ��� �ʾҽ��ϴ�.");
        }
    }

    /// <summary>
    /// Popup UI�� ���ϴ�. �̹� ����������, ����
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
            Debug.LogError($"Ÿ�� {type}�� ��ϵ��� �ʾҽ��ϴ�.");
        }
    }
}
