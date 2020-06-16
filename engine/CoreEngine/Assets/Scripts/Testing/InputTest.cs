using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Synthesis.Simulator.Input;

public class InputTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        InputHandler.MappedDigital[TestEventA] = (KeyDigital)KeyCode.Space;
        InputHandler.MappedDigital[(KeyDigital)new KeyCode[] { KeyCode.A, KeyCode.S }] = TestEventB;

        InputHandler.MappedAxes["TestAxis"] = (JoystickAxis)(1, 1, true);
    }

    private void TestEventA(object s, KeyAction keystate)
    {
        if (keystate == KeyAction.Down) Debug.Log("Test Event A Called");
    }

    private void TestEventB(object s, KeyAction keystate)
    {
        if (keystate == KeyAction.Down) Debug.Log("Test Event B Called");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Debug.Log(InputHandler.MappedAxes["TestAxis"].GetValue());
    }
}
