using UnityEngine;



public class InputControlDemo_PlayerScript : MonoBehaviour
{
    private CharacterController controller;



    // Use this for initialization
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 movement = new Vector3();

        movement.x = InputControl.GetAxis(DemoControls.axes.horizontal) * 5;

        if (InputControl.GetButton(DemoControls.buttons.jump))
        {
            movement.y = 5;
        }
        else
        {
            movement.y = -5;
        }

        movement.z = InputControl.GetAxis(DemoControls.axes.vertical) * 5;

        controller.Move(movement * Time.deltaTime);
    }
}