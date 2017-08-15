using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScroll : MonoBehaviour
{
    public void BeginDrag()
    {
        DynamicCamera.MovingEnabled = false;
    }

    public void EndDrag()
    {
        DynamicCamera.MovingEnabled = true;
    }
}
