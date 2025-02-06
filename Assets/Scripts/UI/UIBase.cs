using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIBase : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        UIManager.Instance.RegisterUI(this);
    }

    public virtual void Active()
    {
        gameObject.SetActive(true);
    }

    public virtual void Deactive()
    {
        gameObject.SetActive(false);
    }
}
