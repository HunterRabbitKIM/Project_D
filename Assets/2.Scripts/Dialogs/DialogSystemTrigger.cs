using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DialogSystemTrigger : MonoBehaviour
{
    [Header("Dialog System Reference")]
    [SerializeField] private DialogSystem dialogSystem;  // DialogSystemManager 대신 DialogSystem 사용
    
    [Header("Auto Start")]
    [SerializeField] private bool autoStartOnStart = true;
    
    private void Start()
    {
        if (autoStartOnStart)
        {
            TriggerDialog();
        }
    }
    
    public void TriggerDialog()
    {
        if (dialogSystem != null)
        {
            StartCoroutine(StartDialogSequence());
        }
        else
        {
            Debug.LogError("DialogSystemTrigger: DialogSystem이 할당되지 않았습니다!");
        }
    }
    
    private IEnumerator StartDialogSequence()
    {
        dialogSystem.StartConversation();
        yield return new WaitUntil(() => dialogSystem.UpdateDialog());
        Debug.Log("Dialog sequence finished.");
    }
    
    // 외부에서 DialogSystem을 추가하는 메서드 (기존 호환성을 위해)
    public void AddDialogSystem(DialogSystem system)
    {
        dialogSystem = system;
    }
    
    // 수동으로 대화 시작하는 메서드
    [ContextMenu("Start Dialog Manually")]
    public void StartDialogManually()
    {
        TriggerDialog();
    }
}