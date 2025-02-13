using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogManager : MonoBehaviour
{
    [SerializeField]
    private string dataName = "Dialog";

    private void Start()
    {
        DataManager.Instance.LoadDialogFromCSV(dataName);
    }

    private void Update()
    {
        if(Input.GetMouseButton(0))
        {
            int leftDialogCount = DataManager.Instance.DoNextDialog();
            if(leftDialogCount == 0)
            {
                //이번 job이 마지막
                Debug.Log("등록된 Job이 없습니다. 다음 이벤트 실행");
            }
        }
    }
}
