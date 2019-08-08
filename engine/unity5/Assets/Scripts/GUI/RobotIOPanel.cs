using Synthesis.Input;
using Synthesis.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Synthesis.GUI
{
    /// <summary>
    /// User interface for viewing emulated RoboRIO outputs and setting its inputs
    /// </summary>
    public class RobotIOPanel : MonoBehaviour
    {
        /// <summary>
        /// Representation of the field which displays a single input or output
        /// </summary>
        public class RobotIOField
        {
            public GameObject gameObject;
            public Text label;
            public InputField inputField;
            private Action<RobotIOField> updateFunction;
            private float scaleFactor = 1;

            private static Color ENABLED_COLOR = Color.white;
            private static Color DISABLED_COLOR = new Color(0.47f, 0.47f, 0.47f);

            /// <summary>
            /// Instantiates a new RobotIOField UI element
            /// </summary>
            /// <param name="name"></param> The name of the field to use in the editor and in the label
            /// <param name="parent"></param> The parent underwhich to instantiate this element
            /// <param name="update"></param> The update function to call to handle its value
            public RobotIOField(string name, GameObject parent, Action<RobotIOField> update)
            {
                gameObject = Instantiate(RobotIOPanel.Instance.robotIOFieldPrefab, parent.transform) as GameObject;
                gameObject.name = name;

                label = gameObject.GetComponentInChildren<Text>();
                label.text = name;

                inputField = gameObject.GetComponentInChildren<InputField>();

                updateFunction = update;
                Update();
            }

            /// <summary>
            /// Update the displayed values and input state for transmission
            /// </summary>
            public void Update()
            {
                updateFunction(this);
            }

            /// <summary>
            /// Update the displayed values and input state for transmission and resize
            /// </summary>
            /// <param name="factor"></param> Resize factor
            public void Update(float factor)
            {
                Update();
                if (Math.Abs(factor - scaleFactor) > EPSILON) // Scale UI
                {
                    scaleFactor = factor;

                    label.fontSize = (int)(RobotIOPanel.Instance.robotIOFieldPrefab.GetComponentInChildren<Text>().fontSize * scaleFactor);
                    label.gameObject.GetComponent<RectTransform>().sizeDelta = RobotIOPanel.Instance.robotIOFieldPrefab.GetComponentInChildren<Text>().gameObject.GetComponent<RectTransform>().sizeDelta * scaleFactor;
                    inputField.textComponent.fontSize = (int)(RobotIOPanel.Instance.robotIOFieldPrefab.GetComponentInChildren<InputField>().textComponent.fontSize * factor);
                    inputField.gameObject.GetComponent<RectTransform>().sizeDelta = RobotIOPanel.Instance.robotIOFieldPrefab.GetComponentInChildren<InputField>().gameObject.GetComponent<RectTransform>().sizeDelta * scaleFactor;
                }
            }

            /// <summary>
            /// Enable or disable the field--grey out
            /// </summary>
            /// <param name="e"></param> Whether to enable or not
            public void SetEnable(bool e)
            {
                label.color = e ? ENABLED_COLOR : DISABLED_COLOR;
                inputField.textComponent.color = e ? ENABLED_COLOR : DISABLED_COLOR;
            }
        }

        /// <summary>
        /// Representation of a group of RobotIOField UI elements, including a scroll rect and a title
        /// </summary>
        public class RobotIOGroup
        {
            /// <summary>
            /// The various group types
            /// </summary>
            public enum Type
            {
                PWM,
                CAN,
                DIO,
                AI,
                AO
            }

            private const float HEIGHT_REF = 270;
            private const float WIDTH_REF = 170;
            private float scaleFactor;

            public GameObject gameObject;
            private Text title;
            public List<RobotIOField> robotIOFields;

            public GameObject GetPanel()
            {
                return Auxiliary.FindObject(gameObject, "Content");
            }

            /// <summary>
            /// Instantiate a new RobotIOGroup UI element
            /// </summary>
            /// <param name="type"></param> The type, used to set the title
            /// <param name="parent"></param> The parent to instantiate the new element under
            public RobotIOGroup(Type type, GameObject parent)
            {
                gameObject = Instantiate(RobotIOPanel.Instance.robotIOGroupPrefab, parent.transform) as GameObject;
                gameObject.name = type.ToString();

                title = gameObject.GetComponentInChildren<Text>();
                title.name = type.ToString() + " Title";
                title.text = type.ToString();


                robotIOFields = new List<RobotIOField>();
            }

            /// <summary>
            /// Update the RobotIOGroup UI element
            /// </summary>
            /// <returns>True if any input field is focused</returns>
            public bool Update()
            {
                bool focused = false;

                // Calculate factor to scale UI by
                float yFactor = gameObject.GetComponent<RectTransform>().sizeDelta.y / HEIGHT_REF;
                float xFactor = gameObject.GetComponent<RectTransform>().sizeDelta.x / WIDTH_REF;
                float factor = Math.Min(xFactor, yFactor); // Find which dimension is the size limiting factor

                if (Math.Abs(factor - scaleFactor) > EPSILON)
                {
                    scaleFactor = factor;
                    title.fontSize = (int)(RobotIOPanel.Instance.robotIOGroupPrefab.GetComponentInChildren<Text>().fontSize * scaleFactor);

                    foreach (var i in robotIOFields)
                    {
                        focused = focused || i.inputField.isFocused;
                        i.Update(scaleFactor);
                    }
                }
                else
                {
                    foreach (var i in robotIOFields)
                    {
                        focused = focused || i.inputField.isFocused;
                        i.Update();
                    }
                }

                if (GetPanel().GetComponent<RectTransform>().sizeDelta.y > gameObject.GetComponent<RectTransform>().sizeDelta.y)
                {
                    GetPanel().GetComponent<VerticalLayoutGroup>().padding.right = (int)(20 * scaleFactor); // Shift RobotIOFields over when scroll bar activates
                }

                return focused;
            }
        }

        /// <summary>
        /// Manager class for the robot prints panel
        /// Receives data and adds it to the UI
        /// </summary>
        public class RobotPrintManager
        {
            private const float HEIGHT_REF = 160;
            private static int FontSize;

            private static string lineBuffer;
            private static StreamReader remoteReader = null;
            private static Queue<Optional<string>> lineQueue = new Queue<Optional<string>>();

            /// <summary>
            /// Empty stream buffer into UI
            /// </summary>
            public static void Update()
            {
                if (EmulatorManager.IsVMConnected() && remoteReader == null) // Start stream if needed
                {
                    Task.Run(OpenPrintStream);
                }
                while (lineQueue.Count > 0) {
                    var line = lineQueue.Dequeue();
                    if (line.IsValid())
                    {
                        Text newPrint = Instantiate(RobotIOPanel.Instance.robotPrintPrefab.GetComponent<Text>(), RobotIOPanel.Instance.robotPrintTextContainer.transform); // Add buffer to console
                        newPrint.text = line.Get();
                        newPrint.fontSize = FontSize;

                        RectTransform rect = RobotIOPanel.Instance.robotPrintScrollContent.GetComponent<RectTransform>(); // Resize scroll area
                        rect.sizeDelta = new Vector2(rect.sizeDelta.x, RobotIOPanel.Instance.robotPrintTextContainer.GetComponent<RectTransform>().sizeDelta.y);

                        if (RobotIOPanel.Instance.autoScroll)
                        {
                            RobotIOPanel.Instance.robotPrintScrollRect.GetComponent<ScrollRect>().verticalNormalizedPosition = 0; // Scroll to bottom
                        }

                        Canvas.ForceUpdateCanvases();
                    }
                }
            }

            /// <summary>
            /// Resize text as screen size changes
            /// </summary>
            /// <param name="forceUpdate"></param>
            public static void Resize(bool forceUpdate = false)
            {
                var lastSize = FontSize;
                FontSize = (int)(RobotIOPanel.Instance.robotPrintPrefab.GetComponent<Text>().fontSize *
                    RobotIOPanel.Instance.robotPrintScrollRect.GetComponent<LayoutElement>().preferredHeight / HEIGHT_REF); // Width isn't as important as height for these
                if (lastSize != FontSize || forceUpdate)
                {
                    for (int i = 0; i < RobotIOPanel.Instance.robotPrintTextContainer.transform.childCount; i++)
                    {
                        var print = RobotIOPanel.Instance.robotPrintTextContainer.transform.GetChild(i);
                        print.GetComponent<Text>().fontSize = FontSize;
                    }
                }
            }

            /// <summary>
            /// Begin reading robot print-outs into the string buffer
            /// </summary>
            private static void OpenPrintStream()
            {
                if (remoteReader == null)
                {
                    remoteReader = EmulatorManager.CreateRobotOutputStream();
                }
                while (EmulatorManager.IsVMConnected() && EmulatorManager.IsRobotOutputStreamGood())
                {
                    try
                    {
                        var c = remoteReader.Read();
                        if (c >= 0)
                        {
                            if ((char)c == '\n')
                            {
                                lineQueue.Enqueue(new Optional<string>(lineBuffer));
                                lineBuffer = "";
                            }
                            else
                            {
                                lineBuffer += (char)c;
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        Debug.Log(e.ToString());
                        break;
                    }
                }
                remoteReader.Close();
                remoteReader = null;
                EmulatorManager.CloseRobotOutputStream();
            }
        }

        public static RobotIOPanel Instance { get; private set; }
        private const float EPSILON = 0.01f;

        private GameObject canvas;
        private GameObject mainPanel;
        private bool lastActive = false;
        private bool shouldBeActive = false;
        private bool settingsTabActive = false;

        // Robot IO view
        private GameObject displayPanel;
        public GameObject robotIOFieldPrefab;
        public GameObject robotIOGroupPrefab;

        private RobotIOGroup[] robotIOGroups;

        // Mini Camera
        private GameObject miniCameraView;
        private UnityEngine.Camera miniCamera = null;
        private float lastSetAspect = 0;
        private Rect lastSetPixelRect;

        // Robot Print-Outs
        private GameObject robotPrintPanel;
        private GameObject robotPrintScrollRect;
        private GameObject robotPrintScrollContent;
        private GameObject robotPrintTextContainer;
        public GameObject robotPrintPrefab;

        private GameObject robotPrintFooter;
        private Image autoScrollButtonImage;
        public Sprite SelectedButtonImage;
        public Sprite UnselectedButtonImage;
        public bool enablePrints = false;

        private bool autoScroll;

        public void Awake()
        {
            Instance = this;

            canvas = GameObject.Find("Canvas");
            mainPanel = Auxiliary.FindObject(canvas, "RobotIOPanel");
            mainPanel.SetActive(false); // Must start inactive, otherwise initializing mini camera will cause problems

            displayPanel = Auxiliary.FindObject(mainPanel, "RobotIODisplayPanel");
            robotIOGroups = new RobotIOGroup[(int)Enum.GetValues(typeof(RobotIOGroup.Type)).Cast<RobotIOGroup.Type>().Max() + 1]; // Set size to number of enum values

            miniCameraView = Auxiliary.FindObject(mainPanel, "MiniCameraView");

            robotPrintPanel = Auxiliary.FindObject(mainPanel, "RobotPrintPanel");
            robotPrintScrollRect = Auxiliary.FindObject(robotPrintPanel, "RobotPrintScrollRect");
            robotPrintScrollContent = Auxiliary.FindObject(robotPrintScrollRect, "Content");
            robotPrintTextContainer = Auxiliary.FindObject(robotPrintScrollContent, "PrintContainer");
            robotPrintFooter = Auxiliary.FindObject(robotPrintPanel, "RobotPrintFooter");
            autoScrollButtonImage = Auxiliary.FindObject(robotPrintFooter, "RobotPrintAutoScrollButton").GetComponent<Image>();

            autoScroll = true;
            autoScrollButtonImage.sprite = SelectedButtonImage;

            Populate();
        }

        public void Update()
        {
            if (shouldBeActive) // Handle hiding the panel. Other UIs don't hide automatically, but this one is big and should.
            {
                if (InputControl.GetButtonDown(new KeyMapping("Hide Menu", KeyCode.H, Input.Enums.KeyModifier.Ctrl), true)) // TODO make global control
                {
                    mainPanel.SetActive(!mainPanel.activeSelf);
                }
                if (SimUI.getSimUI().getTabStateMachine().CurrentState is SettingsState)
                {
                    if (mainPanel.activeSelf)
                    {
                        mainPanel.SetActive(false);
                    }
                    settingsTabActive = true;
                } else if (settingsTabActive)
                {
                    if (!mainPanel.activeSelf)
                    {
                        mainPanel.SetActive(true);
                    }
                    settingsTabActive = false;
                }
            }
            if (mainPanel.activeSelf) // Update rest of UI
            {
                bool focused = false;
                foreach (var group in robotIOGroups)
                {
                    focused = focused || group.Update();
                }
                InputControl.freeze = focused; // Prevent user inputting values from triggering controls

                if (miniCamera == null)
                {
                    InitMiniCamera();
                }
                UpdateMiniCamera();
                Resize();

                DynamicCamera.ControlEnabled = false;
            }
            else
            {
                if (lastActive)
                {
                    DynamicCamera.ControlEnabled = true;
                }
            }
            if (enablePrints && EmulatorManager.IsVMConnected()) // Update robot prints
            {
                RobotPrintManager.Update();
            }

            lastActive = mainPanel.activeSelf;
        }

        /// <summary>
        /// Toggle panel
        /// </summary>
        public void Toggle()
        {
            mainPanel.SetActive(!mainPanel.activeSelf);
            shouldBeActive = mainPanel.activeSelf;
        }

        public void SetActive(bool active)
        {
            mainPanel.SetActive(active);
        }

        /// <summary>
        /// Resize the UI so it scales with different resolutions and aspect ratios
        /// </summary>
        /// <param name="forceUpdate"></param>
        private void Resize(bool forceUpdate = false)
        {
            if (lastSetAspect != UnityEngine.Camera.main.aspect || lastSetPixelRect != UnityEngine.Camera.main.pixelRect || forceUpdate)
            {
                // Update size of mini camera to match the camera aspect ratio
                try
                { 
                    var size_delta = miniCameraView.GetComponent<RectTransform>().sizeDelta;

                    // Maintain main camera aspect ratio in mini camera view
                    miniCameraView.GetComponent<RectTransform>().sizeDelta = new Vector2(
                        size_delta.y * UnityEngine.Camera.main.aspect,
                        size_delta.y);

                    // Update camera texture size as well
                    if (miniCamera.targetTexture != null)
                        miniCamera.targetTexture.Release();
                    miniCamera.targetTexture = new RenderTexture(new RenderTextureDescriptor(
                            UnityEngine.Camera.main.pixelWidth,
                            UnityEngine.Camera.main.pixelHeight));
                    miniCameraView.GetComponent<RawImage>().texture = miniCamera.targetTexture;

                    // Fill remaining width of screen with the robot print panel
                    robotPrintPanel.GetComponent<LayoutElement>().preferredWidth = (UnityEngine.Camera.main.pixelRect.width - size_delta.x) / size_delta.x;
                    robotPrintScrollRect.GetComponent<LayoutElement>().preferredHeight = robotPrintPanel.GetComponent<RectTransform>().sizeDelta.y - robotPrintFooter.GetComponent<RectTransform>().sizeDelta.y;

                    lastSetAspect = UnityEngine.Camera.main.aspect;
                    lastSetPixelRect = UnityEngine.Camera.main.pixelRect;
                }
                catch (Exception e)
                {
                    Debug.Log(e.ToString());
                }
            }
        }

        /// <summary>
        /// Configure the mini camera for use
        /// </summary>
        private void InitMiniCamera()
        {
            try
            {
                miniCamera = Instantiate(UnityEngine.Camera.main, miniCameraView.transform); // Clone main camera
                foreach (Transform child in miniCamera.transform)
                    Destroy(child.gameObject);

                Destroy(miniCamera.GetComponent<DynamicCamera>()); // Shouldn't handle it's own position, so copy from main camera instead
            }
            catch (Exception)
            {
                miniCamera = null;
                throw;
            }
        }

        /// <summary>
        /// Update the mini camera view
        /// </summary>
        private void UpdateMiniCamera()
        {
            // Update position and orientation
            miniCamera.transform.position = UnityEngine.Camera.main.transform.position;
            miniCamera.transform.rotation = UnityEngine.Camera.main.transform.rotation;
        }

        /// <summary>
        /// Reset the robot print-out console
        /// </summary>
        public void ClearPrintConsole()
        {
            foreach (Transform child in robotPrintTextContainer.transform)
                Destroy(child.gameObject);
        }

        /// <summary>
        /// Toggles whether the print-out panel automatically scrolls down to the most recent prints
        /// </summary>
        public void ToggleAutoScroll()
        {
            autoScroll = !autoScroll;
            autoScrollButtonImage.sprite = autoScroll ? SelectedButtonImage : UnselectedButtonImage;
        }

        /// <summary>
        /// Prompt user to download the log file
        /// </summary>
        public void DownloadLog()
        {
            if (EmulationWarnings.CheckRequirement(EmulationWarnings.Requirement.VMConnected))
            {
                EmulatorManager.FetchLogFile();
            }
        }

        /// <summary>
        /// Initialize GameObjects for all robot IO
        /// </summary>
        private void Populate()
        {
            for (var i = 0; i < robotIOGroups.Length; i++)
            {
                robotIOGroups[i] = new RobotIOGroup((RobotIOGroup.Type)i, displayPanel);
            }

            for (int i = 0; i < RoboRIOConstants.NUM_PWM_HDRS + RoboRIOConstants.NUM_PWM_MXP; i++)
            {
                int j = i; // Create copy of iterator
                string name = (j < RoboRIOConstants.NUM_PWM_HDRS) ? j.ToString() : "MXP " + (j - RoboRIOConstants.NUM_PWM_HDRS).ToString();
                robotIOGroups[(int)RobotIOGroup.Type.PWM].robotIOFields.Add(
                    new RobotIOField(
                        name,
                        robotIOGroups[(int)RobotIOGroup.Type.PWM].GetPanel(),
                        (RobotIOField robotIOField) =>
                        {
                            try
                            {
                                if (j < RoboRIOConstants.NUM_PWM_HDRS)
                                {
                                    robotIOField.inputField.text = OutputManager.Instance.PwmHeaders[j].ToString();
                                }
                                else
                                {
                                    int digital_index = j - (int)RoboRIOConstants.NUM_PWM_HDRS;
                                    if (digital_index >= 4) // First 4 MXP PWM outputs have the right index, but the ones after are offset by 4
                                    {
                                        digital_index += 4;
                                    }
                                    if (OutputManager.Instance.MxpData[digital_index].Config == EmulationService.MXPData.Types.Config.Pwm)
                                    {
                                        robotIOField.inputField.text = OutputManager.Instance.MxpData[digital_index].Value.ToString();
                                        robotIOField.SetEnable(true);
                                    }
                                    else
                                    {
                                        robotIOField.inputField.text = "0";
                                        robotIOField.SetEnable(false);
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                robotIOField.inputField.text = "0";
                                if (j >= RoboRIOConstants.NUM_PWM_HDRS)
                                {
                                    robotIOField.SetEnable(false);
                                }
                            }
                            robotIOField.inputField.interactable = false;
                        }
                    )
                );
            }

            for (int i = 0; i < RoboRIOConstants.NUM_CAN_ADDRESSES; i++)
            {
                int j = i; // Create copy of iterator
                robotIOGroups[(int)RobotIOGroup.Type.CAN].robotIOFields.Add(
                    new RobotIOField(
                        j.ToString(),
                        robotIOGroups[(int)RobotIOGroup.Type.CAN].GetPanel(),
                        (RobotIOField robotIOField) =>
                        {
                            try
                            {
                                var can = OutputManager.Instance.CanMotorControllers.First(e => e.Id == j); // Throws on failure
                                robotIOField.gameObject.SetActive(true);
                                robotIOField.label.text = can.Id.ToString();
                                robotIOField.inputField.text = can.PercentOutput.ToString();
                            }
                            catch (Exception)
                            {
                                robotIOField.gameObject.SetActive(false);
                                robotIOField.inputField.text = "0";
                            }
                            robotIOField.inputField.interactable = false;
                        }
                    )
                );
            }
            robotIOGroups[(int)RobotIOGroup.Type.CAN].robotIOFields.Add(
                new RobotIOField( // Custom field to display a warning when there are no active CAN devices
                    "CAN Warning",
                    robotIOGroups[(int)RobotIOGroup.Type.CAN].GetPanel(),
                    (RobotIOField robotIOField) =>
                    {
                        robotIOField.label.text = "No active CAN\nmotor controllers";
                        robotIOField.label.alignment = TextAnchor.MiddleCenter;
                        robotIOField.label.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(
                            Instance.robotIOFieldPrefab.gameObject.GetComponent<RectTransform>().sizeDelta.x * 1.5f, robotIOField.label.gameObject.GetComponent<RectTransform>().sizeDelta.y);
                        robotIOField.inputField.gameObject.SetActive(false);
                        robotIOField.gameObject.SetActive(OutputManager.Instance.CanMotorControllers.Count == 0);
                    }
                )
            );
            for (int i = 0; i < RoboRIOConstants.NUM_DIGITAL_HDRS + RoboRIOConstants.NUM_DIGITAL_MXP; i++)
            {
                int j = i; // Create copy of iterator
                string name = (j < RoboRIOConstants.NUM_DIGITAL_HDRS) ? j.ToString() : "MXP " + (j - RoboRIOConstants.NUM_DIGITAL_HDRS).ToString();
                robotIOGroups[(int)RobotIOGroup.Type.DIO].robotIOFields.Add(
                    new RobotIOField(
                        name,
                        robotIOGroups[(int)RobotIOGroup.Type.DIO].GetPanel(),
                        (RobotIOField robotIOField) =>
                        {
                            try
                            {
                                if (j < RoboRIOConstants.NUM_DIGITAL_HDRS)
                                {
                                    string mode = "";
                                    InputManager.Instance.DigitalHeaders[j].Config = (OutputManager.Instance.DigitalHeaders.Count > j) ? OutputManager.Instance.DigitalHeaders[j].Config : EmulationService.DIOData.Types.Config.Di; // Sync
                                    if (InputManager.Instance.DigitalHeaders[j].Config == EmulationService.DIOData.Types.Config.Di)
                                    {
                                        try
                                        {
                                            mode = "(DI)";
                                            robotIOField.inputField.interactable = true;
                                            InputManager.Instance.DigitalHeaders[j].Value = int.Parse(robotIOField.inputField.text) != 0;
                                        }
                                        catch (Exception)
                                        {
                                            if (!robotIOField.inputField.isFocused && robotIOField.inputField.text == "")
                                            {
                                                robotIOField.inputField.text = "0";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        mode = "(DO)";
                                        robotIOField.inputField.interactable = false;
                                        robotIOField.inputField.text = OutputManager.Instance.DigitalHeaders[j].Value ? "1" : "0";
                                    }
                                    robotIOField.label.text = j.ToString() + " " + mode;
                                }
                                else
                                {
                                    int mxp_index = j - (int)RoboRIOConstants.NUM_DIGITAL_HDRS;
                                    string mode = "";
                                    InputManager.Instance.MxpData[mxp_index].Config = (OutputManager.Instance.MxpData.Count > mxp_index) ? OutputManager.Instance.MxpData[mxp_index].Config : EmulationService.MXPData.Types.Config.Di; // Sync
                                    if (InputManager.Instance.MxpData[mxp_index].Config == EmulationService.MXPData.Types.Config.Di)
                                    {
                                        try
                                        {
                                            mode = "(DI)";
                                            robotIOField.SetEnable(true);
                                            robotIOField.inputField.interactable = true;
                                            InputManager.Instance.MxpData[mxp_index].Value = (int.Parse(robotIOField.inputField.text) != 0) ? 1 : 0;
                                        }
                                        catch (Exception)
                                        {
                                            if (!robotIOField.inputField.isFocused && robotIOField.inputField.text == "")
                                            {
                                                robotIOField.inputField.text = "0";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        robotIOField.inputField.interactable = false;
                                        if (OutputManager.Instance.MxpData[mxp_index].Config == EmulationService.MXPData.Types.Config.Do)
                                        {
                                            robotIOField.SetEnable(true);
                                            mode = "(DO)";
                                            robotIOField.inputField.text = ((int)OutputManager.Instance.MxpData[mxp_index].Value).ToString();
                                        }
                                        else
                                        {
                                            robotIOField.inputField.text = "0";
                                            robotIOField.SetEnable(false);
                                        }
                                    }
                                    robotIOField.label.text = "MXP " + mxp_index.ToString() + " " + mode;
                                }
                            }
                            catch (Exception)
                            {
                                robotIOField.inputField.interactable = false;
                                robotIOField.inputField.text = "0";
                                if (j >= RoboRIOConstants.NUM_DIGITAL_HDRS)
                                {
                                    robotIOField.SetEnable(false);
                                }
                            }
                        }
                    )
                );
            }
            for (int i = 0; i < RoboRIOConstants.NUM_AI_HDRS + RoboRIOConstants.NUM_AI_MXP; i++)
            {
                int j = i; // Create copy of iterator
                string name = (j < RoboRIOConstants.NUM_AI_HDRS) ? j.ToString() : "MXP " + (j - RoboRIOConstants.NUM_AI_HDRS).ToString();
                robotIOGroups[(int)RobotIOGroup.Type.AI].robotIOFields.Add(
                    new RobotIOField(
                        name,
                        robotIOGroups[(int)RobotIOGroup.Type.AI].GetPanel(),
                        (RobotIOField robotIOField) =>
                        {
                            try
                            {
                                InputManager.Instance.AnalogInputs[j] = float.Parse(robotIOField.inputField.text);
                            }
                            catch (Exception)
                            {
                                if (!robotIOField.inputField.isFocused && robotIOField.inputField.text == "")
                                {
                                    robotIOField.inputField.text = "0";
                                }
                            }
                            robotIOField.inputField.interactable = true;
                        }
                    )
                );
            }
            for (int i = 0; i < RoboRIOConstants.NUM_AO_MXP; i++)
            {
                int j = i; // Create copy of iterator
                robotIOGroups[(int)RobotIOGroup.Type.AO].robotIOFields.Add(
                    new RobotIOField(
                        j.ToString(),
                        robotIOGroups[(int)RobotIOGroup.Type.AO].GetPanel(),
                        (RobotIOField robotIOField) =>
                        {
                            try
                            {
                                robotIOField.inputField.text = OutputManager.Instance.AnalogOutputs[j].ToString();
                            }
                            catch (Exception)
                            {
                                robotIOField.inputField.text = "0";
                            }
                            robotIOField.inputField.interactable = false;
                        }
                    )
                );
            }
        }
    }
}
