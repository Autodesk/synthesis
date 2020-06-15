//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//using Synthesis.Simulator.Input;

//public class TestInputUI : MonoBehaviour
//{
//    public Text buttonText;
//    public Text readoutText;
//    public Text readout2Text;

//    private bool read = false;
//    private IAxisInput AssignableAxis;

//    private readonly KeyCode[] KeysToIgnore = new KeyCode[]
//    {
//        KeyCode.Return,
//        KeyCode.Mouse0
//    };
//    private readonly string[] AxesToIgnore = new string[]
//    {
//        "Mouse X",
//        "Mouse Y",
//        "Mouse ScrollWheel",
//        "Vertical",
//        "Horizontal"
//    };

//    public void ToggleReadInput()
//    {
//        read = !read;
//        buttonText.text = "Read:\n" + read;
//        if (!read)
//        {
//            if (AssignableAxis != null)
//            {
//                Debug.Log(AssignableAxis.ToString());
//                IAxisInput previous = InputHandler.MappedAxes["ZoomCamera"];
//                try
//                {
//                    InputHandler.MappedAxes["ZoomCamera"] = (DualAxis)(((DualAxis)previous).PositiveAxis, AssignableAxis);
//                } catch (Exception e)
//                {
//                    InputHandler.MappedAxes["ZoomCamera"] = (DualAxis)(DeadAxis.Instance, AssignableAxis);
//                }
//            }
//            else
//            {
//                Debug.Log("No axis detected");
//            }
//        }
//    }

//    public void Update()
//    {
//        /*
//        float a = Input.GetAxis("Joystick 2 Axis 4");
//        float b = Input.GetAxis("Joystick 2 Axis 5");

//        readoutText.text = "4: " + a;
//        readout2Text.text = "5: " + b;

//        return;
//        */
        
//        if (read)
//        {
//            IAxisInput detected = InputHandler.GetCurrentlyActiveAxisInput(keysToIgnore: KeysToIgnore, axesToIgnore: AxesToIgnore);
//            if (detected != null)
//            {
//                AssignableAxis = detected;
//            }
//        }
//        if (AssignableAxis != null)
//        {
//            readoutText.text = "VALUE: " + AssignableAxis.GetValue(true);
//        }
//    }
//}
