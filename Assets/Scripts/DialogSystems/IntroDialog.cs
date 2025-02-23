using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class IntroDialog : MonoBehaviour
{
    public ChoiceManager choiceManager;

    [SerializeField]
    private DialogSystem dialogSystem2_1;
    [SerializeField]
    private DialogSystem dialogSystem2_2;
    [SerializeField]
    private DialogSystem dialogSystem2_3;
    [SerializeField]
    private DialogSystem dialogSystem2_4;
    [SerializeField]
    private DialogSystem dialogSystem2_5;
    [SerializeField]
    private DialogSystem dialogSystem2_6;
    [SerializeField]
    private DialogSystem dialogSystem3_1;
    [SerializeField]
    private DialogSystem dialogSystem4_1;
    [SerializeField]
    private DialogSystem dialogSystem4_2;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => dialogSystem2_1.UpdateDialog());
        yield return new WaitUntil(() => dialogSystem2_2.UpdateDialog());
        yield return new WaitUntil(() => dialogSystem2_3.UpdateDialog());
        yield return new WaitUntil(() => dialogSystem2_4.UpdateDialog());
        yield return new WaitUntil(() => dialogSystem2_5.UpdateDialog());
        yield return new WaitUntil(() => dialogSystem2_6.UpdateDialog());
        yield return new WaitUntil(() => dialogSystem3_1.UpdateDialog());
        yield return new WaitUntil(() => dialogSystem4_1.UpdateDialog());
        yield return new WaitUntil(() => dialogSystem4_2.UpdateDialog());

        yield return StartCoroutine(choiceManager.ShowChoices());
    }
}
