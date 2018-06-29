using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using Assets.Scripts.Utils;
using Assets.Scripts.FSM;
using System.Reflection;

/// <summary>
/// This is the class that handles nearly everything within the main menu scene such as ui objects, transitions, and loading fields/robots.
/// </summary>
public class MainMenu : MonoBehaviour
{
    private GameObject splashScreen;
    private Canvas canvas;

    /// <summary>
    /// Runs every frame to update the GUI elements.
    /// </summary>
    void OnGUI()
    {
        //Renders the message manager which displays error messages
        UserMessageManager.Render();
        UserMessageManager.scale = canvas.scaleFactor;
    }

    /// <summary>
    /// Switches to the home tab and its respective UI elements.
    /// </summary>
    public void SwitchTabHome()
    {
        StateMachine.Instance.ChangeState(new HomeTabState());
    }

    /// <summary>
    /// Switches to the error screen and its respective UI elements.
    /// </summary>
    public void SwitchErrorScreen()
    {
        StateMachine.Instance.PushState(new ErrorScreenState());
    }

    /// <summary>
    /// Switches to the sim tab and its respective UI elements.
    /// </summary>
    public void SwitchTabSim()
    {
        StateMachine.Instance.ChangeState(new SimTabState());
    }

    /// <summary>
    /// Switches to the options tab and its respective UI elements.
    /// </summary>
    public void SwitchTabOptions()
    {
        StateMachine.Instance.ChangeState(new OptionsTabState());
    }
    
    //Exits the program
    public void Exit()
    {
        if (!Application.isEditor)
            System.Diagnostics.Process.GetCurrentProcess().Kill();
    }

    /// <summary>
    /// Called at initialization of the MainMenu scene. Initializes all variables and loads pre-existing settings if they exist.
    /// </summary>
    void Start()
    {
        LinkTabs();
        FindAllGameObjects();
        RegisterButtonCallbacks();
        splashScreen.SetActive(true); //Turns on the loading screen while initializing

        //Creates the replay directory
        FileInfo file = new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Synthesis\\Replays\\");
        file.Directory.Create();

        //Assigns the currently store registry values or default file path to the proper variables if they exist.
        string robotDirectory = PlayerPrefs.GetString("RobotDirectory", (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "//synthesis//Robots"));
        string fieldDirectory = PlayerPrefs.GetString("FieldDirectory", (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "//synthesis//Fields"));

        //If the directory doesn't exist, create it.
        if (!Directory.Exists(robotDirectory))
        {
            file = new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Synthesis\\Robots\\");
            file.Directory.Create();
            robotDirectory = file.Directory.FullName;
        }
        if (!Directory.Exists(fieldDirectory))
        {
            file = new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Synthesis\\Fields\\");
            file.Directory.Create();
            fieldDirectory = file.Directory.FullName;
        }
        //Saves the current directory information to the registry
        PlayerPrefs.SetString("RobotDirectory", robotDirectory);
        PlayerPrefs.SetString("FieldDirectory", fieldDirectory);

        canvas = GetComponent<Canvas>();

        splashScreen.SetActive(false);

        //This makes it so that if the user exits from the simulator, 
        //they are put into the panel where they can select a robot/field
        //In all other cases, users are welcomed with the main menu screen.
        if (!string.IsNullOrEmpty(AppModel.ErrorMessage))
            SwitchErrorScreen();
        else
            StateMachine.Instance.PushState(new HomeTabState());
    }

    /// <summary>
    /// Links individual tab components with their respective <see cref="State"/>s.
    /// </summary>
    private void LinkTabs()
    {
        LinkTab<HomeTabState>("HomeTab");
        LinkTab<SimTabState>("SimTab", false);
        LinkTab<OptionsTabState>("OptionsTab");
        LinkTab<ErrorScreenState>("ErrorScreen");
        LinkTab<SelectionState>("SelectionPanel");
        LinkTab<DefaultSimulatorState>("DefaultSimulator");
        LinkTab<MixAndMatchState>("MixAndMatchMode");
        LinkTab<LoadReplayState>("SimLoadReplay");
        LinkTab<LoadRobotState>("SimLoadRobot");
        LinkTab<LoadFieldState>("SimLoadField");
    }

    /// <summary>
    /// Links a tab to the provided <see cref="State"/> type from the tab's name.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tabName"></param>
    private void LinkTab<T>(string tabName, bool strict = true) where T : State
    {
        GameObject tab = AuxFunctions.FindGameObject(tabName);

        if (tab != null)
            StateMachine.Instance.Link<T>(tab, strict);
    }

    /// <summary>
    /// Finds each Button component in the main menu that doesn't already have a
    /// listener and registers it with a callback.
    /// </summary>
    private void RegisterButtonCallbacks()
    {
        foreach (Button b in GetComponentsInChildren<Button>(true))
            if (b.onClick.GetPersistentEventCount() == 0)
                b.onClick.AddListener(() => InvokeCallback("On" + b.name + "Pressed"));
    }

    /// <summary>
    /// Invokes a method in the active <see cref="State"/> by the given method name.
    /// </summary>
    /// <param name="methodName"></param>
    private void InvokeCallback(string methodName)
    {
        State currentState = StateMachine.Instance.CurrentState;
        MethodInfo info = currentState.GetType().GetMethod(methodName);

        if (info == null)
            Debug.LogWarning("Method " + methodName + " does not have a listener in " + currentState.GetType().ToString());
        else
            info.Invoke(currentState, null);
    }

    /// <summary>
    /// Finds all the UI game objects and assigns them to a variable
    /// </summary>
    void FindAllGameObjects()
    {
        splashScreen = AuxFunctions.FindObject(gameObject, "LoadSplash");
        AuxFunctions.FindObject(gameObject, "QualitySettingsText").GetComponent<Text>().text = QualitySettings.names[QualitySettings.GetQualityLevel()];
    }
}