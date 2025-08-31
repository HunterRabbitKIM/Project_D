using UnityEngine;
using System;

public class UIManager : MonoBehaviour
{
    [SerializeField] private BeautifyUIController beautifyController; // 추가: BeautifyUIController 참조
    [SerializeField] private DialogUIController dialogController;     // 추가: DialogUIController 참조

    [SerializeField] private bool useCustomDialogSystem = true;

    public static event Action<string> OnDialogSystemTriggered;
    public static event Action<string, string> OnDialogEffectRequested;

    public static event Action<string> OnConversationStarted;         // 추가: 대화 시작 이벤트
    public static event Action OnConversationEnded;                   // 추가: 대화 종료 이벤트

    private string currentSystemName = "";
    private bool isConversationActive = false;

    private void Start()
    {
        InitializeUIManager();
        SubscribeToDialogEvents();
    }

    private void InitializeUIManager()
    {
        if (beautifyController == null)
        {
            beautifyController = FindObjectOfType<BeautifyUIController>();
            if (beautifyController == null)
            {
                Debug.LogWarning("BeautifyUIController를 찾을 수 없습니다!");
            }
        }

        if (dialogController == null)
        {
            dialogController = FindObjectOfType<DialogUIController>();
            if (dialogController == null)
            {
                Debug.LogWarning("DialogUIController를 찾을 수 없습니다!");
            }
        }
    }

    private void SubscribeToDialogEvents()
    {
        // 수정: 커스텀 Dialog System 이벤트 구독 (PixelCrushers 제거)
        OnConversationStarted += HandleConversationStarted;
        OnConversationEnded += HandleConversationEnded;

        // 기존: 커스텀 이벤트 구독
        OnDialogSystemTriggered += HandleDialogSystemTrigger;
        OnDialogEffectRequested += HandleDialogEffectRequest;
    }

    private void HandleConversationStarted(string systemName)
    {
        currentSystemName = systemName;
        isConversationActive = true;
        HandleDialogSystemTrigger(systemName);
    }

    private void HandleConversationEnded()
    {
        isConversationActive = false;
        currentSystemName = "";
        ResetAllUIEffects();
    }


    private void HandleDialogSystemTrigger(string systemName)
    {
        switch (systemName)
        {
            case "IntroDialog":
                HandleIntroDialog();
                break;

            case "MainDialog":
                HandleMainDialog();
                break;

            case "EndingDialog":
                HandleEndingDialog();
                break;

            default:
                HandleDefaultDialog();
                break;
        }
    }

    private void HandleIntroDialog()
    {
        // Bella_Intro_Dialog의 첫 부분에서 눈 깜빡임 실행
        if (beautifyController != null)
        {
            beautifyController.ExecuteBlinkWithBlur();
        }

        Debug.Log("IntroDialog 시스템 이펙트 실행: 눈 깜빡임과 블러 적용");
    }

    private void HandleMainDialog()
    {
        if (beautifyController != null)
        {
            beautifyController.ApplyEndBlur();
        }

        Debug.Log("MainDialog 시스템 이펙트 실행: 블러 적용");
    }

    private void HandleEndingDialog()
    {
        if (beautifyController != null)
        {
            beautifyController.StartBlinking();
        }

        Debug.Log("EndingDialog 시스템 이펙트 실행: 깜빡임 적용");
    }
    private void HandleDefaultDialog()
    {
        if (beautifyController != null)
        {
            beautifyController.ApplyStartBlur();
        }

        Debug.Log("기본 Dialog 시스템 이펙트 실행: 시작 블러 적용");
    }
    private void HandleDialogEffectRequest(string systemName, string effectType)
    {
        switch (effectType.ToLower())
        {
            case "blink":
                beautifyController?.StartBlinking();
                break;

            case "blur_start":
                beautifyController?.ApplyStartBlur();
                break;

            case "blur_end":
                beautifyController?.ApplyEndBlur();
                break;

            case "blink_with_blur":
                beautifyController?.ExecuteBlinkWithBlur();
                break;

            case "reset":
                beautifyController?.ResetAllEffects();
                break;
        }
    }
    private void ResetAllUIEffects()
    {
        if (beautifyController != null)
        {
            beautifyController.ResetAllEffects();
        }
    }
    public static void TriggerDialogEffect(string systemName)
    {
        OnDialogSystemTriggered?.Invoke(systemName);
    }
    public static void RequestDialogEffect(string systemName, string effectType)
    {
        OnDialogEffectRequested?.Invoke(systemName, effectType);
    }
    private void OnDestroy()
    {
        OnConversationStarted -= HandleConversationStarted;
        OnConversationEnded -= HandleConversationEnded;
        OnDialogSystemTriggered -= HandleDialogSystemTrigger;
        OnDialogEffectRequested -= HandleDialogEffectRequest;
    }
}
