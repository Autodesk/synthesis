using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class multiplayer_Init : MonoBehaviour
{
    private GUIController gui;
    private PhysicMaterial chuteMaterial;
    private GameObject activeField;
    private GameObject cameraObject;
    private DynamicCamera1 dynamicCamera1;
    private DynamicCamera2 dynamicCamera2;
    private DynamicCamera3 dynamicCamera3;
    private DynamicCamera4 dynamicCamera4;
    private DynamicCamera5 dynamicCamera5;
    private DynamicCamera6 dynamicCamera6;
    private GameObject light;
    private UnityFieldDefinition field;
    private List<GameObject> totes;
    public unityPacket udp;
    public string filePath;
    private Quaternion rotation;
    private volatile int reloadRobotInFrames;
    private volatile int reloadFieldInFrames;
    private FileBrowser fieldBrowser = null;
    private bool fieldLoaded = false;

    #region GUI Stuf
    /// <summary>
    /// Default textures.
    /// </summary>
    private Texture2D buttonTexture;
    private Texture2D greyWindowTexture;
    private Texture2D darkGreyWindowTexture;
    private Texture2D lightGreyWindowTexture;
    private Texture2D transparentWindowTexture;
    /// <summary>
	/// Selected button texture.
	/// </summary>
	private Texture2D buttonSelected;
    /// <summary>
	/// Gravity-Regular font.
	/// </summary>
	private Font gravityRegular;
    private Font russoOne;
    /// <summary>
	/// Custom GUIStyle for windows.
	/// </summary>
    private GUIStyle menuWindow;
    /// <summary>
	/// Custom GUIStyle for buttons.
	/// </summary>
    private GUIStyle menuButton;
    /// <summary>
    /// Orient Robot Window
    /// </summary>
    private OverlayWindow oWindow;
    #endregion

    public GameObject player;
    public int numPlayers = 0;

    public multiplayer_Init()
    {
        udp = new unityPacket();

        reloadRobotInFrames = -1;
        reloadFieldInFrames = -1;
        rotation = Quaternion.identity;
    }

    public void SpawnPlayer()
    {
        if (numPlayers < 6)
        {
            numPlayers++;
            Instantiate(player, new Vector3(0, 0, 0), Quaternion.identity);
        }
    }

    /// <summary>
    /// Opens windows to orient the robot
    /// </summary>
    public void ShowOrient()
    {
        List<string> titles = new List<string>();
        titles.Add("Left");
        titles.Add("Right");
        titles.Add("Forward");
        titles.Add("Back");
        titles.Add("Save Orientation");
        titles.Add("Close");
        titles.Add("Default");

        List<Rect> rects = new List<Rect>();
        rects.Add(new Rect(40, 200, 105, 35));
        rects.Add(new Rect(245, 200, 105, 35));
        rects.Add(new Rect(147, 155, 105, 35));
        rects.Add(new Rect(147, 245, 105, 35));
        rects.Add(new Rect(110, 95, 190, 35));
        rects.Add(new Rect(270, 50, 90, 35));
        rects.Add(new Rect(50, 50, 90, 35));

        oWindow = new TextWindow("Orient Robot", new Rect((Screen.width / 2) - 150, (Screen.height / 2) - 125, 400, 300),
                                             new string[0], new Rect[0], titles.ToArray(), rects.ToArray());
        //The directional buttons lift the robot to avoid collison with objects, rotates it, and saves the applied rotation to a vector3
        /** gui.AddWindow("Orient Robot", oWindow, (object o)=>{
			mainNode.transform.position = new Vector3(mainNode.transform.position.x, 1, mainNode.transform.position.z);
			switch((int)o)
			{
			case 0:
				mainNode.transform.position = new Vector3(mainNode.transform.position.x, 1, mainNode.transform.position.z);
				mainNode.transform.Rotate(new Vector3(0, 0, 45));
                break;
			case 1:
				mainNode.transform.position = new Vector3(mainNode.transform.position.x, 1, mainNode.transform.position.z);
				mainNode.transform.Rotate(new Vector3(0, 0, -45));
				break;
			case 2:
				mainNode.transform.position = new Vector3(mainNode.transform.position.x, 1, mainNode.transform.position.z);
				mainNode.transform.Rotate(new Vector3(45, 0, 0));
				break;
			case 3:
				mainNode.transform.position = new Vector3(mainNode.transform.position.x, 1, mainNode.transform.position.z);
				mainNode.transform.Rotate(new Vector3(-45, 0, 0));
				break;
			case 4:
				rotation = mainNode.transform.rotation;
				Debug.Log(rotation);
				break;
			case 5:
				oWindow.Active = false;
				break;
			case 6:
				rotation = Quaternion.identity;
				mainNode.transform.rotation = rotation;
				break;

			}			
		}); **/
    }

    void HideGuiSidebar()
    {
        gui.guiVisible = false;
        if (dynamicCamera1 != null)
        {
            dynamicCamera1.EnableMoving();
        }
        if (dynamicCamera2 != null)
        {
            dynamicCamera2.EnableMoving();
        }
        if (dynamicCamera3 != null)
        {
            dynamicCamera3.EnableMoving();
        }
        if (dynamicCamera4 != null)
        {
            dynamicCamera4.EnableMoving();
        }
        if (dynamicCamera5 != null)
        {
            dynamicCamera5.EnableMoving();
        }
        if (dynamicCamera6 != null)
        {
            dynamicCamera6.EnableMoving();
        }
    }

    void ShowGuiSidebar()
    {
        if (dynamicCamera1 != null)
        {
            dynamicCamera1.DisableMoving();
        }
        if (dynamicCamera2 != null)
        {
            dynamicCamera2.DisableMoving();
        }
        if (dynamicCamera3 != null)
        {
            dynamicCamera3.DisableMoving();
        }
        if (dynamicCamera4 != null)
        {
            dynamicCamera4.DisableMoving();
        }
        if (dynamicCamera5 != null)
        {
            dynamicCamera5.DisableMoving();
        }
        if (dynamicCamera6 != null)
        {
            dynamicCamera6.DisableMoving();
        }
    }

    [STAThread]
    void OnGUI()
    {
        //Custom style for windows
        menuWindow = new GUIStyle(GUI.skin.window);
        menuWindow.normal.background = transparentWindowTexture;
        menuWindow.onNormal.background = transparentWindowTexture;
        menuWindow.font = russoOne;

        //Custom style for buttons
        menuButton = new GUIStyle(GUI.skin.button);
        menuButton.font = russoOne;
        menuButton.normal.background = buttonTexture;
        menuButton.hover.background = buttonSelected;
        menuButton.active.background = buttonSelected;
        menuButton.onNormal.background = buttonSelected;
        menuButton.onHover.background = buttonSelected;
        menuButton.onActive.background = buttonSelected;

        if (gui == null)
        {
            gui = new GUIController();
            gui.hideGuiCallback = HideGuiSidebar;
            gui.showGuiCallback = ShowGuiSidebar;

            gui.AddWindow("Load Robot", new FileBrowser("Load Robot"), (object o) =>
            {
                string fileLocation = (string)o;
                // If dir was selected...
                if (File.Exists(fileLocation + "\\skeleton.bxdj"))
                {
                    fileLocation += "\\skeleton.bxdj";
                }
                DirectoryInfo parent = Directory.GetParent(fileLocation);
                if (parent != null && parent.Exists && File.Exists(parent.FullName + "\\skeleton.bxdj"))
                {
                    this.filePath = parent.FullName + "\\";
                    reloadRobotInFrames = 2;
                }
                else
                {
                    UserMessageManager.Dispatch("Invalid selection!", 10f);
                }

                if (dynamicCamera1 != null)
                {
                    dynamicCamera1.EnableMoving();
                }
                if (dynamicCamera2 != null)
                {
                    dynamicCamera2.EnableMoving();
                }
                if (dynamicCamera3 != null)
                {
                    dynamicCamera3.EnableMoving();
                }
                if (dynamicCamera4 != null)
                {
                    dynamicCamera4.EnableMoving();
                }
                if (dynamicCamera5 != null)
                {
                    dynamicCamera5.EnableMoving();
                }
                if (dynamicCamera6 != null)
                {
                    dynamicCamera6.EnableMoving();
                }
            });

            //shows button to manually orient the robot
            ShowOrient();

            if (!File.Exists(filePath + "\\skeleton.bxdj"))
            {
                gui.DoAction("Load Model");
            }

            gui.AddWindow("Add Player", new FileBrowser("Load Robot"), (object o) =>
            {
                string fileLocation = (string)o;
                // If dir was selected...
                if (File.Exists(fileLocation + "\\skeleton.bxdj"))
                {
                    fileLocation += "\\skeleton.bxdj";
                }
                DirectoryInfo parent = Directory.GetParent(fileLocation);
                if (parent != null && parent.Exists && File.Exists(parent.FullName + "\\skeleton.bxdj"))
                {
                    this.filePath = parent.FullName + "\\";
                    if (numPlayers == 1)
                    {
                        GameObject.Find("Camera").GetComponent<Camera>().rect = new Rect(0, 0.5f, 1, 0.5f);
                        cameraObject = new GameObject("Camera1");
                        cameraObject.AddComponent<Camera>();
                        cameraObject.GetComponent<Camera>().rect = new Rect(0, 0, 1, 0.5f);
                        cameraObject.GetComponent<Camera>().backgroundColor = new Color(.3f, .3f, .3f);
                        dynamicCamera2 = cameraObject.AddComponent<DynamicCamera2>();
                    }
                    else if (numPlayers == 2)
                    {
                        GameObject.Find("Camera").GetComponent<Camera>().rect = new Rect(0, 0.5f, 0.5f, 0.5f);
                        GameObject.Find("Camera1").GetComponent<Camera>().rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
                        cameraObject = new GameObject("Camera2");
                        cameraObject.AddComponent<Camera>();
                        cameraObject.GetComponent<Camera>().rect = new Rect(0, 0, 1, 0.5f);
                        cameraObject.GetComponent<Camera>().backgroundColor = new Color(.3f, .3f, .3f);
                        dynamicCamera3 = cameraObject.AddComponent<DynamicCamera3>();
                    }
                    else if (numPlayers == 3)
                    {
                        GameObject.Find("Camera").GetComponent<Camera>().rect = new Rect(0, 0.5f, 0.5f, 0.5f);
                        GameObject.Find("Camera1").GetComponent<Camera>().rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
                        GameObject.Find("Camera2").GetComponent<Camera>().rect = new Rect(0, 0, 0.5f, 0.5f);
                        cameraObject = new GameObject("Camera3");
                        cameraObject.AddComponent<Camera>();
                        cameraObject.GetComponent<Camera>().rect = new Rect(0.5f, 0, 0.5f, 0.5f);
                        cameraObject.GetComponent<Camera>().backgroundColor = new Color(.3f, .3f, .3f);
                        dynamicCamera4 = cameraObject.AddComponent<DynamicCamera4>();
                    }
                    else if (numPlayers == 4)
                    {
                        GameObject.Find("Camera").GetComponent<Camera>().rect = new Rect(0, 0.5f, 0.5f, 0.5f);
                        GameObject.Find("Camera1").GetComponent<Camera>().rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
                        GameObject.Find("Camera2").GetComponent<Camera>().rect = new Rect(0, 0, 0.335f, 0.5f);
                        GameObject.Find("Camera3").GetComponent<Camera>().rect = new Rect(0.335f, 0, 0.33f, 0.5f);
                        cameraObject = new GameObject("Camera4");
                        cameraObject.AddComponent<Camera>();
                        cameraObject.GetComponent<Camera>().rect = new Rect(0.665f, 0, 0.335f, 0.5f);
                        cameraObject.GetComponent<Camera>().backgroundColor = new Color(.3f, .3f, .3f);
                        dynamicCamera5 = cameraObject.AddComponent<DynamicCamera5>();
                    }
                    else if (numPlayers == 5)
                    {
                        GameObject.Find("Camera").GetComponent<Camera>().rect = new Rect(0, 0.5f, 0.335f, 0.5f);
                        GameObject.Find("Camera1").GetComponent<Camera>().rect = new Rect(0.335f, 0.5f, 0.33f, 0.5f);
                        GameObject.Find("Camera2").GetComponent<Camera>().rect = new Rect(0.665f, 0.5f, 0.335f, 0.5f);
                        GameObject.Find("Camera3").GetComponent<Camera>().rect = new Rect(0, 0, 0.335f, 0.5f);
                        GameObject.Find("Camera4").GetComponent<Camera>().rect = new Rect(0.335f, 0, 0.33f, 0.5f);
                        cameraObject = new GameObject("Camera5");
                        cameraObject.AddComponent<Camera>();
                        cameraObject.GetComponent<Camera>().rect = new Rect(0.665f, 0, 0.335f, 0.5f);
                        cameraObject.GetComponent<Camera>().backgroundColor = new Color(.3f, .3f, .3f);
                        dynamicCamera6 = cameraObject.AddComponent<DynamicCamera6>();
                    }
                    reloadRobotInFrames = 2;
                }
                else
                {
                    UserMessageManager.Dispatch("Invalid selection!", 10f);
                }

                if (dynamicCamera1 != null)
                {
                    dynamicCamera1.EnableMoving();
                }
                if (dynamicCamera2 != null)
                {
                    dynamicCamera2.EnableMoving();
                }
                if (dynamicCamera3 != null)
                {
                    dynamicCamera3.EnableMoving();
                }
                if (dynamicCamera4 != null)
                {
                    dynamicCamera4.EnableMoving();
                }
                if (dynamicCamera5 != null)
                {
                    dynamicCamera5.EnableMoving();
                }
                if (dynamicCamera6 != null)
                {
                    dynamicCamera6.EnableMoving();
                }
            });

            gui.AddWindow("Quit to Main Menu", new DialogWindow("Quit to Main Menu?", "Yes", "No"), (object o) =>
            {
                if ((int)o == 0)
                {
                    Application.LoadLevel("MainMenu");
                }
            });

            gui.AddWindow("Quit to Desktop", new DialogWindow("Quit to Desktop?", "Yes", "No"), (object o) =>
            {
                if ((int)o == 0)
                {
                    Application.Quit();
                }
            });
        }

        if (Input.GetMouseButtonUp(0) && !gui.ClickedInsideWindow())
        {
            HideGuiSidebar();
            gui.HideAllWindows();
        }

        if (fieldBrowser == null)
        {
            fieldBrowser = new FileBrowser("Load Field", false);
            fieldBrowser.Active = false;
            fieldBrowser.OnComplete += (object obj) =>
            {
                fieldBrowser.Active = false;
                string fileLocation = (string)obj;
                // If dir was selected...
                if (File.Exists(fileLocation + "\\definition.bxdf"))
                {
                    fileLocation += "\\definition.bxdf";
                }
                DirectoryInfo parent = Directory.GetParent(fileLocation);
                if (parent != null && parent.Exists && File.Exists(parent.FullName + "\\definition.bxdf"))
                {
                    this.filePath = parent.FullName + "\\";
                    fieldBrowser.Active = false;
                    reloadFieldInFrames = 2;
                }
                else
                {
                    UserMessageManager.Dispatch("Invalid selection!", 10f);
                }
            };
        }

        if (!fieldLoaded)
            fieldBrowser.Render();

        else
        {
            // The Menu button on the top left corner
            GUI.Window(1, new Rect(0, 0, gui.GetSidebarWidth(), 25),
                (int windowID) =>
                {
                    if (GUI.Button(new Rect(0, 0, gui.GetSidebarWidth(), 25), "Menu", menuButton))
                        gui.EscPressed();
                },
                "",
                menuWindow
            );

            gui.Render();
        }

        UserMessageManager.Render();

        if (reloadRobotInFrames >= 0 || reloadFieldInFrames >= 0)
        {
            GUI.backgroundColor = new Color(1, 1, 1, 0.5f);
            GUI.Box(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 25, 200, 25), "Loading... Please Wait", gui.BlackBoxStyle);
        }
    }

    private void TryLoadField()
    {
        activeField = new GameObject("Field");

        FieldDefinition.Factory = delegate (Guid guid, string name)
        {
            return new UnityFieldDefinition(guid, name);
        };

        string loadResult;
        field = (UnityFieldDefinition)BXDFProperties.ReadProperties(filePath + "definition.bxdf", out loadResult);
        field.CreateTransform(activeField.transform);
        fieldLoaded = field.CreateMesh(filePath + "mesh.bxda");
    }

    void Start()
    {
        Physics.gravity = new Vector3(0, -9.8f, 0);
        Physics.solverIterationCount = 30;
        Physics.minPenetrationForPenalty = 0.001f;

        cameraObject = new GameObject("Camera");
        cameraObject.AddComponent<Camera>();
        cameraObject.GetComponent<Camera>().backgroundColor = new Color(.3f, .3f, .3f);
        dynamicCamera1 = cameraObject.AddComponent<DynamicCamera1>();

        light = new GameObject("Light");
        Light lightComponent = light.AddComponent<Light>();
        lightComponent.type = LightType.Spot;
        lightComponent.intensity = 1.5f;
        lightComponent.range = 30f;
        lightComponent.spotAngle = 135;
        light.transform.position = new Vector3(0f, 10f, 0f);
        light.transform.Rotate(90f, 0f, 0f);

        chuteMaterial = new PhysicMaterial("chuteMaterial");
        chuteMaterial.dynamicFriction = 0f;
        chuteMaterial.staticFriction = 0f;
        chuteMaterial.frictionCombine = PhysicMaterialCombine.Minimum;

        totes = new List<GameObject>();

        filePath = PlayerPrefs.GetString("Field");
        reloadFieldInFrames = 2;

        reloadRobotInFrames = -1;

        //Loads textures and fonts
        buttonTexture = Resources.Load("Images/greyButton") as Texture2D;
        buttonSelected = Resources.Load("Images/selectedbuttontexture") as Texture2D;
        gravityRegular = Resources.Load("Fonts/Gravity-Regular") as Font;
        russoOne = Resources.Load("Fonts/Russo_One") as Font;
        greyWindowTexture = Resources.Load("Images/greyBackground") as Texture2D;
        darkGreyWindowTexture = Resources.Load("Images/darkGreyBackground") as Texture2D;
        lightGreyWindowTexture = Resources.Load("Images/lightGreyBackground") as Texture2D;
        transparentWindowTexture = Resources.Load("Images/transparentBackground") as Texture2D;
    }

    void OnEnable()
    {
        udp.Start();
    }

    void OnDisable()
    {
        udp.Stop();
    }

    void Update()
    {
        if (Input.GetKeyDown("i"))
        {
            SpawnPlayer();
        }

        if (reloadRobotInFrames >= 0 && reloadRobotInFrames-- == 0)
        {
            reloadRobotInFrames = -1;
            SpawnPlayer();
        }

        if (reloadFieldInFrames >= 0 && reloadFieldInFrames-- == 0)
        {
            reloadFieldInFrames = -1;
            TryLoadField();

            if (fieldLoaded)
            {
                filePath = PlayerPrefs.GetString("Robot");
                SpawnPlayer();
            }
            else
            {
                UserMessageManager.Dispatch("Incompatible Mesh!", 10f);
                Application.LoadLevel(0);
            }
        }

        if (Input.GetKeyDown(Controls.ControlKey[(int)Controls.Control.RobotOrient]))
            if (!oWindow.Active)
            {
                gui.EscPressed();
                oWindow.Active = true;
            }
            else
            {
                gui.EscPressed();
                oWindow.Active = false;
            }
    }
}