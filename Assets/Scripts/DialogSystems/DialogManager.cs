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
                //�̹� job�� ������
                Debug.Log("��ϵ� Job�� �����ϴ�. ���� �̺�Ʈ ����");
            }
        }
    }
}
