using UnityEngine;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Digital;
using SynthesisAPI.EventBus;
using SynthesisAPI.InputManager.Events;

public class TestInput2 : MonoBehaviour
{
    private KeyDigital key;

    public void DigitalStateEventCallback(IEvent e)
    {
        Debug.Log($"DigitalStateEventCallback {e.GetArguments()[0]} {e.GetArguments()[1]}");
    }

    // Start is called before the first frame update
    public void Start()
    {
        key = KeyCode.Space;

        InputManager.AssignDigital("test", key, DigitalStateEventCallback);
    }

    // Update is called once per frame
    public void Update()
    {
        InputManager.DigitalState state = key.GetState();
        if (state != InputManager.DigitalState.None)
        {
            // Debug.Log(state.ToString());
        }
    }
}
