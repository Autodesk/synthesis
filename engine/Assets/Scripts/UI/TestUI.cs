using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Synthesis.UI;

public class TestUI : UIContainer
{
    public TestUI(): base(ObjectLedger.Instance.GetObject("TestPanel")) { }

    public void onSayHiClicked()
    {
        Debug.Log("HIIIII");
    }
}
