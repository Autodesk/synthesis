using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;


public class MainMenu : MonoBehaviour
{

    public enum Tab { Main, Sim, Options, FieldDir, RobotDir };
    public static Tab currentTab = Tab.Main;

    public GameObject homeTab;
    public GameObject simTab;
    public GameObject optionsTab;

    public enum Sim { Selection, DefaultSimulator, DriverPracticeMode, Multiplayer, SimLoadRobot, SimLoadField, DPMLoadRobot, DPMLoadField, MultiplayerLoadRobot, MultiplayerLoadField, CustomFieldLoader, DPMConfiguration }
    public static Sim currentSim = Sim.DefaultSimulator;
    Sim lastSim;

    private GameObject selectionPanel; //The Mode Selection Tab GUI Objects
    private GameObject defaultSimulator;
    private GameObject driverPracticeMode;
    private GameObject dpmConfiguration;
    private GameObject localMultiplayer;
    private GameObject simLoadField; //The LoadField GUI Objects
    private GameObject simLoadRobot; //The LoadRobot GUI Objects
    private GameObject dpmLoadField;
    private GameObject dpmLoadRobot;
    private GameObject multiplayerLoadField;
    private GameObject multiplayerLoadRobot;

    private GameObject simRobotSelectText;
    private GameObject simFieldSelectText;
    private GameObject dpmRobotSelectText;
    private GameObject dpmFieldSelectText;

    private GameObject configurationText;

    private GameObject graphics; //The Graphics GUI Objects
    private GameObject input; //The Input GUI Objects
    private GameObject splashScreen; //A panel that shows up at the start while initializing everything.

    private ArrayList fields; //ArrayList of field folders to select
    private ArrayList robots; //ArrayList of robot folders to select

    private int fieldindex; //The current index of field selection
    private int robotindex; //The current index of robot selection

    private Sprite currentimage; //The current thumbnail to be previewed
    private string currenttext; //The current text to be displayed

    private string simSelectedField; //the selected field file path
    private string simSelectedRobot; //the selected robot file path
    private string simSelectedFieldName; //the selected field name
    private string simSelectedRobotName; //the selected robot name

    private string dpmSelectedField;
    private string dpmSelectedRobot;
    private string dpmSelectedFieldName;
    private string dpmSelectedRobotName;

    private float _buttonDownPhaseStart;
    private float _doubleClickPhaseStart;


    private FileBrowser fieldBrowser = null;
    private bool customfieldon = true;
    public string fieldDirectory;

    private FileBrowser robotBrowser = null;
    private bool customroboton = true;
    public string robotDirectory;

    public static GameObject inputConflict;

    public static bool fullscreen;
    public static int resolutionsetting;
    private int[] xresolution = new int[10];
    private int[] yresolution = new int[10];

    private Canvas canvas;


    /// <summary>
    /// Runs every frame to update the GUI elements.
    /// </summary>
    void OnGUI()
    {
        switch (currentTab)
        {
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
        InitFieldBrowser();
        InitRobotBrowser();

        UserMessageManager.Render();
        UserMessageManager.scale = canvas.scaleFactor;
    }

    public void SwitchTabHome()
    {
        if (currentTab != Tab.RobotDir && currentTab != Tab.FieldDir)
        {
            currentTab = Tab.Main;

            simTab.SetActive(false);
            optionsTab.SetActive(false);
            homeTab.SetActive(true);
        }
        else UserMessageManager.Dispatch("You must select a directory or exit first!", 3);
    }

    public void SwitchTabSim()
    {
        if (currentTab != Tab.RobotDir && currentTab != Tab.FieldDir)
        {
            currentTab = Tab.Sim;

            homeTab.SetActive(false);
            optionsTab.SetActive(false);
            simTab.SetActive(true);
        }
        else UserMessageManager.Dispatch("You must select a directory or exit first!", 3);
    }

    public void SwitchTabOptions()
    {
        if (currentTab != Tab.RobotDir && currentTab != Tab.FieldDir)
        {
            currentTab = Tab.Options;

            homeTab.SetActive(false);
            simTab.SetActive(false);
            optionsTab.SetActive(true);
        }
        else UserMessageManager.Dispatch("You must select a directory or exit first!", 3);
    }

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

    public void SwitchMultiplayer()
    {
        currentSim = Sim.Multiplayer;

        selectionPanel.SetActive(false);
        multiplayerLoadField.SetActive(false);
        multiplayerLoadRobot.SetActive(false);
        localMultiplayer.SetActive(true);
    }

    public void SwitchSimLoadRobot()
    {
        currentSim = Sim.SimLoadRobot;

        defaultSimulator.SetActive(false);
        simLoadRobot.SetActive(true);
    }

    public void SwitchSimLoadField()
    {
        currentSim = Sim.SimLoadField;

        defaultSimulator.SetActive(false);
        simLoadField.SetActive(true);
    }

    public void SwitchDPMLoadRobot()
    {
        currentSim = Sim.DPMLoadRobot;

        driverPracticeMode.SetActive(false);
        dpmLoadRobot.SetActive(true);
    }

    public void SwitchDPMLoadField()
    {
        currentSim = Sim.DPMLoadField;

        driverPracticeMode.SetActive(false);
        dpmLoadField.SetActive(true);
    }

    public void SwitchMultiplayerLoadRobot()
    {
        currentSim = Sim.MultiplayerLoadRobot;

        localMultiplayer.SetActive(false);
        multiplayerLoadRobot.SetActive(true);
    }

    public void SwitchMultiplayerLoadField()
    {
        currentSim = Sim.SimLoadField;

        localMultiplayer.SetActive(false);
        multiplayerLoadField.SetActive(true);
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
        if (!Application.isEditor)
        {
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


    //if single click
    //      highlight directory
    //if double click
    //      open folder

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _buttonDownPhaseStart = Time.time;
        }

        if (_doubleClickPhaseStart > -1 && (Time.time - _doubleClickPhaseStart) > 0.2f)
        {
            Debug.Log("single click");
            _doubleClickPhaseStart = -1;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (Time.time - _buttonDownPhaseStart > 1.0f)
            {
                Debug.Log("long click");
                _doubleClickPhaseStart = -1;
            }
            else
            {
                if (Time.time - _doubleClickPhaseStart < 0.2f)
                {
                    Debug.Log("double click");
                    _doubleClickPhaseStart = -1;
                }
                else
                {
                    _doubleClickPhaseStart = Time.time;
                }
            }
        }
    }

    public void InitFieldBrowser()
    {
        if (fieldBrowser == null)
        {
            fieldBrowser = new FileBrowser("Choose Field Folder", fieldDirectory, true);
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
            robotBrowser = new FileBrowser("Choose Robot Folder", robotDirectory, true);
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
        string entry = (fieldList.GetComponent<ScrollableList>().selectedEntry);
        if (entry != null)
        {
            simSelectedFieldName = fieldList.GetComponent<ScrollableList>().selectedEntry;
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
        string entry = (robotList.GetComponent<ScrollableList>().selectedEntry);
        if (entry != null)
        {
            simSelectedRobotName = robotList.GetComponent<ScrollableList>().selectedEntry;
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
        string entry = (fieldList.GetComponent<ScrollableList>().selectedEntry);
        if (entry != null)
        {
            dpmSelectedFieldName = fieldList.GetComponent<ScrollableList>().selectedEntry;
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
        string entry = (robotList.GetComponent<ScrollableList>().selectedEntry);
        if (entry != null)
        {
            dpmSelectedRobotName = robotList.GetComponent<ScrollableList>().selectedEntry;
            dpmSelectedRobot = robotDirectory + "\\" + dpmSelectedRobotName + "\\";
            SwitchDriverPractice();
        }
        else
        {
            UserMessageManager.Dispatch("No Robot Selected!", 2);
        }
    }
    #endregion
    void Start()
    {

        FindAllGameObjects();
        splashScreen.SetActive(true);
        InitGraphicsSettings();
        fields = new ArrayList();
        robots = new ArrayList();

        robotDirectory = PlayerPrefs.GetString("RobotDirectory", (Application.dataPath) + "//Robots");
        robotDirectory = (Directory.Exists(robotDirectory)) ? robotDirectory : robotDirectory = Directory.GetParent(Application.dataPath).FullName; //If the robot directory no longer exists, set it to the default application path.
        fieldDirectory = PlayerPrefs.GetString("FieldDirectory", (Application.dataPath) + "//Fields");
        fieldDirectory = (Directory.Exists(fieldDirectory)) ? fieldDirectory : robotDirectory = Directory.GetParent(Application.dataPath).FullName; //if the field directory no longer exists, set it to the default application path.

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
            SwitchTabHome();
            SwitchSimDefault();
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
}