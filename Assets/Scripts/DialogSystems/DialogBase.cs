using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DialogBase
{
    public string Narrator;
    public string Dialog;
    public bool IsFinish = false;

    public DialogBase(string narrator, string dialog)
    {
        Narrator = narrator;
        Dialog = dialog;
    }

    public abstract void Excute();

    public abstract void AfterFinish();
}

public class DialogSystem : DialogBase
{
    public DialogSystem(string narrator, string dialog) : base(narrator, dialog)
    {
        this.Narrator = narrator;
        this.Dialog = dialog;
    }

    public override void Excute()
    {
        UIManager.Instance.ChangeHUDUI<UIDialog>();
        UIDialog uiDialog = UIManager.Instance.GetComponent<UIDialog>() as UIDialog;

        if(uiDialog == null)
        {
            return;
        }

        uiDialog.CurrentDialog = this;
        uiDialog.SetText(Dialog, Narrator);
    }

    public override void AfterFinish()
    {

    }
}

public abstract class InputDialog : DialogBase
{
    public UIInput uiInput;

    public InputDialog(string narrator, string dialog) : base (narrator, dialog)
    {
        this.Narrator = narrator;
        this.Dialog = dialog;
    }

    public override void Excute()
    {
        UIManager.Instance.ChangeHUDUI<UIInput>();
        this.uiInput = UIManager.Instance.GetUICopmonent<UIInput>() as UIInput;

        if(uiInput == null)
        {
            return;
        }

        uiInput.SetText(Dialog);
        uiInput.InputStringAction += FinishInput;
    }

    public override void AfterFinish()
    {
        this.uiInput.InputStringAction -= FinishInput;
    }

    public abstract void FinishInput(string inputString);
}

public class SelectDialog : DialogBase
{
    public UISelect uiChoose;

    public SelectDialog(string narrator, string dialog) : base (narrator, dialog)
    {
        this.Narrator = narrator;
        this.Dialog = dialog;
    }

    public override void Excute()
    {
        UIManager.Instance.ChangeHUDUI<UISelect>();
        this.uiChoose = UIManager.Instance.GetUICopmonent<UISelect>() as UISelect;

        string[] buttonTextArray = SplitString(Dialog);
        string[] nextDialogPathArray = SplitString(Narrator);

        if(buttonTextArray.Length == 0 || nextDialogPathArray.Length == 0)
        {
            uiChoose.CreateNewSelectButtonDialog("Error", "Error");
            return;
        }

        uiChoose.CurrentDialog = this;
        uiChoose.SetText(buttonTextArray[0]);
        
        for(int i = 1; i < buttonTextArray.Length; i++)
        {
            int index = i;
            string textString = buttonTextArray[index];
            string dialogPath = nextDialogPathArray[index - 1];
            uiChoose.CreateNewSelectButtonDialog(textString, dialogPath);
        }
    }

    public override void AfterFinish()
    {

    }

    public static string[] SplitString(string dialog)
    {
        if(string.IsNullOrEmpty(dialog))
        {
            return new string[0];
        }

        return dialog.Split('|');
    }
}

public class InputDialogNarratorName : InputDialog
{
    public InputDialogNarratorName(string narrator, string dialog) : base(narrator, dialog)
    {
        this.Narrator = narrator;
        this.Dialog = dialog;
    }

    public override void FinishInput(string inputString)
    {
        DataManager.Instance.RegisterNarratorName(Narrator, inputString);
        IsFinish = true;
        DataManager.Instance.DoNextDialog();
    }
}
