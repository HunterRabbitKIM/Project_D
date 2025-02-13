using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class UIInput : UIHUD
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button button;
    [SerializeField] protected string initString = "입력하세요";

    public event Action<string> InputStringAction;

    public void SetText(string text)
    {
        titleText.text = text;
        inputField.text = initString;
    }

    public void FinishInput()
    {
        InputStringAction?.Invoke(inputField.text);
    }
}
