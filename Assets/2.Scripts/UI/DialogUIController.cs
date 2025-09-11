using UnityEngine;

public class DialogUIController : MonoBehaviour
{
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private DialogSystem dialogSystem;
    private bool wasDialogUIVisible = false;

    private void Start()
    {
        SubscribeToDialogEvents();
        if(dialogSystem == null)
        {
            dialogSystem = FindObjectOfType<DialogSystem>();
        }
    }

    private void SubscribeToDialogEvents()
    {
        BeautifyUIController.OnEffectStarted += OnBeautifyEffectStarted;
        BeautifyUIController.OnEffectCompleted += OnBeautifyEffectCompleted;
    }

    private void OnBeautifyEffectStarted()
    {
        if (dialogPanel != null)
        {
            wasDialogUIVisible = dialogPanel.activeInHierarchy;

            if (wasDialogUIVisible)
            {
                dialogPanel.SetActive(false); // 간단하게 숨기기
            }
        }
        else
        {
            Debug.LogError("DialogPanel이 설정되지 않았습니다!"); // 추가: 에러 로그
        }
    }

    private void OnBeautifyEffectCompleted()
    {
        if(wasDialogUIVisible)
        {
            if(dialogPanel != null)
            {
                dialogSystem.RestoreUIAfterEffect();
            }
            else
            {
                if(dialogSystem != null)
                {
                    dialogPanel.SetActive(true);
                }
            }
        }
    }

    private void OnDestroy()
    {
        BeautifyUIController.OnEffectStarted -= OnBeautifyEffectStarted;
        BeautifyUIController.OnEffectCompleted -= OnBeautifyEffectCompleted;
    }
}
