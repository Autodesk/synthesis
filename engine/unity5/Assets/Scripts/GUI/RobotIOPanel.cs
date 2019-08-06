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
    public class RobotIOPanel : MonoBehaviour
    {
        public class RobotIOField
        {
            public GameObject gameObject;
            public Text label;
            public InputField inputField;
            private Action<RobotIOField> updateFunction;

            private static Color ENABLED_COLOR = Color.white;
            private static Color DISABLED_COLOR = new Color(0.47f, 0.47f, 0.47f);

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

            // Update displayed values
            public void Update()
            {
                updateFunction(this);
            }

            public void SetEnable(bool e)
            {
                label.color = e ? ENABLED_COLOR : DISABLED_COLOR;
                inputField.textComponent.color = e ? ENABLED_COLOR : DISABLED_COLOR;
            }
        }

        public class RobotIOGroup
        {
            public enum Type
            {
                PWM,
                CAN,
                DIO,
                AI,
                AO
            }

            public GameObject gameObject;
            public List<RobotIOField> robotIOFields;

            public GameObject GetPanel()
            {
                return Auxiliary.FindObject(gameObject, "Content");
            }

            public RobotIOGroup(Type type, GameObject parent)
            {
                gameObject = Instantiate(RobotIOPanel.Instance.robotIOGroupPrefab, parent.transform) as GameObject;
                gameObject.name = type.ToString();

                Text title = gameObject.GetComponentInChildren<Text>();
                title.name = type.ToString() + " Title";
                title.text = type.ToString();


                robotIOFields = new List<RobotIOField>();
            }
        }

        public static RobotIOPanel Instance { get; private set; }

        private GameObject canvas;
        private GameObject mainPanel;
        private bool shouldBeActive = false;

        // Robot IO view
        private GameObject displayPanel;
        public GameObject robotIOFieldPrefab;
        public GameObject robotIOGroupPrefab;

        private RobotIOGroup[] robotIOGroups;

        // Mini Camera
        private GameObject miniCameraView;
        private UnityEngine.Camera miniCamera = null;
        float lastSetAspect = 0;
        Rect lastSetPixelRect;

        // Robot Print-Outs
        private GameObject robotPrintPanel;
        private GameObject robotPrintScrollRect;
        private GameObject robotPrintScrollContent;
        private GameObject robotPrintTextContainer;
        public GameObject robotPrintPrefab;

        private GameObject robotPrintFooter;
        private GameObject autoScrollButton;
        public Sprite SelectedButtonImage;
        public Sprite UnselectedButtonImage;

        private StreamReader printReader = null;
        private string robotPrintBuffer = "";
        private bool autoScroll;

        public void Awake()
        {
            Instance = this;

            canvas = GameObject.Find("Canvas");
            mainPanel = Auxiliary.FindObject(canvas, "RobotIOPanel");
            mainPanel.SetActive(false); // Must start inactive, otherwise initializing mini camera will cause problems

            displayPanel = Auxiliary.FindObject(mainPanel, "RobotIODisplayPanel");
            robotIOGroups = new RobotIOGroup[(int)Enum.GetValues(typeof(RobotIOGroup.Type)).Cast<RobotIOGroup.Type>().Max() + 1];

            miniCameraView = Auxiliary.FindObject(mainPanel, "MiniCameraView");

            robotPrintPanel = Auxiliary.FindObject(mainPanel, "RobotPrintPanel");
            robotPrintScrollRect = Auxiliary.FindObject(robotPrintPanel, "RobotPrintScrollRect");
            robotPrintScrollContent = Auxiliary.FindObject(robotPrintScrollRect, "Content");
            robotPrintTextContainer = Auxiliary.FindObject(robotPrintScrollContent, "PrintContainer");
            robotPrintFooter = Auxiliary.FindObject(robotPrintPanel, "RobotPrintFooter");
            autoScrollButton = Auxiliary.FindObject(robotPrintFooter, "RobotPrintAutoScrollButton");

            autoScroll = true;
            autoScrollButton.GetComponent<Image>().sprite = SelectedButtonImage;

            Populate();
        }

        public void Update()
        {
            if (shouldBeActive && InputControl.GetButtonDown(new KeyMapping("Hide Menu", KeyCode.H, Input.Enums.KeyModifier.Ctrl), true))
            {
                mainPanel.SetActive(!mainPanel.activeSelf);
            }
            if (mainPanel.activeSelf) // Update rest of UI
            {
                bool freeze = false;
                foreach (var group in robotIOGroups)
                {
                    foreach (var i in group.robotIOFields)
                    {
                        freeze = freeze || i.inputField.isFocused;
                        i.Update();
                    }
                }
                InputControl.freeze = freeze;

                if (miniCamera == null)
                {
                    InitMiniCamera();
                }
                UpdateMiniCamera();

                for (var i = 0; i < robotIOGroups.Length; i++) // Shift RobotIOFields over when scroll bar activates
                {
                    if (robotIOGroups[i].GetPanel().GetComponent<RectTransform>().sizeDelta.y > robotIOGroups[i].gameObject.GetComponent<RectTransform>().sizeDelta.y)
                        robotIOGroups[i].GetPanel().GetComponent<VerticalLayoutGroup>().padding.right = 20;
                }
            }
            if (EmulatorManager.IsTryingToRunRobotCode()) // Update robot prints
            {
                if (printReader == null) // Start stream if needed
                {
                    Task.Run(OpenPrintStream);
                }

                if (robotPrintBuffer != "")
                {
                    Text a = Instantiate(robotPrintPrefab.GetComponent<Text>(), robotPrintTextContainer.transform); // Add buffer to console
                    a.text = robotPrintBuffer;
                    robotPrintBuffer = ""; // Reset buffer

                    RectTransform rect = robotPrintScrollContent.GetComponent<RectTransform>(); // Resize scroll area
                    rect.sizeDelta = new Vector2(rect.sizeDelta.x, robotPrintTextContainer.GetComponent<RectTransform>().sizeDelta.y);

                    if (autoScroll)
                    {
                        var scrollRect = robotPrintScrollRect.GetComponent<ScrollRect>();
                        scrollRect.verticalNormalizedPosition = 0; // Scroll to bottom
                    }

                    Canvas.ForceUpdateCanvases();
                }
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
                                    if(digital_index >= 4) // First 4 MXP PWM outputs have the right index, but the ones after are offset by 4
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
                        robotIOField.label.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(100, robotIOField.label.gameObject.GetComponent<RectTransform>().sizeDelta.y);
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
                                    if (OutputManager.Instance.DigitalHeaders[j].Config == EmulationService.DIOData.Types.Config.Di) // TODO which manager should control this?
                                    {
                                        try
                                        {
                                            robotIOField.inputField.interactable = true;
                                            robotIOField.label.text = j.ToString() + " (DI)";
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
                                        robotIOField.inputField.interactable = false;
                                        robotIOField.label.text = j.ToString() + " (DO)";
                                        robotIOField.inputField.text = OutputManager.Instance.DigitalHeaders[j].Value ? "1" : "0";
                                    }
                                }
                                else
                                {
                                    int mxp_index = j - (int)RoboRIOConstants.NUM_DIGITAL_HDRS;
                                    string new_label = "MXP " + mxp_index.ToString() + " ";
                                    if (OutputManager.Instance.MxpData[mxp_index].Config == EmulationService.MXPData.Types.Config.Di)
                                    {
                                        try
                                        {
                                            new_label += "(DI)";
                                            robotIOField.inputField.interactable = true;
                                            robotIOField.inputField.text = ((int)InputManager.Instance.MxpData[mxp_index].Value).ToString();
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
                                            new_label += "(DO)";
                                            robotIOField.inputField.text = ((int)OutputManager.Instance.MxpData[mxp_index].Value).ToString();
                                            robotIOField.SetEnable(true);
                                        }
                                        else
                                        {
                                            robotIOField.inputField.text = "0";
                                            robotIOField.SetEnable(false);
                                        }
                                    }
                                    robotIOField.label.text = new_label;
                                }
                            }
                            catch (Exception)
                            {
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
                robotIOGroups[(int)RobotIOGroup.Type.AI].robotIOFields.Add(
                    new RobotIOField(
                        j.ToString(),
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

        /// <summary>
        /// Toggle panel
        /// </summary>
        public void Toggle()
        {
            mainPanel.SetActive(!mainPanel.activeSelf);
            shouldBeActive = mainPanel.activeSelf;
        }

        /// <summary>
        /// Configure the mini camera for use
        /// </summary>
        private void InitMiniCamera()
        {
            try
            {
                miniCamera = Instantiate(UnityEngine.Camera.main, miniCameraView.transform);
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
            // Update size of mini camera to match the camera aspect ratio
            if (lastSetAspect != UnityEngine.Camera.main.aspect || lastSetPixelRect != UnityEngine.Camera.main.pixelRect)
            {
                try
                {
                    var size_delta = miniCameraView.GetComponent<RectTransform>().sizeDelta;

                    // Maintain main camera aspect ratio in mini camera view
                    miniCameraView.GetComponent<RectTransform>().sizeDelta = new Vector2(
                        size_delta.y * UnityEngine.Camera.main.aspect,
                        size_delta.y);

                    size_delta = miniCameraView.GetComponent<RectTransform>().sizeDelta;

                    // Update camera texture size as well
                    if (miniCamera.targetTexture != null)
                        miniCamera.targetTexture.Release();
                    miniCamera.targetTexture = new RenderTexture(new RenderTextureDescriptor(
                            (int)size_delta.x,
                            (int)size_delta.y));
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

            // Update position and orientation
            miniCamera.transform.position = UnityEngine.Camera.main.transform.position;
            miniCamera.transform.rotation = UnityEngine.Camera.main.transform.rotation;
        }

        /// <summary>
        /// Disable user camera control
        /// </summary>
        public void DisableCameraControl()
        {
            DynamicCamera.ControlEnabled = false;
        }

        /// <summary>
        /// Enable user camera control
        /// </summary>
        public void EnableCameraControl()
        {
            DynamicCamera.ControlEnabled = true;
        }

        /// <summary>
        /// Begin reading robot print-outs into the string buffer
        /// </summary>
        private void OpenPrintStream()
        {
            if(printReader == null)
                printReader = EmulatorManager.CreateRobotOutputStream();
            string line;

            while (EmulatorManager.IsTryingToRunRobotCode())
            {
                line = printReader.ReadLine(); // Different read function?
                if (line != null)
                {
                    if (robotPrintBuffer != "")
                        robotPrintBuffer += "\n";
                    robotPrintBuffer += line;
                }
            }
            printReader.Close();
            printReader = null;
            EmulatorManager.CloseRobotOutputStream();
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
            autoScrollButton.GetComponent<Image>().sprite = autoScroll ? SelectedButtonImage : UnselectedButtonImage;
        }

        /// <summary>
        /// Prompt user to download the log file
        /// </summary>
        public void DownloadLog()
        {
            if (EmulationWarnings.CheckRequirement(EmulationWarnings.Requirement.VMConnected))
                EmulatorManager.FetchLogFile();
        }
    }
}
