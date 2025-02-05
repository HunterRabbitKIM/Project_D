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
    private Dictionary<System.Type, UIBase> _uoDict = new Dictionary<System.Type, UIBase>();
    private UIHUD _currentHUD;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
