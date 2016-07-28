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
    public GameObject DefaultPanel; //A blank transparent panel
    public custom_inputs InputManager; //The input manager

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



    //The GUI rendering method. It uses a state machine to switch between the different menus.
    void OnGUI ()
    {
        switch (currentMenu)
         {
             case Menu.Main:
                 RenderMain();
                 break;

             case Menu.LoadField:
                 RenderLoadField();
                 break;

             case Menu.LoadRobot:
                 RenderLoadRobot();
                 break;

             case Menu.Graphics:
                 RenderGraphics();
                 break;

             case Menu.Input:
                 RenderInput();
                 break;

             case Menu.Custom:
                 RenderCustom();
                 break;
         }
         InitFieldBrowser();
         InitRobotBrowser();
         UserMessageManager.Render();
    }
    #region Rendering
    //Method to render the Main GUI objects
    void RenderMain()
    {
        Main.SetActive(true);
        DefaultPanel.SetActive(false);
        LoadField.SetActive(false);
        LoadRobot.SetActive(false);
        Graphics.SetActive(false);
        Input.SetActive(false);

        //Updates the selected robot/field thumbnail and text
        fieldSelectText.GetComponent<Text>().text = selectedFieldName;
        robotSelectText.GetComponent<Text>().text = selectedRobotName;
        if (selectedFieldImage != null) fieldSelectImage.GetComponent<Image>().sprite = selectedFieldImage;
    }

    //Method to render to LoadField GUI objects
    void RenderLoadField()
    {
        Main.SetActive(false);
        LoadField.SetActive(true);
        DefaultPanel.SetActive(false);

        if (fields.Count > 0)
        {
            //Updates the preview thumbnail and text
            fieldText.GetComponent<Text>().text = currenttext;
            fieldImage.GetComponent<Image>().sprite = currentimage;
        }

    }

    //Method to render the LoadRobot GUI objects
    void RenderLoadRobot()
    {
        Main.SetActive(false);
        LoadRobot.SetActive(true);
        DefaultPanel.SetActive(false);

        if (robots.Count > 0)
        {
            //Updates the preview thumbnail and text
            robotText.GetComponent<Text>().text = currenttext;
            robotImage.GetComponent<Image>().sprite = currentimage;
        }
    }

    //Method to render the Graphics Settings GUI objects
    void RenderGraphics()
    {
        Main.SetActive(false);
        Graphics.SetActive(true);
        Input.SetActive(false);
    }

    //Method to render the Input Settings GUI objects
    void RenderInput()
    {
        Main.SetActive(false);
        Graphics.SetActive(false);
        Input.SetActive(true);
    }

    void RenderCustom()
    {
        DefaultPanel.SetActive(true);
        LoadField.SetActive(false);
        LoadRobot.SetActive(false);
        if (customfieldon && !fieldBrowser.Active)
        {
            customfieldon = false;
            currentMenu = Menu.LoadField;
        }
        else if (customroboton && !robotBrowser.Active)
        {
            customroboton = false;
            currentMenu = Menu.LoadRobot;
        }
    }

    #endregion
    #region Main Menu Button Methods
    //Loads the fields arraylist with the folder names and switches the current menu to the loadfield menu.
    public void LoadFieldButtonClicked()
    {
        currentMenu = Menu.LoadField;
        UpdateFieldDirectory();
    }

    //Loads the robot arraylist with the folder names and switches the current menu to the loadrobot menu.
    public void LoadRobotButtonClicked()
    {
        currentMenu = Menu.LoadRobot;
        UpdateRobotDirectory();
    }

    //Switches the current menu to the settings menu.
    public void GraphicsButtonClicked()
    {
        currentMenu = Menu.Graphics;
    }

    public void InputButtonClicked()
    {
        currentMenu = Menu.Input;
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
        currentMenu = Menu.Main;
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
            currentMenu = Menu.Main;
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
            currentMenu = Menu.Main;
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
                    currentMenu = Menu.LoadField;
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
        currentMenu = Menu.Custom;
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
                    currentMenu = Menu.LoadRobot;
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
        currentMenu = Menu.Custom;
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

        LoadField.SetActive(true);
        
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
        currentMenu = Menu.LoadRobot;
        string[] folders = System.IO.Directory.GetDirectories(robotDirectory);
        foreach (string robot in folders)
        {
            if (File.Exists(robot + "\\skeleton.bxdj")) robots.Add(new DirectoryInfo(robot).Name);
        }

        LoadRobot.SetActive(true);
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
    }
    #endregion
    void Start () {

        //We need to make refernces to various buttons/text game objects, but using GameObject.Find is inefficient if we do it every update.
        //Therefore, we assign variables to them and only use GameObject.Find once for each object in startup.
        Main.SetActive(true);
        LoadField.SetActive(true);
        LoadRobot.SetActive(true);

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

        robotDirectory = PlayerPrefs.GetString("RobotDirectory", Directory.GetParent(Application.dataPath).FullName + "//Robots");
        fieldDirectory = PlayerPrefs.GetString("FieldDirectory", Directory.GetParent(Application.dataPath).FullName + "//Fields");
        customfieldon = false;
        customroboton = false;


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
