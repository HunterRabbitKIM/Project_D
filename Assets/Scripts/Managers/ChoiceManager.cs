using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class DialogChoice
{
    public string choiceText;
    public UnityEvent onChoiceSelected;
}

public class ChoiceManager : MonoBehaviour
{
    [SerializeField]
    private List<DialogChoice> choices = new List<DialogChoice>();
    [SerializeField]
    private GameObject choiceButtonPrefab;
    [SerializeField]
    private Transform choiceContainer;

    public GameObject selectCanvas;

    public IEnumerator ShowChoices()
    {
        Debug.Log("OK");

        bool choiceSelected = false;

        foreach (var choice in choices)
        {
            GameObject buttonObj = Instantiate(choiceButtonPrefab, choiceContainer);
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            buttonText.text = choice.choiceText;

            button.onClick.AddListener(() =>
            {
                choice.onChoiceSelected.Invoke();
                choiceSelected = true;

                ClearChoices();
            });
        }

        // 사용자가 선택할 때까지 대기
        yield return new WaitUntil(() => choiceSelected);
    }

    public void ClearChoices()
    {
        foreach(Transform child in choiceContainer)
        {
            Destroy(child.gameObject);
        }
    }
}
