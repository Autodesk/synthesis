InputControl
============

InputControl works similarly to the Unity built-in input manager and allow user to customize controls in Runtime.

Demo:              http://gris.ucoz.ru/UnityModules/InputControl/Web/InputControl.html
Unity Asset Store: http://u3d.as/7Bc

Description:

First of all, you have to extract ProjectSettings.zip file to your project.
Please move Controls.cs file somewhere to your source code and verify it contents.
You can provide a set of keys and set of axes in Controls static constructor.

Example:

static Controls()
{
    buttons.up        = InputControl.setKey("Up",        KeyCode.W,            KeyCode.UpArrow,     new JoystickInput(JoystickAxis.Axis2Negative));
    buttons.down      = InputControl.setKey("Down",      KeyCode.S,            KeyCode.DownArrow,   new JoystickInput(JoystickAxis.Axis2Positive));
    buttons.left      = InputControl.setKey("Left",      KeyCode.A,            KeyCode.LeftArrow,   new JoystickInput(JoystickAxis.Axis1Negative));
    buttons.right     = InputControl.setKey("Right",     KeyCode.D,            KeyCode.RightArrow,  new JoystickInput(JoystickAxis.Axis1Positive));
    buttons.fire1     = InputControl.setKey("Fire1",     MouseButton.Left,     KeyCode.LeftControl, new JoystickInput(JoystickButton.Button1));
    buttons.fire2     = InputControl.setKey("Fire2",     MouseButton.Right,    KeyCode.LeftAlt,     new JoystickInput(JoystickButton.Button2));
    buttons.fire3     = InputControl.setKey("Fire3",     MouseButton.Middle,   KeyCode.LeftCommand, new JoystickInput(JoystickButton.Button3));
    buttons.jump      = InputControl.setKey("Jump",      KeyCode.Space,        KeyCode.None,        new JoystickInput(JoystickButton.Button4));
    buttons.run       = InputControl.setKey("Run",       KeyCode.LeftShift,    KeyCode.RightShift,  new JoystickInput(JoystickButton.Button5));
    buttons.lookUp    = InputControl.setKey("LookUp",    MouseAxis.MouseUp,    KeyCode.None,        new JoystickInput(JoystickAxis.Axis4Negative));
    buttons.lookDown  = InputControl.setKey("LookDown",  MouseAxis.MouseDown,  KeyCode.None,        new JoystickInput(JoystickAxis.Axis4Positive));
    buttons.lookLeft  = InputControl.setKey("LookLeft",  MouseAxis.MouseLeft,  KeyCode.None,        new JoystickInput(JoystickAxis.Axis3Negative));
    buttons.lookRight = InputControl.setKey("LookRight", MouseAxis.MouseRight, KeyCode.None,        new JoystickInput(JoystickAxis.Axis3Positive));

    axes.vertical     = InputControl.setAxis("Vertical",   buttons.down,     buttons.up);
    axes.horizontal   = InputControl.setAxis("Horizontal", buttons.left,     buttons.right);
    axes.mouseX       = InputControl.setAxis("Mouse X",    buttons.lookDown, buttons.lookUp);
    axes.mouseY       = InputControl.setAxis("Mouse Y",    buttons.lookLeft, buttons.lookRight);
}

You can easy change this configuration in Runtime by calling InputControl.setKey() and InputControl.setAxis()



Please use InputControl.getKeysList() to get list of all configured buttons.
It is possible to get current active input with InputControl.currentInput():

CustomInput curInput = InputControl.currentInput();
Debug.Log(curInput == null ? "null" : curInput.ToString());



Another way to modify some button (Please check demo for details):

CustomInput curInput = InputControl.currentInput();

if (curInput != null) // if something pressed
{
    Controls.buttons.up.primaryInput = curInput; // or secondaryInput or thirdInput
}



Please note that it is better to use reference to button in InputControl.GetButton() or in InputControl.GetAxis() instead of providing its name



To invert mouse Y axis:
InputControl.invertMouseY = true;



You can setup mouse sensitivity by:
InputControl.mouseSensitivity = 0.5f;



You can change joystick threshold to avoid small joystick movements:
InputControl.joystickThreshold = 0.1f;



Built-in Input.GetAxis method provide to user smoothed Axis value. Please set smooth coefficient to simulate it.
InputControl.smoothCoefficient = 5f;

To disable smooth just set smoothCoefficient to big value or:
InputControl.smoothCoefficient = InputControl.NO_SMOOTH;



When you setup button configuration you can create KeyboardInput, MouseInput or JoystickInput instance with providing key modifiers:
buttons.copy  = InputControl.setKey("Copy",  new KeyboardInput(KeyCode.C, KeyModifier.Ctrl));
buttons.paste = InputControl.setKey("Paste", new KeyboardInput(KeyCode.V, KeyModifier.Ctrl), new KeyboardInput(KeyCode.Insert, KeyModifier.Shift));

It might be useful if you want use shortcuts in your application.

Please call InputControl.currentInput() with argument useModifiers set to true if you want to allow user to setup shortcut
Please call InputControl.getButton() or InputControl.getAxis() with argument exactKeyModifiers set to true if you want to use shortcuts



It is possible to save each button in configuration file with:
Controls.buttons.up.primaryInput.ToString()

If you want to read configuration just use KeyboardInput.FromString(), MouseInput.FromString() or JoystickInput.FromString()

Please check Controls class for save and load methods



Please feal free to contact with me if you meet some errors.
e-mail: gris87@yandex.ru



Links:

Site:              http://gris.ucoz.ru/index/inputcontrol/0-11
Unity Asset Store: http://u3d.as/7Bc
GitHub:            https://github.com/Gris87/InputControl
