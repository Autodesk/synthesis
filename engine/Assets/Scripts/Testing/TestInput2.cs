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
        if(e is DigitalStateEvent digitalStateEvent)
            Debug.Log($"DigitalStateEventCallback {digitalStateEvent.Name} {digitalStateEvent.KeyState}");
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
        DigitalState state = key.GetState();
        if (state != DigitalState.None)
        {
            // Debug.Log(state.ToString());
        }
    }
}
