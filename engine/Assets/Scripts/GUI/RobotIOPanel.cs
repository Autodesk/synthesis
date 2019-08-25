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
        public abstract class RobotIOFieldBase
        {
            public GameObject gameObject;
            protected float scaleFactor = 1;

            protected static Color ENABLED_COLOR = Color.white;
            protected static Color DISABLED_COLOR = new Color(0.47f, 0.47f, 0.47f);

            /// <summary>
            /// Instantiates a new RobotIOField UI element
            /// </summary>
            /// <param name="name"></param> The name of the field to use in the editor and in the label
            /// <param name="parent"></param> The parent underwhich to instantiate this element
            public RobotIOFieldBase(GameObject prefab, string name, GameObject parent)
            {
                gameObject = Instantiate(prefab, parent.transform) as GameObject;
                gameObject.name = name;
            }

            /// <summary>
            /// Update the displayed values and input state for transmission
            /// </summary>
            public abstract void Update();

            /// <summary>
            /// Update the displayed values and input state for transmission and resize
            /// </summary>
            /// <param name="factor"></param> Resize factor
            /// <return>True if it resized</return>
            public virtual bool Update(float factor)
            {
                Update();
                if (Math.Abs(factor - scaleFactor) > EPSILON) // Scale UI
                {
                    scaleFactor = factor;
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Enable or disable the field--grey out
            /// </summary>
            /// <param name="e"></param> Whether to enable or not
            public virtual void SetEnable(bool e)
            {
            }
        }

        /// <summary>
        /// Representation of the field which displays a single text input or output
        /// </summary>
        public class RobotIOField: RobotIOFieldBase
        {
            public Text label;
            public InputField inputField;
            private Action<RobotIOField> updateFunction;

            /// <summary>
            /// Instantiates a new RobotIOField UI element
            /// </summary>
            /// <param name="name"></param> The name of the field to use in the editor and in the label
            /// <param name="parent"></param> The parent underwhich to instantiate this element
            /// <param name="update"></param> The update function to call to handle its value
            public RobotIOField(string name, GameObject parent, Action<RobotIOField> update): base(RobotIOPanel.Instance.robotIOFieldPrefab, name, parent)
            {
                label = gameObject.GetComponentInChildren<Text>();
                label.text = name;

                inputField = gameObject.GetComponentInChildren<InputField>();

                updateFunction = update;
                Update();
            }

            /// <summary>
            /// Update the displayed values and input state for transmission
            /// </summary>
            public override void Update()
            {
                updateFunction(this);
            }

            /// <summary>
            /// Update the displayed values and input state for transmission and resize
            /// </summary>
            /// <param name="factor"></param> Resize factor
            /// <returns>True if it resized</returns>
            public override bool Update(float factor)
            {
                if (base.Update(factor)) // Scale UI
                {
                    label.fontSize = (int)(RobotIOPanel.Instance.robotIOFieldPrefab.GetComponentInChildren<Text>().fontSize * scaleFactor);
                    label.gameObject.GetComponent<RectTransform>().sizeDelta = RobotIOPanel.Instance.robotIOFieldPrefab.GetComponentInChildren<Text>().gameObject.GetComponent<RectTransform>().sizeDelta * scaleFactor;

                    inputField.textComponent.fontSize = (int)(RobotIOPanel.Instance.robotIOFieldPrefab.GetComponentInChildren<InputField>().textComponent.fontSize * scaleFactor);
                    inputField.gameObject.GetComponent<RectTransform>().sizeDelta = RobotIOPanel.Instance.robotIOFieldPrefab.GetComponentInChildren<InputField>().gameObject.GetComponent<RectTransform>().sizeDelta * scaleFactor;
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Enable or disable the field--grey out
            /// </summary>
            /// <param name="e"></param> Whether to enable or not
            public override void SetEnable(bool e)
            {
                base.SetEnable(e);
                label.color = e ? ENABLED_COLOR : DISABLED_COLOR;
                inputField.textComponent.color = e ? ENABLED_COLOR : DISABLED_COLOR;
            }
        }

        /// <summary>
        /// Representation of the field with a single button input
        /// </summary>
        public class RobotIOButton : RobotIOFieldBase
        {
            public Button button;
            public Text buttonText;
            private Action<RobotIOButton> updateFunction;

            /// <summary>
            /// Instantiates a new RobotIOField UI element
            /// </summary>
            /// <param name="name"></param> The name of the field to use in the editor and in the label
            /// <param name="parent"></param> The parent underwhich to instantiate this element
            /// <param name="update"></param> The update function to call to handle its value
            public RobotIOButton(string name, GameObject parent, Action<RobotIOButton> onClick, Action<RobotIOButton> update = null) : base(RobotIOPanel.Instance.robotIOButtonPrefab, name, parent)
            {
                button = gameObject.GetComponent<Button>();
                buttonText = button.GetComponentInChildren<Text>();

                updateFunction = update;

                button.onClick.AddListener(() => { onClick(this); });
                Update();
            }

            /// <summary>
            /// Update the displayed values and input state for transmission
            /// </summary>
            public override void Update()
            {
                if (updateFunction != null)
                {
                    updateFunction(this);
                }
            }

            /// <summary>
            /// Update the displayed values and input state for transmission and resize
            /// </summary>
            /// <param name="factor"></param> Resize factor
            /// <returns>True if it resized</returns>
            public override bool Update(float factor)
            {
                if (base.Update(factor)) // Scale UI
                {
                    buttonText.fontSize = (int)(RobotIOPanel.Instance.robotIOButtonPrefab.GetComponentInChildren<Text>().fontSize * scaleFactor);
                    buttonText.gameObject.GetComponent<RectTransform>().sizeDelta = RobotIOPanel.Instance.robotIOButtonPrefab.GetComponentInChildren<Text>().GetComponent<RectTransform>().sizeDelta * scaleFactor;
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Enable or disable the field--grey out
            /// </summary>
            /// <param name="e"></param> Whether to enable or not
            public override void SetEnable(bool e)
            {
                base.SetEnable(e);
                button.enabled = e;
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
                AIO,
                MISC
            }

            private const float HEIGHT_REF = 270;
            private const float WIDTH_REF = 170;
            private float scaleFactor;

            public GameObject gameObject;
            private Text title;
            public List<RobotIOFieldBase> robotIOFields;

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

                robotIOFields = new List<RobotIOFieldBase>();
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
                        i.Update(scaleFactor);
                    }
                }
                else
                {
                    foreach (var i in robotIOFields)
                    {
                        i.Update();
                    }
                }

                foreach (var i in robotIOFields)
                {
                    if (i is RobotIOField)
                    {
                        focused = focused || ((RobotIOField)i).inputField.isFocused;
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
        public static class RobotPrintManager
        {
            private const float HEIGHT_REF = 160;
            private static float scrollHeight = 0;
            private static int FontSize = 0;

            private const int MIN_LINE_BUFFER_LEN = 30; // characters
            private static System.Diagnostics.Stopwatch timeoutTimer = new System.Diagnostics.Stopwatch();
            private const long TIMEOUT = 100; // ms

            private const int RENDER_LIMIT = 200; // Number of prints to keep. Large numbers cause lag before Unity crashes.
            private static Queue<Text> renderedPrints = new Queue<Text>();

            private static RectTransform scrollView = null;
            private static RectTransform scrollContent = null;
            private static ScrollRect scrollRect = null;

            private static bool printStreamOpen = false;
            private static StreamReader remoteReader = null;
            private static string newPrintBuffer = "";
            private static Queue<string> newPrintQueue = new Queue<string>();

            public static void Init()
            {
                scrollView = RobotIOPanel.Instance.robotPrintScrollContent.GetComponent<RectTransform>();
                scrollContent = RobotIOPanel.Instance.robotPrintTextContainer.GetComponent<RectTransform>();
                scrollRect = RobotIOPanel.Instance.robotPrintScrollRect.GetComponent<ScrollRect>();
            }

            /// <summary>
            /// Empty stream buffer into UI
            /// </summary>
            public static void Update()
            {
                if (EmulatorManager.IsTryingToRunRobotCode() && !EmulatorManager.IsRobotCodeRestarting() && !printStreamOpen) // Start stream if needed
                {
                    Task.Factory.StartNew(OpenPrintStream, TaskCreationOptions.LongRunning);
                }
                while (newPrintQueue.Count > 0)
                {
                    AddLine(newPrintQueue.Dequeue());
                }
            }

            public static void ScrollToBottom()
            {
                scrollRect.verticalNormalizedPosition = 0;
            }

            public static void AddLine(string line)
            {
                var newPrint = Instantiate(RobotIOPanel.Instance.robotPrintPrefab.GetComponent<Text>(), RobotIOPanel.Instance.robotPrintTextContainer.transform);
                newPrint.fontSize = FontSize;
                newPrint.text = line;
                renderedPrints.Enqueue(newPrint);
                if (renderedPrints.Count > RENDER_LIMIT)
                {
                    Destroy(renderedPrints.Dequeue().gameObject);
                }
                timeoutTimer.Restart();

                scrollView.sizeDelta = new Vector2(scrollView.sizeDelta.x, scrollContent.sizeDelta.y); // Resize scroll area

                if (RobotIOPanel.Instance.autoScroll)
                {
                    ScrollToBottom();
                }

                Canvas.ForceUpdateCanvases();
            }

            /// <summary>
            /// Resize text as screen size changes
            /// </summary>
            /// <param name="forceUpdate"></param>
            public static void Resize(bool forceUpdate = false)
            {
                var lastHeight = scrollHeight;
                scrollHeight = RobotIOPanel.Instance.robotPrintScrollRect.GetComponent<RectTransform>().sizeDelta.y;
                if (Math.Abs(lastHeight - scrollHeight) > EPSILON || forceUpdate)
                {
                    var lastSize = FontSize;
                    FontSize = (int)Math.Ceiling(RobotIOPanel.Instance.robotPrintPrefab.GetComponent<Text>().fontSize *
                        scrollHeight / HEIGHT_REF); // Width isn't as important as height for these
                    if (lastSize != FontSize || forceUpdate)
                    {
                        for (int i = 0; i < RobotIOPanel.Instance.robotPrintTextContainer.transform.childCount; i++)
                        {
                            var print = RobotIOPanel.Instance.robotPrintTextContainer.transform.GetChild(i);
                            print.GetComponent<Text>().fontSize = FontSize;
                        }
                    }
                }
            }

            /// <summary>
            /// Begin reading robot print-outs into the string buffer
            /// </summary>
            private static void OpenPrintStream()
            {
                printStreamOpen = true;
                if (remoteReader == null)
                {
                    remoteReader = EmulatorManager.CreateRobotOutputStream();
                }
                while (remoteReader != null && EmulatorManager.IsTryingToRunRobotCode() && !EmulatorManager.IsRobotCodeRestarting() && EmulatorManager.IsRobotOutputStreamGood() && Instance && RobotIOPanel.Instance.enablePrints)
                {
                    try
                    {
                        int r = remoteReader.Read();
                        if (r != -1)
                        {
                            char c = (char)r;
                            if (c == '\n' && (newPrintBuffer.Length > MIN_LINE_BUFFER_LEN || timeoutTimer.ElapsedMilliseconds > TIMEOUT)) // Concatenate multiple short prints if in quick enough succession
                            {
                                timeoutTimer.Restart();
                                newPrintQueue.Enqueue(newPrintBuffer);
                                newPrintBuffer = "";
                            }
                            else
                            {
                                newPrintBuffer += c;
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        Debug.Log(e.ToString());
                        break;
                    }
                }
                if (remoteReader != null)
                {
                    remoteReader.Dispose();
                    remoteReader = null;
                    if (EmulatorManager.IsRobotOutputStreamGood())
                    {
                        EmulatorManager.CloseRobotOutputStream();
                    }
                }
                printStreamOpen = false;
            }

            public static void Clear()
            {
                renderedPrints.Clear();
                foreach (Transform child in RobotIOPanel.Instance.robotPrintTextContainer.transform)
                    Destroy(child.gameObject);
            }
        }

        public static RobotIOPanel Instance { get; private set; }
        private const float EPSILON = 0.01f;

        private GameObject canvas;
        private GameObject mainPanel;
        private bool lastActive = false;
        private bool shouldBeActive = false;

        // Robot IO view
        private GameObject displayPanel;
        public GameObject robotIOFieldPrefab;
        public GameObject robotIOButtonPrefab;
        public GameObject robotIOGroupPrefab;

        private RobotIOGroup[] robotIOGroups;

        // Mini Camera
        private GameObject miniCameraView;
        private UnityEngine.Camera miniCamera = null;
        private Rect lastSetPixelRect;

        // Robot Print-Outs
        private GameObject robotPrintPanel;
        private GameObject robotPrintScrollRect;
        private GameObject robotPrintScrollContent;
        private GameObject robotPrintTextContainer;
        public GameObject robotPrintPrefab;

        private GameObject robotPrintFooter;
        private Image autoScrollButtonImage;
        private Image enablePrintsButtonImage;
        public Sprite SelectedButtonImage;
        public Sprite UnselectedButtonImage;
        private bool autoScroll = true;
        public bool enablePrints = true;

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
            autoScrollButtonImage.sprite = SelectedButtonImage;
            enablePrintsButtonImage = Auxiliary.FindObject(robotPrintFooter, "EnableRobotPrintButton").GetComponent<Image>();
            enablePrintsButtonImage.sprite = SelectedButtonImage;

            RobotPrintManager.Init();
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
            else if (lastActive)
            {
                DynamicCamera.ControlEnabled = true;
            }
            if (enablePrints && EmulatorManager.IsTryingToRunRobotCode() && !EmulatorManager.IsRobotCodeRestarting()) // Update robot prints
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
            if (!lastSetPixelRect.Equals(UnityEngine.Camera.main.pixelRect) || forceUpdate)
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

                    lastSetPixelRect = UnityEngine.Camera.main.pixelRect;
                }
                catch (Exception e)
                {
                    Debug.Log(e.ToString());
                }
            }
            RobotPrintManager.Resize(forceUpdate);
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
            RobotPrintManager.Clear();
        }

        public void ToggleRobotPrints()
        {
            bool newEnable = !enablePrints;
            // TODO warn and wait if last command didnt finish
            if(!enablePrints && newEnable && EmulatorManager.IsRobotOutputStreamGood()) // When trying to enable, wait until old connection closes
            {
                UserMessageManager.Dispatch("Waiting to close last readout connection", EmulationWarnings.WARNING_DURATION);
            }
            else
            {
                enablePrints = newEnable;
                enablePrintsButtonImage.sprite = enablePrints ? SelectedButtonImage : UnselectedButtonImage;
                if(enablePrints && !EmulatorManager.IsTryingToRunRobotCode())
                {
                    UserMessageManager.Dispatch("Readout enabled, waiting for robot program to start", EmulationWarnings.WARNING_DURATION);
                }
            }
        }

        /// <summary>
        /// Toggles whether the print-out panel automatically scrolls down to the most recent prints
        /// </summary>
        public void ToggleAutoScroll()
        {
            autoScroll = !autoScroll;
            autoScrollButtonImage.sprite = autoScroll ? SelectedButtonImage : UnselectedButtonImage;
            if (autoScroll)
            {
                RobotPrintManager.ScrollToBottom();
            }
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

            for (int i = 0; i < EmulatedRoboRIO.Constants.NUM_PWM_HDRS + EmulatedRoboRIO.Constants.NUM_PWM_MXP; i++)
            {
                int j = i; // Create copy of iterator
                string name = (j < EmulatedRoboRIO.Constants.NUM_PWM_HDRS) ? j.ToString() : "MXP " + (j - EmulatedRoboRIO.Constants.NUM_PWM_HDRS).ToString();
                robotIOGroups[(int)RobotIOGroup.Type.PWM].robotIOFields.Add(
                    new RobotIOField(
                        name,
                        robotIOGroups[(int)RobotIOGroup.Type.PWM].GetPanel(),
                        (RobotIOField robotIOField) =>
                        {
                            try
                            {
                                if (j < EmulatedRoboRIO.Constants.NUM_PWM_HDRS)
                                {
                                    robotIOField.inputField.text = EmulatedRoboRIO.RobotOutputs.PwmHeaders[j].ToString();
                                }
                                else
                                {
                                    int digital_index = EmulatedRoboRIO.MXPPWMToDigitalIndex(j - (int)EmulatedRoboRIO.Constants.NUM_PWM_HDRS);
                                    if (EmulatedRoboRIO.RobotOutputs.MxpData[digital_index].Config == EmulationService.MXPData.Types.Config.Pwm)
                                    {
                                        robotIOField.inputField.text = EmulatedRoboRIO.RobotOutputs.MxpData[digital_index].Value.ToString();
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
                                if (j >= EmulatedRoboRIO.Constants.NUM_PWM_HDRS)
                                {
                                    robotIOField.SetEnable(false);
                                }
                            }
                            robotIOField.inputField.interactable = false;
                        }
                    )
                );
            }

            for (int i = 0; i < EmulatedRoboRIO.Constants.NUM_CAN_ADDRESSES; i++)
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
                                var can = EmulatedRoboRIO.RobotOutputs.CanMotorControllers.First(e => e.Id == j); // Throws on failure
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
                        robotIOField.gameObject.SetActive(EmulatedRoboRIO.RobotOutputs.CanMotorControllers.Count == 0);
                    }
                )
            );
            for (int i = 0; i < EmulatedRoboRIO.Constants.NUM_DIGITAL_HDRS + EmulatedRoboRIO.Constants.NUM_DIGITAL_MXP; i++)
            {
                int j = i; // Create copy of iterator
                string name = (j < EmulatedRoboRIO.Constants.NUM_DIGITAL_HDRS) ? j.ToString() : "MXP " + (j - EmulatedRoboRIO.Constants.NUM_DIGITAL_HDRS).ToString();
                robotIOGroups[(int)RobotIOGroup.Type.DIO].robotIOFields.Add(
                    new RobotIOField(
                        name,
                        robotIOGroups[(int)RobotIOGroup.Type.DIO].GetPanel(),
                        (RobotIOField robotIOField) =>
                        {
                            try
                            {
                                if (j < EmulatedRoboRIO.Constants.NUM_DIGITAL_HDRS)
                                {
                                    string mode = "";
                                    EmulatedRoboRIO.RobotInputs.DigitalHeaders[j].Config = (EmulatedRoboRIO.RobotOutputs.DigitalHeaders.Count > j) ? EmulatedRoboRIO.RobotOutputs.DigitalHeaders[j].Config : EmulationService.DIOData.Types.Config.Di; // Sync
                                    if (EmulatedRoboRIO.RobotInputs.DigitalHeaders[j].Config == EmulationService.DIOData.Types.Config.Di)
                                    {
                                        try
                                        {
                                            mode = "(DI)";
                                            robotIOField.inputField.interactable = true;
                                            EmulatedRoboRIO.RobotInputs.DigitalHeaders[j].Value = int.Parse(robotIOField.inputField.text) != 0;
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
                                        robotIOField.inputField.text = EmulatedRoboRIO.RobotOutputs.DigitalHeaders[j].Value ? "1" : "0";
                                    }
                                    robotIOField.label.text = j.ToString() + " " + mode;
                                }
                                else
                                {
                                    int mxp_index = j - (int)EmulatedRoboRIO.Constants.NUM_DIGITAL_HDRS;
                                    string mode = "";
                                    EmulatedRoboRIO.RobotInputs.MxpData[mxp_index].Config = (EmulatedRoboRIO.RobotOutputs.MxpData.Count > mxp_index) ? EmulatedRoboRIO.RobotOutputs.MxpData[mxp_index].Config : EmulationService.MXPData.Types.Config.Di; // Sync
                                    if (EmulatedRoboRIO.RobotInputs.MxpData[mxp_index].Config == EmulationService.MXPData.Types.Config.Di)
                                    {
                                        try
                                        {
                                            mode = "(DI)";
                                            robotIOField.SetEnable(true);
                                            robotIOField.inputField.interactable = true;
                                            EmulatedRoboRIO.RobotInputs.MxpData[mxp_index].Value = (int.Parse(robotIOField.inputField.text) != 0) ? 1 : 0;
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
                                        if (EmulatedRoboRIO.RobotOutputs.MxpData[mxp_index].Config == EmulationService.MXPData.Types.Config.Do)
                                        {
                                            robotIOField.SetEnable(true);
                                            mode = "(DO)";
                                            robotIOField.inputField.text = ((int)EmulatedRoboRIO.RobotOutputs.MxpData[mxp_index].Value).ToString();
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
                                if (j >= EmulatedRoboRIO.Constants.NUM_DIGITAL_HDRS)
                                {
                                    robotIOField.SetEnable(false);
                                }
                            }
                        }
                    )
                );
            }
            for (int i = 0; i < EmulatedRoboRIO.Constants.NUM_AI_HDRS + EmulatedRoboRIO.Constants.NUM_AI_MXP + EmulatedRoboRIO.Constants.NUM_AO_MXP; i++)
            {
                int j = i; // Create copy of iterator
                string name = "";
                if (j < EmulatedRoboRIO.Constants.NUM_AI_HDRS)
                {
                    name = "AI " + j.ToString();
                }
                else if(j < EmulatedRoboRIO.Constants.NUM_AI_HDRS+ EmulatedRoboRIO.Constants.NUM_AI_MXP)
                {
                    name = "MXP AI " + (j - EmulatedRoboRIO.Constants.NUM_AI_HDRS).ToString();
                }
                else
                {
                    name = "MXP AO " + (j - EmulatedRoboRIO.Constants.NUM_AI_HDRS - EmulatedRoboRIO.Constants.NUM_AI_MXP).ToString();
                }
                robotIOGroups[(int)RobotIOGroup.Type.AIO].robotIOFields.Add(
                    new RobotIOField(
                        name,
                        robotIOGroups[(int)RobotIOGroup.Type.AIO].GetPanel(),
                        (RobotIOField robotIOField) =>
                        {
                            if (j < EmulatedRoboRIO.Constants.NUM_AI_HDRS + EmulatedRoboRIO.Constants.NUM_AI_MXP) // Analog Input
                            {
                                try
                                {
                                    EmulatedRoboRIO.RobotInputs.AnalogInputs[j] = float.Parse(robotIOField.inputField.text);
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
                            else // Analog Output
                            {
                                try
                                {
                                    int ao_index = (int)(j - EmulatedRoboRIO.Constants.NUM_AI_HDRS - EmulatedRoboRIO.Constants.NUM_AI_MXP);
                                    robotIOField.inputField.text = EmulatedRoboRIO.RobotOutputs.AnalogOutputs[ao_index].ToString();
                                }
                                catch (Exception)
                                {
                                    robotIOField.inputField.text = "0";
                                }
                                robotIOField.inputField.interactable = false;
                            }
                        }
                    )
                );
            }
            for (int i = 0; i < EmulatedRoboRIO.Constants.NUM_RELAYS; i++)
            {
                int j = i; // Create copy of iterator
                robotIOGroups[(int)RobotIOGroup.Type.MISC].robotIOFields.Add(
                    new RobotIOField(
                        "Relay " + j.ToString(),
                        robotIOGroups[(int)RobotIOGroup.Type.MISC].GetPanel(),
                        (RobotIOField robotIOField) =>
                        {
                            try
                            {
                                robotIOField.inputField.characterValidation = InputField.CharacterValidation.Alphanumeric;
                                switch (EmulatedRoboRIO.RobotOutputs.Relays[j])
                                {
                                    case EmulationService.RobotOutputs.Types.RelayState.Off:
                                        robotIOField.inputField.text = "Off";
                                        break;
                                    case EmulationService.RobotOutputs.Types.RelayState.Forward:
                                        robotIOField.inputField.text = "Forward";
                                        break;
                                    case EmulationService.RobotOutputs.Types.RelayState.Reverse:
                                        robotIOField.inputField.text = "Reverse";
                                        break;
                                    case EmulationService.RobotOutputs.Types.RelayState.InvalidState:
                                        robotIOField.inputField.text = "Error";
                                        break;
                                    default:
                                        throw new Exception("Unhandled relay state");
                                }
                            }
                            catch (Exception)
                            {
                                robotIOField.inputField.text = "Off";
                            }
                            robotIOField.inputField.interactable = false;
                        }
                    )
                );
            }
            robotIOGroups[(int)RobotIOGroup.Type.MISC].robotIOFields.Add(
                new RobotIOButton(
                    "User Button",
                    robotIOGroups[(int)RobotIOGroup.Type.MISC].GetPanel(),
                    async (RobotIOButton robotIOButton) =>
                    {
                        EmulatedRoboRIO.RobotInputs.UserButton = true;
                        await Task.Delay(100);
                        robotIOButton.button.interactable = false; // Toggling interactable deselects the button without deslecting any other buttons
                        await Task.Delay(100); // Delay again to ensure its transmitted
                        robotIOButton.button.interactable = true;
                        EmulatedRoboRIO.RobotInputs.UserButton = false;
                    },
                    (RobotIOButton robotIOButton) =>
                    {
                        robotIOButton.buttonText.text = "User Button";
                    }
                ));
        }
    }
}
