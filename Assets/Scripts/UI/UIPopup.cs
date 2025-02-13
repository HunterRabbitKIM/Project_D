using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopup : UIBase
{
    public bool IsActive { get; private set; }

    public override void Active()
    {
        base.Active();
        IsActive = true;
    }

    public override void Deactive()
    {
        IsActive = false;
        base.Deactive();
    }
}
