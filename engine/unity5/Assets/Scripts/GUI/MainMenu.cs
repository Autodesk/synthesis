using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using System;
using UnityEngine.SceneManagement;
using Assets.Scripts;
using Assets.Scripts.Utils;

/// <summary>
/// This is the class that handles nearly everything within the main menu scene such as ui objects, transitions, and loading fields/robots.
/// </summary>
public class MainMenu : MonoBehaviour
{

    //This refers to what tab the main menu is currently in.
    public enum Tab { Main, Sim, Options, FieldDir, RobotDir, ErrorScreen };
    public static Tab currentTab = Tab.Main;

    public static bool isMixAndMatchTab = false;
    public GameObject mixAndMatchModeScript;

    //These refer to the parent gameobjects; each of them contain all the UI objects of the main menu state they are representing.
    //We store these because it allows us to easily find and access specific UI objects.
    public GameObject homeTab;
    public GameObject simTab;
    public GameObject optionsTab;

    //This refers to what 'state' or 'page' the main menu is in while it is in the 'Sim' tab.

    public enum Sim
    {
        Selection, DefaultSimulator, MixAndMatchMode, SimLoadRobot,
        SimLoadField, CustomFieldLoader, SimLoadReplay
    }
   
    public static Sim currentSim = Sim.Selection;
    Sim lastSim;

    //These are necessary references to specific UI parent objects.
    //We disable and enable these when the state of the main menu changes to reflect the user's selection.
    private GameObject navigationPanel;
    private GameObject selectionPanel;
    private GameObject defaultSimulator;
    private GameObject mixAndMatchMode;
    private GameObject simLoadField;
    private GameObject simLoadRobot;
    private GameObject simLoadReplay;
    private GameObject errorScreen;

    //We alter these to reflect the user's selected fields and robots.
    private GameObject simRobotSelectText;
    private GameObject simFieldSelectText;

    //This reflects the state of driver practice mode configuration and displays what field a robot is configured for.
    private GameObject configurationText;

    private GameObject graphics; //The Graphics GUI Objects
    private GameObject input; //The Input GUI Objects

    private GameObject settingsMode; //The InputManager Objects
    private Text errorText; // The text of the error message

    private GameObject splashScreen; //A panel that shows up at the start to cover the screen while initializing everything.

    private ArrayList fields; //ArrayList of field folders to select
    private ArrayList robots; //ArrayList of robot folders to select

    //Variables for Default Simulator mode
    private string simSelectedField; //the selected field file path
    private string simSelectedRobot; //the selected robot file path
    private string simSelectedFieldName; //the selected field name
    private string simSelectedRobotName; //the selected robot name

    private FileBrowser fieldBrowser = null; //field directory browser
    private bool customfieldon = true; //whether the field directory browser is on
    public string fieldDirectory; //file path for field directory

    private FileBrowser robotBrowser = null; //robot directory browser
    private bool customroboton = true; //whether the robot directory browser is on
    public string robotDirectory; //file path for robot directory

    public static bool fullscreen; //true if application is in fullscreen
    public static int resolutionsetting; //resolution setting index
    private int[] xresolution = new int[9]; //arrays of resolution widths corresponding to index
    private int[] yresolution = new int[9]; //arrays of resolution heights corresponding to index

    private Canvas canvas; //canvas component of this object--used for scaling user message manager to size


    /// <summary>
    /// Runs every frame to update the GUI elements.
    /// </summary>
    void OnGUI()
    {
        switch (currentTab)
        {
            //Switches back to sim tab UI elements if field browser is closed
            case Tab.FieldDir:
                if (customfieldon && !fieldBrowser.Active)
                {
                    currentTab = Tab.Sim;

                    homeTab.SetActive(false);
                    optionsTab.SetActive(false);
                    simTab.SetActive(true);
                    customfieldon = false;
                }
                break;

            //Switches back to sim tab UI elements if robot directory is closed
            case Tab.RobotDir:
                if (customroboton && !robotBrowser.Active)
                {
                    currentTab = Tab.Sim;

                    homeTab.SetActive(false);
                    optionsTab.SetActive(false);
                    simTab.SetActive(true);
                    customroboton = false;
                }
                break;
        }

        //Initializes and renders the Field Browser
        if (fieldDirectory != null) InitFieldBrowser();
        if (robotDirectory != null) InitRobotBrowser();


        //Renders the message manager which displays error messages
        UserMessageManager.Render();
        UserMessageManager.scale = canvas.scaleFactor;
    }

    /// <summary>
    /// Switches to the home tab and its respective UI elements.
    /// </summary>
    public void SwitchTabHome()
    {
        if (currentTab != Tab.RobotDir && currentTab != Tab.FieldDir) //checks if directory browser is active
        {
            currentTab = Tab.Main;

            errorScreen.SetActive(false);
            simTab.SetActive(false);
            optionsTab.SetActive(false);
            navigationPanel.SetActive(true);
            homeTab.SetActive(true);
        }
        else UserMessageManager.Dispatch("You must select a directory or exit first!", 3);
    }

    /// <summary>
    /// Switches to the error screen and its respective UI elements.
    /// </summary>
    public void SwitchErrorScreen()
    {
        currentTab = Tab.ErrorScreen;

        navigationPanel.SetActive(false);
        homeTab.SetActive(false);
        optionsTab.SetActive(false);
        simTab.SetActive(false);
        errorText.text = AppModel.ErrorMessage;
        errorScreen.SetActive(true);

        AppModel.ClearError();
    }

    /// <summary>
    /// Switches to the sim tab and its respective UI elements.
    /// </summary>
    public void SwitchTabSim()
    {
        if (currentTab != Tab.RobotDir && currentTab != Tab.FieldDir) //checks if directory browser is active
        {
            currentTab = Tab.Sim;

            homeTab.SetActive(false);
            optionsTab.SetActive(false);
            simTab.SetActive(true);
        }
        else UserMessageManager.Dispatch("You must select a directory or exit first!", 3);
    }

    /// <summary>
    /// Switches to the options tab and its respective UI elements.
    /// </summary>
    public void SwitchTabOptions()
    {
        if (currentTab != Tab.RobotDir && currentTab != Tab.FieldDir) //checks if directory browser is active
        {
            currentTab = Tab.Options;

            homeTab.SetActive(false);
            simTab.SetActive(false);
            optionsTab.SetActive(true);
            settingsMode.SetActive(true);
            GameObject.Find("SettingsMode").GetComponent<SettingsMode>().GetLastSavedControls();
        }
        else UserMessageManager.Dispatch("You must select a directory or exit first!", 3);
    }

    /// <summary>
    /// Switches to the selection menu within the simulation tab and activates its respective UI elements.
    /// </summary>
    public void SwitchSimSelection()
    {
        currentSim = Sim.Selection;

        defaultSimulator.SetActive(false);

        simLoadField.SetActive(false);
        simLoadRobot.SetActive(false);
        simLoadReplay.SetActive(false);
        mixAndMatchMode.SetActive(false);

        selectionPanel.SetActive(true);

        isMixAndMatchTab = false;
    }

    /// <summary>
    /// Switches to the default simulator menu within the simulation tab and activates its respective UI elements.
    /// </summary>
    public void SwitchSimDefault()
    {
        if (!isMixAndMatchTab)
        {
            currentSim = Sim.DefaultSimulator;

            selectionPanel.SetActive(false);
            simLoadField.SetActive(false);
            simLoadRobot.SetActive(false);
            simLoadReplay.SetActive(false);
            defaultSimulator.SetActive(true);

            PlayerPrefs.SetString("simSelectedRobot", simSelectedRobot);
            PlayerPrefs.SetString("simSelectedField", simSelectedField);


            simRobotSelectText.GetComponent<Text>().text = simSelectedRobotName;
            simFieldSelectText.GetComponent<Text>().text = simSelectedFieldName;

            RobotTypeManager.SetProperties(false);
        }
        else
        {
            SwitchMixAndMatch();
        }
    }

    /// <summary>
    /// Switches to the Mix and Match menu within the simulation tab and activates its respective UI elements.
    /// </summary>
    public void SwitchMixAndMatch()
    {
        currentSim = Sim.MixAndMatchMode;
        
        selectionPanel.SetActive(false);
        simLoadField.SetActive(false);
        simLoadRobot.SetActive(false);
        simLoadReplay.SetActive(false);
        mixAndMatchMode.SetActive(true);

        RobotTypeManager.SetProperties(true);
        isMixAndMatchTab = true;

    }

    /// <summary>
    /// Switches to the load robot menu for the default simulator and activates its respective UI elements.
    /// </summary>
    public void SwitchSimLoadRobot()
    {
        currentSim = Sim.SimLoadRobot;

        defaultSimulator.SetActive(false);
        simLoadRobot.SetActive(true);
    }

    /// <summary>
    /// Switches to the load field menu for the default simulator and activates its respective UI elements.
    /// </summary>
    public void SwitchSimLoadField()
    {
        currentSim = Sim.SimLoadField;

        defaultSimulator.SetActive(false);
        simLoadField.SetActive(true);
    }

    /// <summary>
    /// Switches to the load field menu for the default simulator and activates its respective UI elements.
    /// </summary>
    public void SwitchSimLoadField(bool isMaM)
    {
        currentSim = Sim.SimLoadField;

        defaultSimulator.SetActive(false);
        simLoadField.SetActive(true);
        mixAndMatchMode.SetActive(false);
    }

    /// <summary>
    /// Switches to the load replay menu for the default simulator and activates its respective UI elements.
    /// </summary>
    public void SwitchSimLoadReplay()
    {
        currentSim = Sim.SimLoadReplay;

        defaultSimulator.SetActive(false);
        simLoadReplay.SetActive(true);
    }

    /// <summary>
    /// Switches to the field directory browser
    /// </summary>
    public void SwitchFieldDir()
    {
        currentTab = Tab.FieldDir;

        homeTab.SetActive(false);
        simTab.SetActive(false);
        optionsTab.SetActive(false);

        customfieldon = true;
        fieldBrowser.Active = true;
    }

    //Switches to the robot directory browser
    public void SwitchRobotDir()
    {
        currentTab = Tab.RobotDir;

        homeTab.SetActive(false);
        simTab.SetActive(false);
        optionsTab.SetActive(false);

        customroboton = true;
        robotBrowser.Active = true;
    }

    /// <summary>
    /// Switches to the graphics settings panel
    /// </summary>
    public void SwitchGraphics()
    {
        graphics.SetActive(true);
        input.SetActive(false);
        settingsMode.SetActive(true);
    }

    /// <summary>
    /// Switches to the input settings panel
    /// </summary>
    public void SwitchInput()
    {
        graphics.SetActive(false);
        input.SetActive(true);
        settingsMode.SetActive(true);
    }

    /// <summary>
    /// If robot and field is properly selected, switch to the default simulator and save robot/field directory information
    /// </summary>
    public void StartDefaultSim()
    {
        if (Directory.Exists(simSelectedField) && Directory.Exists(simSelectedRobot))
        {
            splashScreen.SetActive(true);
            PlayerPrefs.SetString("simSelectedReplay", string.Empty);
            PlayerPrefs.SetString("simSelectedField", simSelectedField);
            PlayerPrefs.SetString("simSelectedFieldName", simSelectedFieldName);
            PlayerPrefs.SetString("simSelectedRobot", simSelectedRobot);
            PlayerPrefs.SetString("simSelectedRobotName", simSelectedRobotName);
            PlayerPrefs.Save();
            SceneManager.LoadScene("Scene");

            RobotTypeManager.SetProperties(false);

        }
        else UserMessageManager.Dispatch("No Robot/Field Selected!", 2);
    }



    #region Main Tab Button Methods

    //Exits the program
    public void Exit()
    {
        if (!Application.isEditor)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }

    #endregion
    #region LoadRobot and LoadField Button Methods

    /// <summary>
    /// Initializes and runs the custom field directory browser
    /// </summary>
    public void InitFieldBrowser()
    {
        if (fieldBrowser == null)
        {
            fieldBrowser = new FileBrowser("Choose Field Directory", fieldDirectory, true);
            fieldBrowser.Active = true;
            fieldBrowser.OnComplete += (object obj) =>
            {
                fieldBrowser.Active = true;
                string fileLocation = (string)obj;
                // If dir was selected...
                DirectoryInfo directory = new DirectoryInfo(fileLocation);
                if (directory != null && directory.Exists)
                {
                    Debug.Log(directory);
                    fieldDirectory = (directory.FullName);
                    currentTab = Tab.Sim;
                    SwitchTabSim();
                    customfieldon = false;
                    PlayerPrefs.SetString("FieldDirectory", fieldDirectory);
                    PlayerPrefs.Save();
                }
                else
                {
                    UserMessageManager.Dispatch("Invalid selection!", 10f);
                }
            };
        }
        if (customfieldon) fieldBrowser.Render();
    }

    /// <summary>
    /// Starts the custom field directory browser
    /// </summary>
    public void LoadFieldDirectory()
    {
        if (!fieldBrowser.Active)
        {
            fieldBrowser.Active = true;
        }
        customfieldon = true;
        currentTab = Tab.FieldDir;
    }

    /// <summary>
    /// Initializes and runs the custom robot directory browser
    /// </summary>
    public void InitRobotBrowser()
    {
        if (robotBrowser == null)
        {
            robotBrowser = new FileBrowser("Choose Robot Directory", robotDirectory, true);
            robotBrowser.Active = true;
            robotBrowser.OnComplete += (object obj) =>
            {
                robotBrowser.Active = true;
                string fileLocation = (string)obj;
                // If dir was selected...
                DirectoryInfo directory = new DirectoryInfo(fileLocation);
                if (directory != null && directory.Exists)
                {
                    robotDirectory = (directory.FullName);
                    currentTab = Tab.Sim;
                    SwitchTabSim();
                    customroboton = false;
                    PlayerPrefs.SetString("RobotDirectory", robotDirectory);
                    PlayerPrefs.Save();
                }
                else
                {
                    UserMessageManager.Dispatch("Invalid selection!", 10f);
                }
            };
        }
        if (customroboton) robotBrowser.Render();
    }

    /// <summary>
    /// Starts the custom robot directory browser
    /// </summary>
    public void LoadRobotDirectory()
    {
        if (!robotBrowser.Active)
        {
            robotBrowser.Active = true;
        }
        customroboton = true;
        currentTab = Tab.RobotDir;
    }

    #endregion
    #region Other Methods
    /// <summary>
    /// Resets to default inputs (Arcade Drive for now)
    /// </summary>
    public void InputDefaultPressed()
    {
        Controls.ArcadeDrive();
    }

    /// <summary>
    /// Applies the currently selected graphics settings
    /// </summary>
    public void ApplyGraphics()
    {
        Screen.SetResolution(xresolution[resolutionsetting], yresolution[resolutionsetting], fullscreen);
        PlayerPrefs.SetInt("fullscreen", (fullscreen ? 1 : 0));
        splashScreen.SetActive(true);
        StartCoroutine(HideSplashScreen(1));
        SwitchTabHome();
    }

    /// <summary>
    /// Hides the splash screen after a certain amount of seconds (meant for transitions)
    /// </summary>
    IEnumerator HideSplashScreen(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        splashScreen.SetActive(false);
    }

    //Various functions for linking to website
    public void OpenWebsite()
    {
        //System.Diagnostics.Process.Start("\\..\\FieldExporter\\Inventor_Exporter.exe");
        Application.OpenURL("http://bxd.autodesk.com/");
    }

    public void OpenTutorials()
    {
        Application.OpenURL("http://bxd.autodesk.com/tutorials.html");
    }

    public void OpenRobotExportTutorial()
    {
        Application.OpenURL("http://bxd.autodesk.com/synthesis/tutorials-robot.html");
    }

    public void OpenFieldExportTutorial()
    {
        Application.OpenURL("http://bxd.autodesk.com/synthesis/tutorials-field.html");
    }

    /// <summary>
    /// Called when the "Select Field" button is clicked within the field selection panel
    /// If not in Mix and Match Mode, Saves the currently selected value in the panel and switches back to the previous panel
    /// It in Mix and Match Mode, starts the simulation for Mix and Match 
    /// </summary>
    public void SelectSimField()
    {
        GameObject fieldList = GameObject.Find("SimLoadFieldList");
        string entry = (fieldList.GetComponent<SelectFieldScrollable>().selectedEntry);
        if (entry != null)
        {
            simSelectedFieldName = fieldList.GetComponent<SelectFieldScrollable>().selectedEntry;
            simSelectedField = fieldDirectory + "\\" + simSelectedFieldName + "\\";

            if (isMixAndMatchTab) //Starts the MixAndMatch scene
            {
                PlayerPrefs.SetString("simSelectedField", simSelectedField);
                fieldList.SetActive(false);
                splashScreen.SetActive(true);
                mixAndMatchModeScript.GetComponent<MixAndMatchMode>().StartMaMSim();
            } else
            {
                SwitchSimDefault();
            }            
        }
        else
        {
            UserMessageManager.Dispatch("No Field Selected!", 2);
        }
        
    }

    /// <summary>
    /// Called when the "Select Robot" button is clicked within the robot selection panel
    /// Saves the currently selected value in the panel and switches back to the previous panel
    /// </summary>
    public void SelectSimRobot()
    {
        GameObject robotList = GameObject.Find("SimLoadRobotList");
        string entry = (robotList.GetComponent<SelectRobotScrollable>().selectedEntry);
        if (entry != null)
        {
            simSelectedRobotName = robotList.GetComponent<SelectRobotScrollable>().selectedEntry;
            simSelectedRobot = robotDirectory + "\\" + simSelectedRobotName + "\\";
            SwitchSimDefault();
        }
        else
        {
            UserMessageManager.Dispatch("No Robot Selected!", 2);
        }
    }

    /// <summary>
    /// Called when the "Select Replay" button is clicked within the replay selection panel
    /// Scene switches to the Scene.unity to load the replay
    /// </summary>
    public void SelectSimReplay()
    {
        GameObject replayList = GameObject.Find("SimLoadReplayList");
        string entry = replayList.GetComponent<ScrollableList>().selectedEntry;

        if (entry != null)
        {
            simLoadReplay.SetActive(false);
            splashScreen.SetActive(true);
            PlayerPrefs.SetString("simSelectedReplay", entry);

            PlayerPrefs.Save();
            Application.LoadLevel("Scene");
        }
    }

    /// <summary>
    /// Called when the "Delete replay" button is clicked within the replay selection panel
    /// Deletes the selected replay
    /// </summary>
    public void SelectDeleteReplay()
    {
        GameObject replayList = GameObject.Find("SimLoadReplayList");
        string entry = replayList.GetComponent<ScrollableList>().selectedEntry;

        if (entry != null)
        {
            File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Synthesis\\Replays\\" + entry + ".replay");
            replayList.SetActive(false);
            replayList.SetActive(true);
        }
    }
    #endregion

    /// <summary>
    /// Called at initialization of the MainMenu scene. Initializes all variables and loads pre-existing settings if they exist.
    /// </summary>
    void Start()
    {
        FindAllGameObjects();
        splashScreen.SetActive(true); //Turns on the loading screen while initializing
        InitGraphicsSettings();
        fields = new ArrayList();
        robots = new ArrayList();

        //Creates the replay directory
        FileInfo file = new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Synthesis\\Replays\\");
        file.Directory.Create();

        //Assigns the currently store registry values or default file path to the proper variables if they exist.
        robotDirectory = PlayerPrefs.GetString("RobotDirectory", (System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "//synthesis//Robots"));
        fieldDirectory = PlayerPrefs.GetString("FieldDirectory", (System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "//synthesis//Fields"));

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

        //Assigns the currently stored registry values for the selected field/robot to the proper variables.
        simSelectedField = PlayerPrefs.GetString("simSelectedField");
        simSelectedFieldName = (Directory.Exists(simSelectedField)) ? PlayerPrefs.GetString("simSelectedFieldName", "No Field Selected!") : "No Field Selected!";
        simSelectedRobot = PlayerPrefs.GetString("simSelectedRobot");
        simSelectedRobotName = (Directory.Exists(simSelectedRobot)) ? PlayerPrefs.GetString("simSelectedRobotName", "No Robot Selected!") : "No Robot Selected!";

        canvas = GetComponent<Canvas>();

        customfieldon = false;
        customroboton = false;
        ApplyGraphics();

        //Checks if this is the first launch of the main scene.
        if (AppModel.InitialLaunch)
        {
            AppModel.InitialLaunch = false;

            //Loads robot and field directories from command line arguments if valid.
            string[] args = Environment.GetCommandLineArgs();
            bool robotDefined = false;
            bool fieldDefined = false;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-robot":
                        if (i < args.Length - 1)
                        {
                            string robotFile = args[++i];

                            DirectoryInfo dirInfo = new DirectoryInfo(robotFile);
                            robotDirectory = dirInfo.Parent.FullName;
                            PlayerPrefs.SetString("RobotDirectory", robotDirectory);
                            simSelectedRobot = robotFile;
                            simSelectedRobotName = dirInfo.Name;
                            robotDefined = true;
                        }
                        break;
                    case "-field":
                        if (i < args.Length - 1)
                        {
                            string fieldFile = args[++i];

                            DirectoryInfo dirInfo = new DirectoryInfo(fieldFile);
                            fieldDirectory = dirInfo.Parent.FullName;
                            PlayerPrefs.SetString("FieldDirectory", fieldDirectory);
                            simSelectedField = fieldFile;
                            simSelectedFieldName = dirInfo.Name;
                            fieldDefined = true;
                        }
                        break;
                }
            }

            //If command line arguments have been passed, start the simulator.
            if (robotDefined && fieldDefined)
            {
                StartDefaultSim();
                return;
            }
        }

        //This makes it so that if the user exits from the simulator, 
        //they are put into the panel where they can select a robot/field
        //In all other cases, users are welcomed with the main menu screen.
        if (!string.IsNullOrEmpty(AppModel.ErrorMessage))
        {
            SwitchErrorScreen();
        }
        else if (currentSim == Sim.DefaultSimulator)
        {
            SwitchTabSim();
            SwitchSimSelection();
            SwitchSimDefault();
        }
        else
        {
            SwitchSimSelection();
            SwitchTabHome();
        }
    }

    /// <summary>
    /// Finds all the UI game objects and assigns them to a variable
    /// </summary>
    void FindAllGameObjects()
    {
        //We need to make refernces to various buttons/text game objects, but using GameObject.Find is inefficient if we do it every update.
        //Therefore, we assign variables to them and only use GameObject.Find once for each object in startup.
        navigationPanel = AuxFunctions.FindObject(gameObject, "NavigationPanel");
        selectionPanel = AuxFunctions.FindObject(gameObject, "SelectionPanel"); //The Mode Selection Tab GUI Objects
        defaultSimulator = AuxFunctions.FindObject(gameObject, "DefaultSimulator");
        mixAndMatchMode = AuxFunctions.FindObject(gameObject, "MixAndMatchMode");
        simLoadField = AuxFunctions.FindObject(gameObject, "SimLoadField");
        simLoadRobot = AuxFunctions.FindObject(gameObject, "SimLoadRobot");
        simLoadReplay = AuxFunctions.FindObject(gameObject, "SimLoadReplay");
        splashScreen = AuxFunctions.FindObject(gameObject, "LoadSplash");
        errorScreen = AuxFunctions.FindObject(gameObject, "ErrorScreen");

        graphics = AuxFunctions.FindObject(gameObject, "Graphics");
        input = AuxFunctions.FindObject(gameObject, "Input");

        settingsMode = AuxFunctions.FindObject(gameObject, "SettingsMode");
        errorText = AuxFunctions.FindObject(errorScreen, "ErrorText").GetComponent<Text>();

        simFieldSelectText = AuxFunctions.FindObject(defaultSimulator, "SimFieldSelectText");
        simRobotSelectText = AuxFunctions.FindObject(defaultSimulator, "SimRobotSelectText");

        AuxFunctions.FindObject(gameObject, "QualitySettingsText").GetComponent<Text>().text = QualitySettings.names[QualitySettings.GetQualityLevel()];

        mixAndMatchModeScript = AuxFunctions.FindObject(gameObject, "MixAndMatchModeScript");
        Debug.Log(mixAndMatchModeScript.ToString());
    }

    /// <summary>
    /// Initializes graphics settings
    /// </summary>
    void InitGraphicsSettings()
    {
        xresolution[0] = 1024;
        xresolution[1] = 1280;
        xresolution[2] = 1280;
        xresolution[3] = 1280;
        xresolution[4] = 1400;
        xresolution[5] = 1600;
        xresolution[6] = 1680;
        xresolution[7] = 1920;
        xresolution[8] = Screen.currentResolution.width;

        yresolution[0] = 768;
        yresolution[1] = 720;
        yresolution[2] = 768;
        yresolution[3] = 1024;
        yresolution[4] = 900;
        yresolution[5] = 900;
        yresolution[6] = 1050;
        yresolution[7] = 1080;
        yresolution[8] = Screen.currentResolution.height;

        fullscreen = (PlayerPrefs.GetInt("fullscreen", 0) == 1);
        int width = xresolution[8];
        int height = yresolution[8];
        if (width == xresolution[0] && height == yresolution[0]) resolutionsetting = 0;
        else if (width == xresolution[1] && height == yresolution[1]) resolutionsetting = 1;
        else if (width == xresolution[2] && height == yresolution[2]) resolutionsetting = 2;
        else if (width == xresolution[3] && height == yresolution[3]) resolutionsetting = 3;
        else if (width == xresolution[4] && height == yresolution[4]) resolutionsetting = 4;
        else if (width == xresolution[5] && height == yresolution[5]) resolutionsetting = 5;
        else if (width == xresolution[6] && height == yresolution[6]) resolutionsetting = 6;
        else if (width == xresolution[7] && height == yresolution[7]) resolutionsetting = 7;
        else resolutionsetting = 8;
    }

    /// <summary>
    /// Changes the quality settings in a positive increment
    /// </summary>
    public void ChangeQualitySettings()
    {
        if (QualitySettings.GetQualityLevel() < QualitySettings.names.Length - 1) QualitySettings.SetQualityLevel(QualitySettings.GetQualityLevel() + 1);
        else QualitySettings.SetQualityLevel(0);
        GameObject.Find("QualitySettingsText").GetComponent<Text>().text = QualitySettings.names[QualitySettings.GetQualityLevel()];

    }
}