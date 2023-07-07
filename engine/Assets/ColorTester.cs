using System.Collections;
using System.Collections.Generic;
using SynthesisAPI.EventBus;
using UnityEngine;
using Utilities.ColorManager;

public class ColorTester : MonoBehaviour
{
    void Start()
    {
        /*ColorManager.AssignColor(ColorManager.SynthesisColor.SynthesisOrange, (c) =>
        {
            Debug.Log($"Color assigned to {c}");
        });*/
        ColorManager.GetColor(ColorManager.SynthesisColor.SynthesisHighlightSelect);
        //var col = ColorManager.GetColor(ColorManager.SynthesisColor.SynthesisOrange);
        //Debug.Log(col);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            ColorManager.SelectedTheme = "another_test_theme";
        }
    }
}
