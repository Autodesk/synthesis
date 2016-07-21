using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

/// <summary>
/// The class that controls all of the Main Menu GUI. It is attached to the MainMenuCanvas gameobject in the MainMenu scene.
/// </summary>
public class MainMenu : MonoBehaviour {

    enum Menu { Main, LoadField, LoadRobot, Graphics, Input, Custom};
    Menu currentMenu = Menu.Main;

    public GameObject Main; //The Main GUI Objects
    public GameObject LoadField; //The LoadField GUI Objects
    public GameObject LoadRobot; //The LoadRobot GUI Objects
    public GameObject Graphics; //The Graphics GUI Objects
    public GameObject Input; //The Input GUI Objects
    public GameObject DefaultPanel; //A blank transparent panel

    private ArrayList fields; //ArrayList of field folders to select
    private ArrayList robots; //ArrayList of robot folders to select

    private string filepath; //The shortcut of the filepath where the exe should be located
    private int fieldindex; //The current index of field selection
    private int robotindex; //The current index of robot selection

    private Sprite currentimage; //The current thumbnail to be previewed
    private string currenttext; //The current text to be displayed

    private string selectedField; //the selected field file path
    private string selectedRobot; //the selected robot file path
    private string selectedFieldName; //the selected field name
    private string selectedRobotName; //the selected robot name
    private Sprite selectedFieldImage; //the selected field image

    private FileBrowser fieldBrowser = null; //Custom Field Browser
    private bool customfieldon = true; // Is true when the custom field browser is active

    private FileBrowser robotBrowser = null; //Custom Robot Browser
    private bool customroboton = true; //Is true when the custom robot browser is active

    public static GameObject InputConflict; //The text GUI object that only is active when there is an input conflict.

    public static bool fullscreen = false; //True when fullscreen, false when windowed
    public static int resolutionsetting = 0; //Resolution setting index 
    private int[] xresolution = new int[10]; //The widths of the different resolution settings
    private int[] yresolution = new int[10]; //The heights of the different resolution settings



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
         InitCustomField();
         InitCustomRobot();
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
        GameObject.Find("FieldSelectText").GetComponent<Text>().text = selectedFieldName;
        GameObject.Find("RobotSelectText").GetComponent<Text>().text = selectedRobotName;
        if (selectedFieldImage != null) GameObject.Find("FieldSelectImage").GetComponent<Image>().sprite = selectedFieldImage;
    }

    //Method to render to LoadField GUI objects
    void RenderLoadField()
    {
        Main.SetActive(false);
        LoadField.SetActive(true);
        DefaultPanel.SetActive(false);

        //Updates the preview thumbnail and text
        GameObject.Find("FieldText").GetComponent<Text>().text = currenttext;
        GameObject.Find("FieldImage").GetComponent<Image>().sprite = currentimage;

        //If there is nothing found in the field folder, provide error message
        if (fields.Count <= 0)
        {
            foreach (GameObject gameobject in GameObject.FindGameObjectsWithTag("FieldNavigation")) gameobject.SetActive(false);
            GameObject.Find("NoFieldFound").SetActive(true);
        }
        else
        {
            foreach (GameObject gameobject in GameObject.FindGameObjectsWithTag("FieldNavigation")) gameobject.SetActive(true);
            GameObject.Find("NoFieldFound").SetActive(false);
        }
    }

    //Method to render the LoadRobot GUI objects
    void RenderLoadRobot()
    {
        Main.SetActive(false);
        LoadRobot.SetActive(true);
        DefaultPanel.SetActive(false);

        //Updates the preview thumbnail and text
        GameObject.Find("RobotText").GetComponent<Text>().text = currenttext;
        GameObject.Find("RobotImage").GetComponent<Image>().sprite = currentimage;

        //If there is nothing found in the robot folder, provide error message
        if (robots.Count <= 0)
        {
            foreach (GameObject gameobject in GameObject.FindGameObjectsWithTag("RobotNavigation")) gameobject.SetActive(false);
            GameObject.Find("NoRobotFound").SetActive(true);
        }
        else
        {
            foreach (GameObject gameobject in GameObject.FindGameObjectsWithTag("RobotNavigation")) gameobject.SetActive(true);
            GameObject.Find("NoRobotFound").SetActive(false);
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

    //Method to render the custom field/robot browsers
    void RenderCustom()
    {
        DefaultPanel.SetActive(true);
        LoadField.SetActive(false);
        LoadRobot.SetActive(false);
        //If the field browser is no longer active, switch menu states accordingly.
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
        fields.Clear();
        currentMenu = Menu.LoadField;
        string[] folders = System.IO.Directory.GetDirectories(filepath+"\\Fields");
        foreach (string field in folders)
        {
            if (File.Exists(field+"\\definition.bxdf")) fields.Add(new DirectoryInfo(field).Name);
        }
        UpdatePreview();
    }

    //Loads the robot arraylist with the folder names and switches the current menu to the loadrobot menu.
    public void LoadRobotButtonClicked()
    {
        robots.Clear();
        currentMenu = Menu.LoadRobot;
        string[] folders = System.IO.Directory.GetDirectories(filepath + "\\Robots");
        foreach (string robot in folders)
        {
            if (File.Exists(robot + "\\skeleton.bxdj")) robots.Add(new DirectoryInfo(robot).Name);
        }
        UpdatePreview();
    }

    //Switches the current menu to the graphics settings menu.
    public void GraphicsButtonClicked()
    {
        currentMenu = Menu.Graphics;
    }

    //Switches the current menu to the input settings menu.
    public void InputButtonClicked()
    {
        currentMenu = Menu.Input;
    }

    //Makes the Start Button display different things depending on whether a robot/field is loaded or not.
    public void StartButtonHover()
    {
        if (selectedFieldName.Equals("No Field Loaded!") || selectedRobotName.Equals("No Robot Loaded!")) 
        {
            GameObject.Find("StartButton").GetComponent<Image>().color = Color.red;
            Text buttontext = GameObject.Find("ReadyText").GetComponent<Text>();
            buttontext.text = ( "Can't start without robot/field loaded!");
            buttontext.fontSize = 12;
        }
        else GameObject.Find("StartButton").GetComponent<Image>().color = Color.green;
    }

    //Resets start button text when it is no longer hovered over.
    public void StartButtonExit()
    {
        Text buttontext = GameObject.Find("ReadyText").GetComponent<Text>();
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
        selectedRobot = (filepath + "//Robots//" + robots[robotindex] + "\\");
        selectedRobotName = "Robot: " + currenttext;
        currentMenu = Menu.Main;
    }

    //Selects the fields, records the filename, and switches to the main menu.
    public void SelectFieldButtonClicked()
    {
        selectedField = (filepath + "//Fields//" + fields[fieldindex]+ "\\");
        selectedFieldImage = currentimage;
        selectedFieldName = "Field: " + currenttext;
        currentMenu = Menu.Main;
    }

    //Initializes the custom field browser
    public void InitCustomField()
    {
        if (fieldBrowser == null)
        {
            fieldBrowser = new FileBrowser("Load Custom Field", true);
            fieldBrowser.Active = true;
            fieldBrowser.OnComplete += (object obj) =>
            {
                fieldBrowser.Active = true;
                string fileLocation = (string)obj;
                // If dir was selected...
                if (File.Exists(fileLocation + "\\definition.bxdf"))
                {
                    fileLocation += "\\definition.bxdf";
                }
                DirectoryInfo parent = Directory.GetParent(fileLocation);
                if (parent != null && parent.Exists && File.Exists(parent.FullName + "\\definition.bxdf"))
                {
					selectedField = (parent.FullName+"\\");
                    if (File.Exists(parent.FullName + "\\thumbnail.png")) selectedFieldImage = Sprite.Create(Extensions.LoadPNG(parent.FullName + "\\thumbnail.png"), new Rect(0.0f, 0.0f, 1280.0f, 720.0f), new Vector2(0.5f, 0.5f), 1000);
                    else currentimage = Sprite.Create(Extensions.LoadPNG(Application.dataPath + "\\Resources\\Images\\defaulfield.png"), new Rect(0.0f, 0.0f, 1280.0f, 720.0f), new Vector2(0.5f, 0.5f), 1000);
                    selectedFieldName = "Field: " + parent.Name;
                    currentMenu = Menu.Main;
                    customfieldon = false;
                }
                else
                {
                    UserMessageManager.Dispatch("Invalid selection!", 10f);
                }
            };
        }
        if (customfieldon) fieldBrowser.Render(); //Only renders the field browser when needed
    }

    //Activates the custom field browser
    public void LoadCustomField()
    {
        if (!fieldBrowser.Active) fieldBrowser.Active = true;
        customfieldon = true;
        currentMenu = Menu.Custom;
    }

    //Initializes the custom robot browser
    public void InitCustomRobot()
    {
        if (robotBrowser == null)
        {
            robotBrowser = new FileBrowser("Load Custom Robot", true);
            robotBrowser.Active = true;
            robotBrowser.OnComplete += (object obj) =>
            {
                robotBrowser.Active = true;
                string fileLocation = (string)obj;
                // If dir was selected...
                if (File.Exists(fileLocation + "\\skeleton.bxdj"))
                {
                    fileLocation += "\\skeleton.bxdf";
                }
                DirectoryInfo parent = Directory.GetParent(fileLocation);
                if (parent != null && parent.Exists && File.Exists(parent.FullName + "\\skeleton.bxdj"))
                {
                    selectedRobot = (parent.FullName + "\\");
                    selectedRobotName = "Robot: " + parent.Name;
                    currentMenu = Menu.Main;
                    customroboton = false;
                }
                else
                {
                    UserMessageManager.Dispatch("Invalid selection!", 10f);
                }
            };
        }
        if (customroboton) robotBrowser.Render(); //Only renders the robot browser when needed
    }

    //Activates the custom robot loader
    public void LoadCustomRobot()
    {
        if (!robotBrowser.Active) robotBrowser.Active = true;
        customroboton = true;
        currentMenu = Menu.Custom;
    }

    //Updates preview information with thumbnail and title.
    void UpdatePreview()
    {
        if (currentMenu == Menu.LoadField)
        {
            currenttext = (string)fields[fieldindex];
            if (File.Exists(filepath + "\\Fields\\" + fields[fieldindex] + "\\thumbnail.png")) currentimage = Sprite.Create(Extensions.LoadPNG(filepath + "\\Fields\\" + fields[fieldindex] + "\\thumbnail.png"), new Rect(0.0f, 0.0f, 1280.0f, 720.0f), new Vector2(0.5f, 0.5f), 1000);
            else currentimage = Sprite.Create(Extensions.LoadPNG(Application.dataPath + "\\Resources\\Images\\defaultfield.png"), new Rect(0.0f, 0.0f, 1280.0f, 720.0f), new Vector2(0.5f, 0.5f), 1000);
        }
        else
        {
            currenttext = (string)robots[robotindex];
            if (File.Exists(filepath + "\\Robots\\" + robots[robotindex] + "\\thumbnail.png")) currentimage = Sprite.Create(Extensions.LoadPNG(filepath + "\\Robots\\" + robots[robotindex] + "\\thumbnail.png"), new Rect(0.0f, 0.0f, 1280.0f, 720.0f), new Vector2(0.5f, 0.5f), 1000);
            else currentimage = Sprite.Create(Extensions.LoadPNG(Application.dataPath + "\\Resources\\Images\\defaultrobot.png"), new Rect(0.0f, 0.0f, 1280.0f, 720.0f), new Vector2(0.5f, 0.5f), 1000);
        }
    }

    #endregion
    #region Other Methods
    //Resets input defaults when button is pressed
    public void InputDefaultPressed()
    {
        Controls.ResetDefaults();
    }
    //Applies graphics settings.
    public void ApplyGraphics()
    {
        Screen.SetResolution(xresolution[resolutionsetting], yresolution[resolutionsetting], fullscreen);
    }
    #endregion
    void Start () {
        //Initializes the filepath and field/robot array lists.
        filepath = Directory.GetParent(Application.dataPath).FullName;
        fields = new ArrayList();
        robots = new ArrayList();

        selectedRobotName = "No Robot Loaded!";
        selectedFieldName = "No Field Loaded!";
        customfieldon = false;
        customroboton = false;

        InputConflict = GameObject.Find("InputConflict"); //Sets the input conflict 

        //Initializes the array of resolution settings
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
