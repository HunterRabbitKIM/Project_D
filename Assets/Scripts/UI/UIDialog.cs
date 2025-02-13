using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;

public class UIDialog : UIHUD
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text dialogText;
    [SerializeField] private GameObject blinklconObject;
    [SerializeField] private DataManager dialog;

    [SerializeField] private float charPerSecond = 0.05f;

    private Coroutine blinkCoroutine;
    private bool isFinish = true;

    public DialogSystem CurrentDialog;

    public override void Active()
    {
        base.Active();
    }

    public void SetText(string dialog, string id)
    {
        if(DataManager.Instance.ReplaceDict.TryGetValue(id, out string narratorName))
        {
            nameText.text = narratorName;
        }
        isFinish = false;

        StartCoroutine(DoText(dialog));
    }

    private IEnumerator DoText(string text)
    {
        if(blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }
        blinklconObject.SetActive(false);

        foreach(var pair in DataManager.Instance.ReplaceDict)
        {
            text = text.Replace($"{{{pair.Key}}}", pair.Value);
        }

        StringBuilder sb = new StringBuilder();
        WaitForSeconds wfs = new WaitForSeconds(charPerSecond);

        for(int i = 0; i < text.Length; i++)
        {
            sb.Append(text[i]);
            dialogText.text = sb.ToString();
            yield return wfs;
        }

        isFinish = true;
        if(CurrentDialog != null)
        {
            CurrentDialog.IsFinish = true;
        }
        blinkCoroutine = StartCoroutine(DoTextEndBlink());
    }

    private IEnumerator DoTextEndBlink()
    {
        WaitForSecondsRealtime wfs = new WaitForSecondsRealtime(0.5f);
        while(true)
        {
            blinklconObject.SetActive(!blinklconObject.activeSelf);
            yield return wfs;
        }
    }
}
