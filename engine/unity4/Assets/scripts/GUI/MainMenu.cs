using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class MainMenu : MonoBehaviour {

    enum Menu { Main, LoadField, LoadRobot, Graphics, Input, Custom};
    Menu currentMenu = Menu.Main;

    public GameObject Main; //The Main GUI Objects
    public GameObject LoadField; //The LoadField GUI Objects
    public GameObject LoadRobot; //The LoadRobot GUI Objects
    public GameObject Graphics; //The Graphics GUI Objects
    public GameObject Input; //The Input GUI Objects
    public GameObject SplashScreen; //A panel that shows up at the start while initializing everything.

    private GameObject fieldSelectText;
    private GameObject robotSelectText;
    private GameObject fieldSelectImage;

    private GameObject fieldText;
    private GameObject fieldImage;
    private GameObject robotText;
    private GameObject robotImage;

    private GameObject startButton;
    private GameObject readyText;

    private GameObject[] robotNavigation;
    private GameObject[] fieldNavigation;


    private ArrayList fields; //ArrayList of field folders to select
    private ArrayList robots; //ArrayList of robot folders to select

    private int fieldindex; //The current index of field selection
    private int robotindex; //The current index of robot selection

    private Sprite currentimage; //The current thumbnail to be previewed
    private string currenttext; //The current text to be displayed

    private string selectedField; //the selected field file path
    private string selectedRobot; //the selected robot file path
    private string selectedFieldName; //the selected field name
    private string selectedRobotName; //the selected robot name
    private Sprite selectedFieldImage; //the selected field image

    private FileBrowser fieldBrowser = null;
    private bool customfieldon = true;
    private string fieldDirectory;

    private FileBrowser robotBrowser = null;
    private bool customroboton = true;
    private string robotDirectory;

    private bool showList = false;
    private int listEntry = 0;
    private GUIContent[] list;
    private GUIStyle listStyle;
    private bool picked;

    public static GameObject inputConflict;

    public static bool fullscreen = false;
    public static int resolutionsetting = 0;
    private int[] xresolution = new int[10];
    private int[] yresolution = new int[10];


    /// <summary>
    /// Runs every frame to update the GUI elements.
    /// </summary>
    void OnGUI ()
    {
        switch (currentMenu)
         {
             case Menu.LoadField:
                //Updates the preview thumbnail and text
                if (fields.Count > 0)
                {
                    fieldText.GetComponent<Text>().text = currenttext;
                    fieldImage.GetComponent<Image>().sprite = currentimage;
                }
                break;

             case Menu.LoadRobot:
                //Updates the preview thumbnail and text
                if (robots.Count > 0)
                {
                    robotText.GetComponent<Text>().text = currenttext;
                    robotImage.GetComponent<Image>().sprite = currentimage;
                }
                break;

             case Menu.Custom:
                //Switches back to the Load Field or Load Robot menu if the custom directory browser has been turned off
                if (customfieldon && !fieldBrowser.Active)
                {
                    customfieldon = false;
                    SwitchState(Menu.LoadField);
                }
                else if (customroboton && !robotBrowser.Active)
                {
                    customroboton = false;
                    SwitchState(Menu.LoadRobot);
                }
                break;
         }
         InitFieldBrowser();
         InitRobotBrowser();
         UserMessageManager.Render();
    }

    /// <summary>
    /// Switches between the different states of the menu to render different parts of the Main Menu GUI.
    /// </summary>
    /// <param name="menu">The menu to switch to</param>
    void SwitchState(Menu menu)
    {
        switch (menu)
        {
            case Menu.Main:
                currentMenu = Menu.Main;

                Main.SetActive(true);
                LoadField.SetActive(false);
                LoadRobot.SetActive(false);
                Graphics.SetActive(false);
                Input.SetActive(false);

                //Updates the selected robot/field thumbnail and text
                fieldSelectText.GetComponent<Text>().text = selectedFieldName;
                robotSelectText.GetComponent<Text>().text = selectedRobotName;
                if (selectedFieldImage != null) fieldSelectImage.GetComponent<Image>().sprite = selectedFieldImage;

                break;

            case Menu.LoadField:
                currentMenu = Menu.LoadField;

                Main.SetActive(false);
                LoadField.SetActive(true);

                UpdateFieldDirectory();
                break;

            case Menu.LoadRobot:
                currentMenu = Menu.LoadRobot;

                Main.SetActive(false);
                LoadRobot.SetActive(true);

                UpdateRobotDirectory();
                break;

            case Menu.Graphics:
                currentMenu = Menu.Graphics;

                Main.SetActive(false);
                Graphics.SetActive(true);
                Input.SetActive(false);
                break;

            case Menu.Input:
                currentMenu = Menu.Input;

                Main.SetActive(false);
                Graphics.SetActive(false);
                Input.SetActive(true);
                break;

            case Menu.Custom:
                currentMenu = Menu.Custom;

                LoadField.SetActive(false);
                LoadRobot.SetActive(false);
                break;

        }
    }

    #region Main Menu Button Methods
        //Loads the fields arraylist with the folder names and switches the current menu to the loadfield menu.
    public void LoadFieldButtonClicked()
    {
        SwitchState(Menu.LoadField);
    }

    //Loads the robot arraylist with the folder names and switches the current menu to the loadrobot menu.
    public void LoadRobotButtonClicked()
    {
        SwitchState(Menu.LoadRobot);
    }

    //Switches the current menu to the settings menu.
    public void GraphicsButtonClicked()
    {
        SwitchState(Menu.Graphics);
    }

    public void InputButtonClicked()
    {
        SwitchState(Menu.Input);
    }

    //Makes the Start Button display different things depending on whether a robot/field is loaded or not.
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

    //Starts the simulation
    public void StartButtonClicked()
    {
        if (!selectedFieldName.Equals("No Field Loaded!") && !selectedRobotName.Equals("No Robot Loaded!"))
        {
            PlayerPrefs.SetString("Field", selectedField);
            PlayerPrefs.SetString("Robot", selectedRobot);
            Application.LoadLevel("Scene");
        }
    }

    //Exits the program
    public void Exit()
    {
        Application.Quit();
    }

    #endregion
    #region LoadRobot and LoadField Button Methods
    //Switches the current menu to the settings menu.
    public void BackButtonClicked()
    {
        SwitchState(Menu.Main);
    }

    //Shifts the previewing index to the right and updates preview info.
    public void RightArrowClicked()
    {
        if (currentMenu == Menu.LoadField)
            if (fieldindex >= fields.Count-1) fieldindex = 0;
            else fieldindex++;
        else if (currentMenu == Menu.LoadRobot)
            if (robotindex >= robots.Count - 1) robotindex = 0;
            else robotindex++;
        UpdatePreview();
    }

    //Shifts the previewing index to the left and updates preview info.
    public void LeftArrowClicked()
    {
        if (currentMenu == Menu.LoadField)
            if (fieldindex == 0) fieldindex = fields.Count-1;
            else fieldindex--;
        else if (currentMenu == Menu.LoadRobot)
            if (robotindex == 0) robotindex = robots.Count-1;
            else robotindex--;
        UpdatePreview();
    }

    //Selects the robot, records the filename, and switches to the main menu.
    public void SelectRobotButtonClicked()
    {
        if (robots.Count > 0)
        {
            selectedRobot = (robotDirectory + "\\" + robots[robotindex] + "\\");
            selectedRobotName = "Robot: " + currenttext;
            SwitchState(Menu.Main);
        }
        else UserMessageManager.Dispatch("No robot in directory!", 2);
    }

    //Selects the fields, records the filename, and switches to the main menu.
    public void SelectFieldButtonClicked()
    {
        if (fields.Count > 0)
        {
            selectedField = (fieldDirectory + "\\" + fields[fieldindex] + "\\");
            selectedFieldImage = currentimage;
            selectedFieldName = "Field: " + currenttext;
            SwitchState(Menu.Main);
        }
        else UserMessageManager.Dispatch("No field in directory!",2);
    }

    public void InitFieldBrowser()
    {
        if (fieldBrowser == null)
        {
            fieldBrowser = new FileBrowser("Choose Field Directory", true);
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
                    SwitchState(Menu.LoadField);
                    customfieldon = false;
                    PlayerPrefs.SetString("FieldDirectory", fieldDirectory);
                    PlayerPrefs.Save();
                    UpdateFieldDirectory();
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
        if (!fieldBrowser.Active) fieldBrowser.Active = true;
        customfieldon = true;
        SwitchState(Menu.Custom);
    }

    public void InitRobotBrowser()
    {
        if (robotBrowser == null)
        {
            robotBrowser = new FileBrowser("Choose Robot Directory", true);
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
                    SwitchState(Menu.LoadRobot);
                    customroboton = false;
                    PlayerPrefs.SetString("RobotDirectory", robotDirectory);
                    PlayerPrefs.Save();
                    UpdateRobotDirectory();
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
        if (!robotBrowser.Active) robotBrowser.Active = true;
        customroboton = true;
        SwitchState(Menu.Custom);
    }

    //Updates preview information.
    void UpdatePreview()
    {
        if (currentMenu == Menu.LoadField)
        {
            currenttext = (string)fields[fieldindex];
            if (File.Exists(fieldDirectory + "\\" + fields[fieldindex] + "\\thumbnail.png")) currentimage = Sprite.Create(Extensions.LoadPNG(fieldDirectory + "\\" + fields[fieldindex] + "\\thumbnail.png"), new Rect(0.0f, 0.0f, 1280.0f, 720.0f), new Vector2(0.5f, 0.5f), 1000);
            else currentimage = Sprite.Create(Extensions.LoadPNG(Application.dataPath + "\\Resources\\Images\\defaultfield.png"), new Rect(0.0f, 0.0f, 1280.0f, 720.0f), new Vector2(0.5f, 0.5f), 1000);
        }
        else
        {
            currenttext = (string)robots[robotindex];
            if (File.Exists(robotDirectory + "\\" + robots[robotindex] + "\\thumbnail.png")) currentimage = Sprite.Create(Extensions.LoadPNG(robotDirectory + "\\" + robots[robotindex] + "\\thumbnail.png"), new Rect(0.0f, 0.0f, 1280.0f, 720.0f), new Vector2(0.5f, 0.5f), 1000);
            else currentimage = Sprite.Create(Extensions.LoadPNG(Application.dataPath + "\\Resources\\Images\\defaultrobot.png"), new Rect(0.0f, 0.0f, 1280.0f, 720.0f), new Vector2(0.5f, 0.5f), 1000);
        }
    }

    void UpdateFieldDirectory()
    {
        fields.Clear();
        string[] folders = System.IO.Directory.GetDirectories(fieldDirectory);
        foreach (string field in folders)
        {
            if (File.Exists(field + "\\definition.bxdf")) fields.Add(new DirectoryInfo(field).Name);
        }
        
        //If there is nothing found in the field folder, provide error message
        if (fields.Count <= 0)
        {
            foreach (GameObject gameobject in fieldNavigation) gameobject.SetActive(false);
        }
        else
        {
            foreach (GameObject gameobject in fieldNavigation) gameobject.SetActive(true);
        }

        if (fields.Count > 0)
        {
            if (fieldindex >= fields.Count) fieldindex = 0;
            UpdatePreview();
        }
    }

    void UpdateRobotDirectory()
    {
        robots.Clear();
        string[] folders = System.IO.Directory.GetDirectories(robotDirectory);
        foreach (string robot in folders)
        {
            if (File.Exists(robot + "\\skeleton.bxdj")) robots.Add(new DirectoryInfo(robot).Name);
        }

        //If there is nothing found in the robot folder, provide error message
        if (robots.Count <= 0)
        {
            foreach (GameObject gameobject in robotNavigation) gameobject.SetActive(false);
        }
        else
        {
            foreach (GameObject gameobject in robotNavigation) gameobject.SetActive(true);
        }

        if (robots.Count > 0)
        {
            if (robotindex >= robots.Count) robotindex = robots.Count - 1;
            UpdatePreview();
        }
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
        SplashScreen.SetActive(true);
        StartCoroutine(HideSplashScreen(1));
        SwitchState(Menu.Main);
    }

    IEnumerator HideSplashScreen(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        SplashScreen.SetActive(false);
    }
    #endregion
    void Start () {
        //We need to make refernces to various buttons/text game objects, but using GameObject.Find is inefficient if we do it every update.
        //Therefore, we assign variables to them and only use GameObject.Find once for each object in startup.
        Main.SetActive(true);
        LoadField.SetActive(true);
        LoadRobot.SetActive(true);
        Input.SetActive(true);
        

        fieldSelectText = GameObject.Find("FieldSelectText");
        robotSelectText = GameObject.Find("RobotSelectText");
        fieldSelectImage = GameObject.Find("FieldSelectImage");
        
        fieldText = GameObject.Find("FieldText");
        fieldImage = GameObject.Find("FieldImage");
        robotText = GameObject.Find("RobotText");
        robotImage = GameObject.Find("RobotImage");

        startButton = GameObject.Find("StartButton");
        readyText = GameObject.Find("ReadyText");

        robotNavigation = GameObject.FindGameObjectsWithTag("RobotNavigation");
        fieldNavigation = GameObject.FindGameObjectsWithTag("FieldNavigation");

        inputConflict = GameObject.Find("InputConflict");

        fields = new ArrayList();
        robots = new ArrayList();

        selectedRobot = PlayerPrefs.GetString("Robot", "No Robot Loaded!");
        selectedField = PlayerPrefs.GetString("Field", "No Field Loaded!");

        selectedRobotName = "Robot: " + new DirectoryInfo(selectedRobot).Name;
        selectedFieldName = "Field: " + new DirectoryInfo(selectedField).Name;

        robotDirectory = PlayerPrefs.GetString("RobotDirectory", Directory.GetParent(Application.dataPath).FullName);
        fieldDirectory = PlayerPrefs.GetString("FieldDirectory", Directory.GetParent(Application.dataPath).FullName);
        customfieldon = false;
        customroboton = false;

        SwitchState(Menu.Main);

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

    }
	
	void Update () {
	    
	}
}
