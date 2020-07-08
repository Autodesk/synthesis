using UnityEngine;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Digital;

public class TestInput2 : MonoBehaviour
{
    private KeyDigital key;

    // Start is called before the first frame update
    public void Start()
    {
        key = KeyCode.Space;
    }

    // Update is called once per frame
    public void Update()
    {
        InputManager.DigitalState state = key.GetState();
        if (state != InputManager.DigitalState.None)
        {
            Debug.Log(state.ToString());
        }
    }
}
