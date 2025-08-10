using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DialogSystemTrigger : MonoBehaviour
{
    [SerializeField]
    private List<DialogSystem> dialogSystems = new List<DialogSystem>();

    private IEnumerator Start()
    {
        foreach (var dialogSystem in dialogSystems)
        {
            if (dialogSystem != null)
            {
                dialogSystem.StartConversation();
                yield return new WaitUntil(() => dialogSystem.UpdateDialog());
            }
        }
        Debug.Log("All dialog systems have finished.");
    }

    public void AddDialogSystem(DialogSystem dialogSystem)
    {
        if (dialogSystem != null && !dialogSystems.Contains(dialogSystem))
        {
            dialogSystems.Add(dialogSystem);
        }
    }

    public void RemoveDialogSystem(DialogSystem dialogSystem)
    {
        if (dialogSystem != null)
        {
            dialogSystems.Remove(dialogSystem);
        }
    }

    public void ClearDialogSystems()
    {
        dialogSystems.Clear();
    }

    public void ReorderDialogSystems(int oldIndex, int newIndex)
    {
        if (oldIndex >= 0 && oldIndex < dialogSystems.Count && newIndex >= 0 && newIndex < dialogSystems.Count)
        {
            var item = dialogSystems[oldIndex];
            dialogSystems.RemoveAt(oldIndex);
            dialogSystems.Insert(newIndex, item);
        }
    }
}