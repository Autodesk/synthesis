using System.Collections;
using System.Collections.Generic;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using UnityEngine;
using Logger = SynthesisAPI.Utilities.Logger;

public class TestScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        InputManager.AssignValueInput("test_input_1", new Digital("W"));
    }

    // Update is called once per frame
    void Update()
    {
        Logger.Log(InputManager.MappedValueInputs["test_input_1"].Value);
    }
}
