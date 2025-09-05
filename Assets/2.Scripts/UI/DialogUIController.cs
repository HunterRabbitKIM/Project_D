using UnityEngine;

public class DialogUIController : MonoBehaviour
{
    [SerializeField] private GameObject dialogPanel;
    private bool wasDialogUIVisible = false;

    private void Start()
    {
        SubscribeToDialogEvents();
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
        if (dialogPanel != null && wasDialogUIVisible)
        {
            dialogPanel.SetActive(true); // 간단하게 복원
        }
    }

    private void OnDestroy()
    {
        BeautifyUIController.OnEffectStarted -= OnBeautifyEffectStarted;
        BeautifyUIController.OnEffectCompleted -= OnBeautifyEffectCompleted;
    }
}
