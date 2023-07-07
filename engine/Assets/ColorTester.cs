using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.ColorManager;

public class ColorTester : MonoBehaviour
{
    void Start()
    {
        var col = ColorManager.GetColor(ColorManager.SynthesisColor.SynthesisOrange);
        Debug.Log(col);
    }

    void Update()
    {
        
    }
}
