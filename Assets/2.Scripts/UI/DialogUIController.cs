using UnityEngine;

public class DialogUIController : MonoBehaviour
{
    [SerializeField] private DialogSystem dialogSystem;

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
        
    }

    private void OnBeautifyEffectCompleted()
    {
        
    }

    private void OnDestroy()
    {
        BeautifyUIController.OnEffectStarted -= OnBeautifyEffectStarted;
        BeautifyUIController.OnEffectCompleted -= OnBeautifyEffectCompleted;
    }
}
