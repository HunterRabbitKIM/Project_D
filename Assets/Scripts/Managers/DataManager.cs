using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class DataManager : SingleTonMonoBehaviour<DataManager>
{
    private string dialogFilePath = "Assets/Data/";
    private string dialogFileExtention = ".csv";

    private LinkedList<DialogBase> dialogList = new LinkedList<DialogBase>();
    private DialogBase currentDialog = null;

    public Dictionary<string, string> ReplaceDict = new Dictionary<string, string>();
    public List<int> TEST_EVENT_INT_LIST = new List<int>();

    protected override void Awake()
    {
        base.Awake();

        ReplaceDict["Narrator"] = "나레이터";
        ReplaceDict["Player"] = "주인공";
        ReplaceDict["Witch"] = "마녀(벨라)";

        EventHandler.AfterRegisterUIManager += DoDialog;
    }

    private void OnDisable()
    {
        EventHandler.AfterRegisterUIManager -= DoDialog;
    }

    public void LoadDialogFromCSV(string filePath, bool resetDialogList = false)
    {
        filePath = dialogFilePath + filePath + dialogFileExtention;

        if(!File.Exists(filePath))
        {
            Debug.LogError("CSV 파일이 존재하지 않습니다.");
            return;
        }

        if(resetDialogList )
        {
            dialogList.Clear();
        }

        currentDialog = null;

        string[] lineArray = File.ReadAllLines(filePath);
        for(int i = 1; i < lineArray.Length; i++)
        {
            string line = lineArray[i];

            string[] parts = ParseCsvLine(line);
            if(parts.Length < 4)
            {
                Debug.LogError($"CSV파일에 잘못된 형식이 있습니다. line: {i + 1}");
                continue;
            }

            //DB 구조가 바뀌면 수정해야 할 곳
            int dialogType = int.Parse(parts[0].Trim());
            string narrator = parts[1].Trim();
            int isTalking = int.Parse(parts[2].Trim());
            string dialog = parts[3].Trim().Replace('*', ',');

            TEST_EVENT_INT_LIST.Add(dialogType);

            DialogBase newDialog = CreateDialog(narrator, dialog, dialogType);

            if(newDialog != null)
            {
                dialogList.AddLast(newDialog);
            }
            else
            {
                Debug.LogError($"CSV 파일에 잘못된 event 형식이 있거나 등록되지 않은 Job Class 입니다. line: {i}");
            }
        }
    }

    private string[] ParseCsvLine(string line)
    {
        List<string> fields = new List<string>();
        string pattern = @"""([^""]*)""|([^,]+)";
        Regex regex = new Regex(pattern);

        foreach(Match match in regex.Matches(line))
        {
            if (match.Groups[1].Success)
            {
                fields.Add(match.Groups[1].Value);
            }
            else if(match.Groups[2].Success)
            {
                fields.Add(match.Groups[2].Value);
            }
        }

        return fields.ToArray();
    }

    private DialogBase CreateDialog(string narrator, string dialog, int type)
    {
        switch(type)
        {
            case 100:
                return new DialogSystem(narrator, dialog);
            case 200:
                return new InputDialogNarratorName(narrator, dialog);
            case 300:
                return new SelectDialog(narrator, dialog);
        }
        return null;
    }

    public int DoNextDialog()
    {
        int DialogCount = dialogList.Count;

        DoDialog();

        return DialogCount;
    }

    private void DoDialog()
    {
        if(currentDialog != null)
        {
            if(!currentDialog.IsFinish)
            {
                return;
            }
            else
            {
                currentDialog.AfterFinish();
            }
        }

        if(dialogList.Count > 0)
        {
            currentDialog = dialogList.First.Value;
            dialogList.RemoveFirst();
            currentDialog.Excute();
        }
    }

    public void RegisterNarratorName(string id, string name)
    {
        ReplaceDict[id] = name;
    }

    public void RegisterDialog(DialogBase dialog)
    {
        if(dialog != null)
        {
            dialogList.AddFirst(dialog);
        }
    }
}
