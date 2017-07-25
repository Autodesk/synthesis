using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
/// <summary>
/// This is the class that handles nearly everything within the main menu scene such as ui objects, transitions, and loading fields/robots.
/// </summary>
public class MainMenu : MonoBehaviour {

    //This refers to what tab the main menu is currently in.
    public enum Tab { Main, Sim, Options, FieldDir, RobotDir};
    public static Tab currentTab = Tab.Main;

    //These refer to the parent gameobjects; each of them contain all the UI objects of the main menu state they are representing.
    //We store these because it allows us to easily find and access specific UI objects.
    public GameObject homeTab;
    public GameObject simTab;
    public GameObject optionsTab;

    //This refers to what 'state' or 'page' the main menu is in while it is in the 'Sim' tab.
    public enum Sim { Selection, DefaultSimulator, DriverPracticeMode, Multiplayer, SimLoadRobot, SimLoadField, DPMLoadRobot, DPMLoadField, MultiplayerLoadRobot, MultiplayerLoadField, CustomFieldLoader, DPMConfiguration}
    public static Sim currentSim = Sim.DefaultSimulator;
    Sim lastSim;

    //These are necessary references to specific UI parent objects.
    //We disable and enable these when the state of the main menu changes to reflect the user's selection.
    private GameObject selectionPanel;
    private GameObject defaultSimulator;
    private GameObject driverPracticeMode;
    private GameObject dpmConfiguration;
    private GameObject localMultiplayer;
    private GameObject simLoadField;
    private GameObject simLoadRobot;
    private GameObject dpmLoadField;
    private GameObject dpmLoadRobot;
    private GameObject multiplayerLoadField;
    private GameObject multiplayerLoadRobot;

    //We alter these to reflect the user's selected fields and robots.
    private GameObject simRobotSelectText;
    private GameObject simFieldSelectText;
    private GameObject dpmRobotSelectText;
    private GameObject dpmFieldSelectText;

    //This reflects the state of driver practice mode configuration and displays what field a robot is configured for.
    private GameObject configurationText;

    private GameObject graphics; //The Graphics GUI Objects
    private GameObject input; //The Input GUI Objects

    private GameObject splashScreen; //A panel that shows up at the start to cover the screen while initializing everything.

    private ArrayList fields; //ArrayList of field folders to select
    private ArrayList robots; //ArrayList of robot folders to select

    //Variables for Default Simulator mode
    private string simSelectedField; //the selected field file path
    private string simSelectedRobot; //the selected robot file path
    private string simSelectedFieldName; //the selected field name
    private string simSelectedRobotName; //the selected robot name

    //Variables for Driver Practice mode
    private string dpmSelectedField; //the selected field file path
    private string dpmSelectedRobot; //the selected robot file path
    private string dpmSelectedFieldName; //the selected field name
    private string dpmSelectedRobotName; //the selected robot name

    private FileBrowser fieldBrowser = null; //field directory browser
    private bool customfieldon = true; //whether the field directory browser is on
    public string fieldDirectory; //file path for field directory

    private FileBrowser robotBrowser = null; //robot directory browser
    private bool customroboton = true; //whether the robot directory browser is on
    public string robotDirectory; //file path for robot directory

    public static GameObject inputConflict; //UI object that shows when two inputs conflict with each other

    public static bool fullscreen; //true if application is in fullscreen
    public static int resolutionsetting; //resolution setting index
    private int[] xresolution = new int[10]; //arrays of resolution widths corresponding to index
    private int[] yresolution = new int[10]; //arrays of resolution heights corresponding to index

    private Canvas canvas; //canvas component of this object--used for scaling user message manager to size


    /// <summary>
    /// Runs every frame to update the GUI elements.
    /// </summary>
    void OnGUI ()
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

            simTab.SetActive(false);
            optionsTab.SetActive(false);
            homeTab.SetActive(true);
        }
        else UserMessageManager.Dispatch("You must select a directory or exit first!",3);
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
        driverPracticeMode.SetActive(false);
        localMultiplayer.SetActive(false);

        dpmLoadField.SetActive(false);
        dpmLoadRobot.SetActive(false);
        simLoadField.SetActive(false);
        simLoadRobot.SetActive(false);
        dpmConfiguration.SetActive(false);

        selectionPanel.SetActive(true);

    }

    /// <summary>
    /// Switches to the default simulator menu within the simulation tab and activates its respective UI elements.
    /// </summary>
    public void SwitchSimDefault()
    {
        currentSim = Sim.DefaultSimulator;

        selectionPanel.SetActive(false);
        simLoadField.SetActive(false);
        simLoadRobot.SetActive(false);
        defaultSimulator.SetActive(true);

        simRobotSelectText.GetComponent<Text>().text = simSelectedRobotName;
        simFieldSelectText.GetComponent<Text>().text = simSelectedFieldName;
    }

    /// <summary>
    /// Switches to the driver practice menu within the simulation tab and activates its respective UI elements.
    /// </summary>
    public void SwitchDriverPractice()
    {
        currentSim = Sim.DriverPracticeMode;

        selectionPanel.SetActive(false);
        dpmLoadField.SetActive(false);
        dpmLoadRobot.SetActive(false);
        driverPracticeMode.SetActive(true);
        dpmConfiguration.SetActive(false);

        dpmRobotSelectText.GetComponent<Text>().text = dpmSelectedRobotName;
        dpmFieldSelectText.GetComponent<Text>().text = dpmSelectedFieldName;
    }

    /// <summary>
    /// Switches to the multiplayer menu within the simulation tab and activates its respective UI elements.
    /// </summary>
    public void SwitchMultiplayer()
    {
        currentSim = Sim.Multiplayer;

        selectionPanel.SetActive(false);
        multiplayerLoadField.SetActive(false);
        multiplayerLoadRobot.SetActive(false);
        localMultiplayer.SetActive(true);
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
    /// Switches to the load robot menu for the driver practice mode and activates its respective UI elements.
    /// </summary>
    public void SwitchDPMLoadRobot()
    {
        currentSim = Sim.DPMLoadRobot;

        driverPracticeMode.SetActive(false);
        dpmLoadRobot.SetActive(true);
    }

    /// <summary>
    /// Switches to the load field menu for the driver practice mode and activates its respective UI elements.
    /// </summary>
    public void SwitchDPMLoadField()
    {
        currentSim = Sim.DPMLoadField;

        driverPracticeMode.SetActive(false);
        dpmLoadField.SetActive(true);
    }

    public void SwitchFieldDir()
    {
        currentTab = Tab.FieldDir;

        homeTab.SetActive(false);
        simTab.SetActive(false);
        optionsTab.SetActive(false);

        customfieldon = true;
        fieldBrowser.Active = true;
    }

    public void SwitchRobotDir()
    {
        currentTab = Tab.RobotDir;

        homeTab.SetActive(false);
        simTab.SetActive(false);
        optionsTab.SetActive(false);

        customroboton = true;
        robotBrowser.Active = true;
    }

    public void SwitchDPMConfiguration()
    {
        if (Directory.Exists(dpmSelectedField) && Directory.Exists(dpmSelectedField))
        {
            currentSim = Sim.DPMConfiguration;

            driverPracticeMode.SetActive(false);
            dpmConfiguration.SetActive(true);

            if (File.Exists(dpmSelectedRobot + "\\dpmConfiguration.txt"))
            {
                string line = "";
                int counter = 0;
                StreamReader reader = new StreamReader(dpmSelectedRobot + "\\dpmConfiguration.txt");

                string fieldName = "";
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Equals("#Field")) counter++;
                    else if (counter == 1)
                    {
                        fieldName = line;
                        break;
                    }
                }
                reader.Close();
                configurationText.GetComponent<Text>().text = "Robot Status: <color=#008000ff>Configured For " + fieldName + "</color>";
            }
            else configurationText.GetComponent<Text>().text = "Robot Status: <color=#a52a2aff>NOT CONFIGURED</color>";

  
        }
        else UserMessageManager.Dispatch("No Robot/Field Selected!", 5);
    }

    public void SwitchGraphics()
    {
        graphics.SetActive(true);
        input.SetActive(false);
    }

    public void SwitchInput()
    {
        graphics.SetActive(false);
        input.SetActive(true);
    }

    public void StartDefaultSim()
    {
        if (Directory.Exists(simSelectedField) && Directory.Exists(simSelectedRobot))
        {
            PlayerPrefs.SetString("simSelectedField", simSelectedField);
            PlayerPrefs.SetString("simSelectedFieldName", simSelectedFieldName);
            PlayerPrefs.SetString("simSelectedRobot", simSelectedRobot);
            PlayerPrefs.SetString("simSelectedRobotName", simSelectedRobotName);
            PlayerPrefs.Save();
            Application.LoadLevel("Scene");
        }
        else UserMessageManager.Dispatch("No Robot/Field Selected!", 2);
    }

    public void StartRobotConfiguration()
    {
        if (Directory.Exists(dpmSelectedField) && Directory.Exists(dpmSelectedField))
        {
            PlayerPrefs.SetString("dpmSelectedField", dpmSelectedField);
            PlayerPrefs.SetString("dpmSelectedFieldName", dpmSelectedFieldName);
            PlayerPrefs.SetString("dpmSelectedRobot", dpmSelectedRobot);
            PlayerPrefs.SetString("dpmSelectedRobotName", dpmSelectedRobotName);
            PlayerPrefs.Save();
            Application.LoadLevel("RobotConfiguration");
        }
        else UserMessageManager.Dispatch("No Robot/Field Selected!", 2);
    }

    public void StartDPM()
    {
        if (Directory.Exists(dpmSelectedField) && Directory.Exists(dpmSelectedField))
        {
            if (File.Exists(dpmSelectedRobot + "\\dpmConfiguration.txt"))
            {
                PlayerPrefs.SetString("dpmSelectedField", dpmSelectedField);
                PlayerPrefs.SetString("dpmSelectedFieldName", dpmSelectedFieldName);
                PlayerPrefs.SetString("dpmSelectedRobot", dpmSelectedRobot);
                PlayerPrefs.SetString("dpmSelectedRobotName", dpmSelectedRobotName);
                PlayerPrefs.Save();
                Application.LoadLevel("DriverPracticeMode");
            }
            else UserMessageManager.Dispatch("Robot is not configured yet!", 2);
        }
        else UserMessageManager.Dispatch("No Robot/Field Selected!", 2);
    }



    #region Main Tab Button Methods

    /* //Makes the Start Button display different things depending on whether a robot/field is loaded or not.
     public void StartButtonHover()
     {
         if (selectedFieldName.Equals("No Field Loaded!") || selectedRobotName.Equals("No Robot Loaded!")) 
         {
             startButton.GetComponent<Image>().color = Color.red;
             Text buttontext = readyText.GetComponent<Text>();
             buttontext.text = ( "Can't start without robot/field loaded!");
             buttontext.fontSize = 12;
         }
         else startButton.GetComponent<Image>().color = Color.green;
     }

     public void StartButtonExit()
     {
         Text buttontext = readyText.GetComponent<Text>();
         buttontext.text = ("START");
         buttontext.fontSize = 30;
     }

 /*    //Starts the simulation
     public void StartButtonClicked()
     {
         if (!selectedFieldName.Equals("No Field Loaded!") && !selectedRobotName.Equals("No Robot Loaded!"))
         {
             PlayerPrefs.SetString("Field", selectedField);
             PlayerPrefs.SetString("Robot", selectedRobot);
             Application.LoadLevel("Scene");
         }
     }*/

    //Exits the program
    public void Exit()
    {
        if (!Application.isEditor) {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }

    #endregion
    #region LoadRobot and LoadField Button Methods

    /*//Selects the robot, records the filename, and switches to the main Tab.
    public void SelectRobotButtonClicked()
    {
        if (robots.Count > 0)
        {
            selectedRobot = (robotDirectory + "\\" + robots[robotindex] + "\\");
            selectedRobotName = "Robot: " + currenttext;
            //SwitchTab(Tab.Main);
        }
        else UserMessageManager.Dispatch("No robot in directory!", 2);
    }

    //Selects the fields, records the filename, and switches to the main Tab.
    public void SelectFieldButtonClicked()
    {
        if (fields.Count > 0)
        {
            selectedField = (fieldDirectory + "\\" + fields[fieldindex] + "\\");
            selectedFieldName = "Field: " + currenttext;
            //SwitchTab(Tab.Main);
        }
        else UserMessageManager.Dispatch("No field in directory!",2);
    }*/

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

    public void LoadFieldDirectory()
    {
        if (!fieldBrowser.Active)
        {
            fieldBrowser.Active = true;
        }
        customfieldon = true;
        currentTab = Tab.FieldDir;
    }

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
    public void InputDefaultPressed()
    {
        Controls.ResetDefaults();
    }

    public void ApplyGraphics()
    {
        Screen.SetResolution(xresolution[resolutionsetting], yresolution[resolutionsetting], fullscreen);
        splashScreen.SetActive(true);
        StartCoroutine(HideSplashScreen(1));
        SwitchTabHome();
    }

    IEnumerator HideSplashScreen(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        splashScreen.SetActive(false);
    }

    public void OpenWebsite()
    {
        //System.Diagnostics.Process.Start("\\..\\FieldExporter\\Inventor_Exporter.exe");
        Application.OpenURL("http://bxd.autodesk.com/");
    }

    public void OpenTutorials()
    {
        Application.OpenURL("http://bxd.autodesk.com/?page=Tutorials");
    }

    public void OpenRobotExportTutorial()
    {
        Application.OpenURL("http://bxd.autodesk.com/?page=tutorialRobotExporter");
    }

    public void OpenFieldExportTutorial()
    {
        Application.OpenURL("http://bxd.autodesk.com/?page=tutorialFieldExporter");
    }
    public void OpenRobotConfigurationTutorial()
    {
        Application.OpenURL("http://bxd.autodesk.com/?page=tutorialRunningSimulator");
    }
    public void ResetControls()
    {
        Controls.ResetDefaults();
        Controls.SaveControls();
        GameObject.Find("InputPanel").GetComponent<InputScrollable>().UpdateControlList();
    }
    public void SelectSimField()
    {
        GameObject fieldList = GameObject.Find("SimLoadFieldList");
        string entry = (fieldList.GetComponent<SelectFieldScrollable>().selectedEntry);
        if (entry != null)
        {
            simSelectedFieldName = fieldList.GetComponent<SelectFieldScrollable>().selectedEntry;
            simSelectedField = fieldDirectory + "\\" + simSelectedFieldName + "\\";
            SwitchSimDefault();
        }
        else
        {
            UserMessageManager.Dispatch("No Field Selected!", 2);
        }
    }

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

    public void SelectDPMField()
    {
        GameObject fieldList = GameObject.Find("DPMLoadFieldList");
        string entry = (fieldList.GetComponent<ScrollablePanel>().selectedEntry);
        if (entry != null)
        {
            dpmSelectedFieldName = fieldList.GetComponent<ScrollablePanel>().selectedEntry;
            dpmSelectedField = fieldDirectory + "\\" + dpmSelectedFieldName + "\\";
            SwitchDriverPractice();
        }
        else
        {
            UserMessageManager.Dispatch("No Field Selected!", 2);
        }
    }

    public void SelectDPMRobot()
    {
        GameObject robotList = GameObject.Find("DPMLoadRobotList");
        string entry = (robotList.GetComponent<ScrollablePanel>().selectedEntry);
        if (entry != null)
        {
            dpmSelectedRobotName = robotList.GetComponent<ScrollablePanel>().selectedEntry;
            dpmSelectedRobot = robotDirectory + "\\" + dpmSelectedRobotName + "\\";
            SwitchDriverPractice();
        }
        else
        {
            UserMessageManager.Dispatch("No Robot Selected!", 2);
        }
    }
    #endregion
    void Start () {
        
        FindAllGameObjects();
        splashScreen.SetActive(true);
        InitGraphicsSettings();
        fields = new ArrayList();
        robots = new ArrayList();

        robotDirectory = PlayerPrefs.GetString("RobotDirectory", (System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "//synthesis//Robots"));
        robotDirectory = (Directory.Exists(robotDirectory)) ? robotDirectory : robotDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments); //If the robot directory no longer exists, set it to the default application path.
        fieldDirectory = PlayerPrefs.GetString("FieldDirectory", (System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "//synthesis//Fields"));
        fieldDirectory = (Directory.Exists(fieldDirectory)) ? fieldDirectory : robotDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments); //if the field directory no longer exists, set it to the default application path.

        simSelectedField = PlayerPrefs.GetString("simSelectedField");
        simSelectedFieldName = (Directory.Exists(simSelectedField)) ? PlayerPrefs.GetString("simSelectedFieldName", "No Field Selected!") : "No Field Selected!";
        simSelectedRobot = PlayerPrefs.GetString("simSelectedRobot");
        simSelectedRobotName = (Directory.Exists(simSelectedRobot)) ? PlayerPrefs.GetString("simSelectedRobotName", "No Robot Selected!") : "No Robot Selected!";
        dpmSelectedField = PlayerPrefs.GetString("dpmSelectedField");
        dpmSelectedFieldName = (Directory.Exists(dpmSelectedField)) ? PlayerPrefs.GetString("dpmSelectedFieldName", "No Field Selected!") : "No Field Selected!";
        dpmSelectedRobot = PlayerPrefs.GetString("dpmSelectedRobot");
        dpmSelectedRobotName = (Directory.Exists(dpmSelectedRobot)) ? PlayerPrefs.GetString("dpmSelectedRobotName", "No Robot Selected!") : "No Robot Selected!";

        canvas = GetComponent<Canvas>();



        customfieldon = false;
        customroboton = false;
        ApplyGraphics();
        if (currentSim != Sim.Selection)
        {
            if (currentSim == Sim.DPMConfiguration)
            {
                SwitchTabSim();
                SwitchSimSelection();
                SwitchDriverPractice();
                SwitchDPMConfiguration();
            }
            else if (currentSim == Sim.DefaultSimulator)
            {
                SwitchTabSim();
                SwitchSimSelection();
                SwitchSimDefault();
            }
        }
        else
        {
            SwitchSimDefault();
            SwitchTabHome();
        }
        
        
    }
	 void FindAllGameObjects()
    {
        //We need to make refernces to various buttons/text game objects, but using GameObject.Find is inefficient if we do it every update.
        //Therefore, we assign variables to them and only use GameObject.Find once for each object in startup.
        selectionPanel = AuxFunctions.FindObject(gameObject, "SelectionPanel"); //The Mode Selection Tab GUI Objects
        defaultSimulator = AuxFunctions.FindObject(gameObject, "DefaultSimulator");
        driverPracticeMode = AuxFunctions.FindObject(gameObject, "DriverPracticeMode");
        dpmConfiguration = AuxFunctions.FindObject(gameObject, "DPMConfiguration");
        localMultiplayer = AuxFunctions.FindObject(gameObject, "LocalMultiplayer");
        simLoadField = AuxFunctions.FindObject(gameObject, "SimLoadField");
        simLoadRobot = AuxFunctions.FindObject(gameObject, "SimLoadRobot");
        dpmLoadField = AuxFunctions.FindObject(gameObject, "DPMLoadField");
        dpmLoadRobot = AuxFunctions.FindObject(gameObject, "DPMLoadRobot");
        multiplayerLoadField = AuxFunctions.FindObject(gameObject, "MultiplayerLoadField");
        multiplayerLoadRobot = AuxFunctions.FindObject(gameObject, "MultiplayerLoadRobot");
        splashScreen = AuxFunctions.FindObject(gameObject, "LoadSplash");

        graphics = AuxFunctions.FindObject(gameObject, "Graphics");
        input = AuxFunctions.FindObject(gameObject, "Input");

        simFieldSelectText = AuxFunctions.FindObject(defaultSimulator, "SimFieldSelectText");
        simRobotSelectText = AuxFunctions.FindObject(defaultSimulator, "SimRobotSelectText");
        dpmFieldSelectText = AuxFunctions.FindObject(driverPracticeMode, "DPMFieldSelectText");
        dpmRobotSelectText = AuxFunctions.FindObject(driverPracticeMode, "DPMRobotSelectText");

        configurationText = AuxFunctions.FindObject(dpmConfiguration, "ConfigurationText");

        inputConflict = AuxFunctions.FindObject(gameObject, "InputConflict");

        AuxFunctions.FindObject(gameObject, "QualitySettingsText").GetComponent<Text>().text = QualitySettings.names[QualitySettings.GetQualityLevel()];
    }

    void InitGraphicsSettings()
    {
        xresolution[0] = 640;
        xresolution[1] = 800;
        xresolution[2] = 1024;
        xresolution[3] = 1280;
        xresolution[4] = 1280;
        xresolution[5] = 1280;
        xresolution[6] = 1400;
        xresolution[7] = 1600;
        xresolution[8] = 1680;
        xresolution[9] = 1920;

        yresolution[0] = 480;
        yresolution[1] = 600;
        yresolution[2] = 768;
        yresolution[3] = 720;
        yresolution[4] = 768;
        yresolution[5] = 1024;
        yresolution[6] = 900;
        yresolution[7] = 900;
        yresolution[8] = 1050;
        yresolution[9] = 1080;

        fullscreen = Screen.fullScreen;
        int width = Screen.currentResolution.width;
        int height = Screen.currentResolution.height;
        if (width == xresolution[0] && height == yresolution[0]) resolutionsetting = 0;
        else if (width == xresolution[1] && height == yresolution[1]) resolutionsetting = 1;
        else if (width == xresolution[2] && height == yresolution[2]) resolutionsetting = 2;
        else if (width == xresolution[3] && height == yresolution[3]) resolutionsetting = 3;
        else if (width == xresolution[4] && height == yresolution[4]) resolutionsetting = 4;
        else if (width == xresolution[5] && height == yresolution[5]) resolutionsetting = 5;
        else if (width == xresolution[6] && height == yresolution[6]) resolutionsetting = 6;
        else if (width == xresolution[7] && height == yresolution[7]) resolutionsetting = 7;
        else if (width == xresolution[8] && height == yresolution[8]) resolutionsetting = 8;
        else if (width == xresolution[9] && height == yresolution[9]) resolutionsetting = 9;
        else resolutionsetting = 2;
    }

    public void ChangeQualitySettings()
    {
        if (QualitySettings.GetQualityLevel() < QualitySettings.names.Length - 1) QualitySettings.SetQualityLevel(QualitySettings.GetQualityLevel() + 1);
        else QualitySettings.SetQualityLevel(0);
        GameObject.Find("QualitySettingsText").GetComponent<Text>().text = QualitySettings.names[QualitySettings.GetQualityLevel()];

    } 
}
