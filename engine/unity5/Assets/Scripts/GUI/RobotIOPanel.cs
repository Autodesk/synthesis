using Synthesis.Input;
using Synthesis.Utils;
using System;
using System.Collections.Generic;
using System.IO;
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
            public RectTransform body;
            public Text label;
            public InputField inputField;
            private Action<InputField> updateFunction;

            public RobotIOField(string name, Vector2 relative_position, GameObject parent, Action<InputField> update)
            {
                robotIOField = Instantiate(RobotIOPanel.Instance.robotIOFieldPrefab, parent.transform) as GameObject;
                robotIOField.name = name;

                body = robotIOField.GetComponent<RectTransform>();
                body.anchoredPosition = relative_position;
                body.localScale = new Vector3(1, 1, 1);

                label = robotIOField.GetComponentInChildren<Text>();
                label.text = name;

                inputField = robotIOField.GetComponentInChildren<InputField>();

                updateFunction = update;
                Update();
            }

            // Update displayed values
            public void Update()
            {
                updateFunction(inputField);
            }
        }

        public static RobotIOPanel Instance { get; private set; }

        private GameObject canvas;
        private GameObject mainPanel;

        // Robot IO view
        private GameObject displayPanel;
        public GameObject robotIOFieldPrefab;
        private List<RobotIOField> robotIOFields;


        private GameObject robotIOFooter;

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
        public GameObject robotPrintFooter;

        private StreamReader printReader = null;
        private string robotPrintBuffer = "";
        private bool autoScroll = true;

        public void Awake()
        {
            Instance = this;

            canvas = GameObject.Find("Canvas");
            mainPanel = Auxiliary.FindObject(canvas, "RobotIOPanel");
            mainPanel.SetActive(false); // Must start inactive, otherwise initializing mini camera will cause problems

            displayPanel = Auxiliary.FindObject(mainPanel, "RobotIODisplayPanel");
            robotIOFooter = Auxiliary.FindObject(mainPanel, "RobotIOFooter");
            robotIOFields = new List<RobotIOField>();

            miniCameraView = Auxiliary.FindObject(mainPanel, "MiniCameraView");

            robotPrintPanel = Auxiliary.FindObject(mainPanel, "RobotPrintPanel");
            robotPrintScrollRect = Auxiliary.FindObject(robotPrintPanel, "RobotPrintScrollRect");
            robotPrintScrollContent = Auxiliary.FindObject(robotPrintScrollRect, "Content");
            robotPrintTextContainer = Auxiliary.FindObject(robotPrintScrollContent, "PrintContainer");
            robotPrintFooter = Auxiliary.FindObject(robotPrintPanel, "RobotPrintFooter");

            Populate();
        }


        public void Update()
        {
            if (mainPanel.activeSelf)
            {
                bool freeze = false;
                foreach (var i in robotIOFields)
                {
                    freeze = freeze || i.inputField.isFocused;
                    i.Update();
                }
                InputControl.freeze = freeze;

                if (miniCamera == null)
                {
                    InitMiniCamera();
                }
                UpdateMiniCamera();
            }
            if (EmulatorManager.IsTryingToRunRobotCode())
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
            Vector2 initialPos = new Vector2(robotIOFieldPrefab.GetComponent<RectTransform>().rect.width / 2, -robotIOFieldPrefab.GetComponent<RectTransform>().rect.height / 2);
            for (int i = 0; i < OutputManager.NUM_PWM_HDRS; i++)
            {
                int j = i; // Create copy of iterator
                robotIOFields.Add(
                    new RobotIOField(
                        "PWM " + j,
                        new Vector2(initialPos.x, initialPos.y - j * 25),
                        displayPanel,
                        (InputField inputField) =>
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
                // Maintain main camera aspect ratio in mini camera view
                miniCameraView.GetComponent<RectTransform>().sizeDelta = new Vector2(
                    miniCameraView.GetComponent<RectTransform>().sizeDelta.y * UnityEngine.Camera.main.aspect,
                    miniCameraView.GetComponent<RectTransform>().sizeDelta.y);

                // Update camera texture size as well
                if (miniCamera.targetTexture != null)
                    miniCamera.targetTexture.Release();
                miniCamera.targetTexture = new RenderTexture(new RenderTextureDescriptor(
                        (int)miniCameraView.GetComponent<RectTransform>().sizeDelta.x,
                        (int)miniCameraView.GetComponent<RectTransform>().sizeDelta.y));
                miniCameraView.GetComponent<RawImage>().texture = miniCamera.targetTexture;

                // Fill remaining width of screen with the robot print panel
                robotPrintPanel.GetComponent<LayoutElement>().preferredWidth = (UnityEngine.Camera.main.pixelRect.width - miniCameraView.GetComponent<RectTransform>().sizeDelta.x) / miniCameraView.GetComponent<RectTransform>().sizeDelta.x;
                robotPrintScrollRect.GetComponent<LayoutElement>().preferredHeight = robotPrintPanel.GetComponent<RectTransform>().sizeDelta.y - robotPrintFooter.GetComponent<RectTransform>().sizeDelta.y;

                lastSetAspect = UnityEngine.Camera.main.aspect;
                lastSetPixelRect = UnityEngine.Camera.main.pixelRect;
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
        }

        /// <summary>
        /// Prompt user to download the log file
        /// </summary>
        public void DownloadLog()
        {
            if(EmulatorManager.IsVMConnected())
                EmulatorManager.FetchLogFile();
        }
    }
}
