using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class UISelect : UIHUD
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Transform buttonParent;
    [SerializeField] private GameObject buttonPrefab;

    private List<Button> buttonList = new List<Button>();
    public DialogBase CurrentDialog;

    public override void Active()
    {
        base.Active();

        foreach (Button button in buttonList)
        {
            Destroy(button.gameObject);
        }
    }

    public override void Deactive()
    {
        base.Deactive();
    }

    public void SetText(string text)
    {
        titleText.text = text;
    }

    public void CreateNewSelectButtonDialog(string dialog, string path)
    {
        GameObject go = Instantiate(buttonPrefab, buttonParent);

        if(go.TryGetComponent<Button>(out Button button))
        {
            TMP_Text text = button.GetComponentInChildren<TMP_Text>();
            if(text != null)
            {
                text.text = dialog;
            }

            button.onClick.AddListener(() => { OnButtonClieckEventDialog(path); });
        }
    }

    public void OnButtonClieckEventDialog(string path)
    {
        DataManager.Instance.LoadDialogFromCSV(path);
        if(CurrentDialog != null)
        {
            CurrentDialog.IsFinish = true;
            DataManager.Instance.DoNextDialog();
        }
    }
}
