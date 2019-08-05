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
            public GameObject robotIOField;
            public Text label;
            public InputField inputField;
            private Action<Text, InputField> updateFunction;

            public RobotIOField(string name, GameObject parent, Action<Text, InputField> update)
            {
                robotIOField = Instantiate(RobotIOPanel.Instance.robotIOFieldPrefab, parent.transform) as GameObject;
                robotIOField.name = name;

                label = robotIOField.GetComponentInChildren<Text>();
                label.text = name;

                inputField = robotIOField.GetComponentInChildren<InputField>();

                updateFunction = update;
                Update();
            }

            // Update displayed values
            public void Update()
            {
                updateFunction(label, inputField);
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

            for (int i = 0; i < OutputManager.NUM_PWM_HDRS + OutputManager.NUM_PWM_MXP; i++)
            {
                int j = i; // Create copy of iterator
                robotIOGroups[(int)RobotIOGroup.Type.PWM].robotIOFields.Add(
                    new RobotIOField(
                        j.ToString(),
                        robotIOGroups[(int)RobotIOGroup.Type.PWM].GetPanel(),
                        (Text label, InputField inputField) =>
                        {
                            try
                            {
                                inputField.text = OutputManager.Instance.PwmHeaders[j].ToString();
                            }
                            catch (Exception)
                            {
                                inputField.text = "0";
                            }
                            inputField.interactable = false;
                        }
                    )
                );
            }

            for (int i = 0; i < 63; i++)
            {
                int j = i; // Create copy of iterator
                robotIOGroups[(int)RobotIOGroup.Type.CAN].robotIOFields.Add(
                    new RobotIOField(
                        j.ToString(),
                        robotIOGroups[(int)RobotIOGroup.Type.CAN].GetPanel(),
                        (Text label, InputField inputField) =>
                        {
                            try
                            {
                                label.text = j.ToString();
                                //label.text = OutputManager.Instance.CanMotorControllers[j].Id.ToString();
                                inputField.text = OutputManager.Instance.CanMotorControllers[j].PercentOutput.ToString();
                            }
                            catch (Exception)
                            {
                                //label.text = "0";
                                inputField.text = "0";
                            }
                            inputField.interactable = false;
                        }
                    )
                );
            }
            for (int i = 0; i < OutputManager.NUM_DIO_HDRS + OutputManager.NUM_DIO_MXP; i++)
            {
                int j = i; // Create copy of iterator
                string name = (j < OutputManager.NUM_DIO_HDRS) ? j.ToString() : "MXP " + (j - OutputManager.NUM_DIO_HDRS).ToString();
                robotIOGroups[(int)RobotIOGroup.Type.DIO].robotIOFields.Add(
                    new RobotIOField(
                        name,
                        robotIOGroups[(int)RobotIOGroup.Type.DIO].GetPanel(),
                        (Text label, InputField inputField) =>
                        {
                            try
                            {
                                if (j < OutputManager.NUM_DIO_HDRS)
                                {
                                    // label.text = ""; // TODO
                                    inputField.text = OutputManager.Instance.DigitalHeaders[j].ToString();
                                    inputField.interactable = false; // TODO
                                }
                                else
                                {
                                    // label.text = ""; // TODO
                                    inputField.text = ((int)OutputManager.Instance.MxpData[j].Value).ToString(); // TODO
                                    inputField.interactable = false; // TODO
                                }
                            }
                            catch (Exception)
                            {
                                inputField.text = "0";
                            }
                        }
                    )
                );
            }
            for (int i = 0; i < InputManager.NUM_AI_HDRS + InputManager.NUM_AI_MXP; i++)
            {
                int j = i; // Create copy of iterator
                robotIOGroups[(int)RobotIOGroup.Type.AI].robotIOFields.Add(
                    new RobotIOField(
                        j.ToString(),
                        robotIOGroups[(int)RobotIOGroup.Type.AI].GetPanel(),
                        (Text label, InputField inputField) =>
                        {
                            try
                            {
                                inputField.text = "0";
                                //inputField.text = InputManager.Instance.AnalogInputs[j].ToString(); // TODO
                            }
                            catch (Exception)
                            {
                                inputField.text = "0";
                            }
                            inputField.interactable = true;
                        }
                    )
                );
            }
            for (int i = 0; i < OutputManager.NUM_AO_MXP; i++)
            {
                int j = i; // Create copy of iterator
                robotIOGroups[(int)RobotIOGroup.Type.AO].robotIOFields.Add(
                    new RobotIOField(
                        j.ToString(),
                        robotIOGroups[(int)RobotIOGroup.Type.AO].GetPanel(),
                        (Text label, InputField inputField) =>
                        {
                            try
                            {
                                inputField.text = OutputManager.Instance.AnalogOutputs[j].ToString();
                            }
                            catch (Exception)
                            {
                                inputField.text = "0";
                            }
                            inputField.interactable = false;
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
