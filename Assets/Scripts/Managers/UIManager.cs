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
                
            }
        }
    }
}
