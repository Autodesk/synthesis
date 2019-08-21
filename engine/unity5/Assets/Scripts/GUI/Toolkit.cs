using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Synthesis.FSM;
using BulletSharp;
using BulletUnity;
using Synthesis.Sensors;
using Synthesis.States;
using Synthesis.Utils;

namespace Synthesis.GUI
{
    /// <summary>
    /// Helps the user with various helper functions such as stopwatch, and ruler
    /// </summary>
    public class Toolkit : LinkedMonoBehaviour<MainState>
    {
        private bool ignoreClick = true;

        private GameObject canvas;
        private SensorManagerGUI sensorManagerGUI;

        //ruler variables
        private GameObject rulerWindow;
        private GameObject rulerStartPoint;
        private GameObject rulerEndPoint;
        private Text rulerText;
        private Text rulerXText;
        private Text rulerYText;
        private Text rulerZText;

        private bool usingRuler;
        private BulletSharp.Math.Vector3 firstPoint = BulletSharp.Math.Vector3.Zero;

        //stopwatch variables
        private GameObject stopwatchWindow;
        private Text stopwatchText;
        private Text stopwatchStartButtonText;

        private bool stopwatchOn;
        private float stopwatchTime;

        private GameObject statsWindow;
        private GameObject speedEntry;
        private GameObject accelerationEntry;
        private GameObject angularVelocityEntry;
        private GameObject weightEntry;
        private Text speedUnit;
        private Text accelerationUnit;
        private Text weightUnit;
        private bool statsOn;

        // Use this for initialization
        private void Start()
        {
            canvas = GameObject.Find("Canvas");
            sensorManagerGUI = GetComponent<SensorManagerGUI>();

            //Ruler Objects
            rulerStartPoint = GameObject.Find("RulerStartPoint");
            rulerEndPoint = GameObject.Find("RulerEndPoint");
            rulerWindow = Auxiliary.FindObject(canvas, "RulerPanel");
            rulerText = Auxiliary.FindObject(canvas, "RulerText").GetComponent<Text>();
            rulerXText = Auxiliary.FindObject(canvas, "RulerXAxisText").GetComponent<Text>();
            rulerYText = Auxiliary.FindObject(canvas, "RulerYAxisText").GetComponent<Text>();
            rulerZText = Auxiliary.FindObject(canvas, "RulerZAxisText").GetComponent<Text>();

            //Stopwatch Objects
            stopwatchWindow = Auxiliary.FindObject(canvas, "StopwatchPanel");
            stopwatchText = Auxiliary.FindObject(canvas, "StopwatchText").GetComponent<Text>();
            stopwatchStartButtonText = Auxiliary.FindObject(canvas, "StopwatchStartText").GetComponent<Text>();

            //Stats Objects
            statsWindow = Auxiliary.FindObject(canvas, "StatsPanel");
            speedEntry = Auxiliary.FindObject(statsWindow, "SpeedEntry");
            speedUnit = Auxiliary.FindObject(speedEntry, "Unit").GetComponent<Text>();
            accelerationEntry = Auxiliary.FindObject(statsWindow, "AccelerationEntry");
            accelerationUnit = Auxiliary.FindObject(accelerationEntry, "Unit").GetComponent<Text>();
            angularVelocityEntry = Auxiliary.FindObject(statsWindow, "AngularVelocityEntry");
            weightEntry = Auxiliary.FindObject(statsWindow, "WeightEntry");
            weightUnit = Auxiliary.FindObject(weightEntry, "Unit").GetComponent<Text>();
        }

        // Update is called once per frame
        void Update()
        {
            if (usingRuler)
            {
                if (ignoreClick) ignoreClick = false;
                else ClickRuler();
            }
            UpdateStopwatch();

            if (statsOn)
            {
                UpdateStatsWindow();
            }
            //UpdateControlIndicator();
        }

        #region Ruler Functions
        /// <summary>
        /// Toggle the ruler window on/off according to the show boolean passed in
        /// </summary>
        /// <param name="show"></param>
        public void ToggleRulerWindow(bool show)
        {
            if (show)
            {
                EndProcesses(true);
                rulerWindow.SetActive(true);
            }
            else
            {
                rulerWindow.SetActive(false);
                DisableRuler();
            }
        }

        /// <summary>
        /// Toggle the ruler window on/off according to its current state
        /// </summary>
        public void ToggleRulerWindow()
        {
            ToggleRulerWindow(!rulerWindow.activeSelf);
        }

        /// <summary>
        /// Initiate ruler
        /// </summary>
        /// <summary>
        /// Initiate ruler
        /// </summary>
        public void StartRuler()
        {
            usingRuler = true;
            rulerStartPoint.SetActive(true);
            Auxiliary.FindObject(canvas, "RulerPanelExtension").SetActive(false);
        }

        /// <summary>
        /// Handles the ruler animation and display values
        /// </summary>
        private void ClickRuler()
        {
            //Casts a ray from the camera in the direction the mouse is in and returns the closest object hit
            Ray ray = UnityEngine.Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
            BulletSharp.Math.Vector3 start = ray.origin.ToBullet();
            BulletSharp.Math.Vector3 end = ray.GetPoint(200).ToBullet();

            //Creates a callback result that will be updated if we do a ray test with it
            ClosestRayResultCallback rayResult = new ClosestRayResultCallback(ref start, ref end);

            //Retrieves the bullet physics world and does a ray test with the given coordinates and updates the callback object
            BPhysicsWorld world = BPhysicsWorld.Get();
            world.world.RayTest(start, end, rayResult);

            if (rayResult.CollisionObject != null)
            {
                if (UnityEngine.Input.GetMouseButtonDown(0))
                {
                    if (firstPoint == BulletSharp.Math.Vector3.Zero)
                    {
                        rulerStartPoint.GetComponent<LineRenderer>().enabled = true;
                        rulerStartPoint.GetComponent<LineRenderer>().SetPosition(0, rulerStartPoint.transform.position);
                        rulerEndPoint.SetActive(true);
                        firstPoint = rayResult.HitPointWorld;
                    }
                    else
                    {
                        DisableRuler();
                    }
                }

                if (firstPoint != null) Debug.DrawRay(firstPoint.ToUnity(), Vector3.up);
                if (firstPoint == BulletSharp.Math.Vector3.Zero)
                {
                    rulerStartPoint.transform.position = rayResult.HitPointWorld.ToUnity();
                }
                //Display different values based on the measure system it's currently using
                else if (!State.IsMetric)
                {
                    rulerText.text = Mathf.Round(BulletSharp.Math.Vector3.Distance(firstPoint, rayResult.HitPointWorld) * 328.084f) / 100 + "ft";
                    rulerXText.text = Mathf.Round(Mathf.Abs(firstPoint.X - rayResult.HitPointWorld.X) * 328.084f) / 100 + "ft";
                    rulerYText.text = Mathf.Round(Mathf.Abs(firstPoint.Y - rayResult.HitPointWorld.Y) * 328.084f) / 100 + "ft";
                    rulerZText.text = Mathf.Round(Mathf.Abs(firstPoint.Z - rayResult.HitPointWorld.Z) * 328.084f) / 100 + "ft";
                    rulerEndPoint.transform.position = rayResult.HitPointWorld.ToUnity();
                    rulerStartPoint.GetComponent<LineRenderer>().SetPosition(1, rulerEndPoint.transform.position);
                }
                else
                {
                    rulerText.text = Mathf.Round(BulletSharp.Math.Vector3.Distance(firstPoint, rayResult.HitPointWorld) * 100f) / 100 + "m";
                    rulerXText.text = Mathf.Round(Mathf.Abs(firstPoint.X - rayResult.HitPointWorld.X) * 1000f) / 1000 + "m";
                    rulerYText.text = Mathf.Round(Mathf.Abs(firstPoint.X - rayResult.HitPointWorld.X) * 1000f) / 1000 + "m";
                    rulerZText.text = Mathf.Round(Mathf.Abs(firstPoint.X - rayResult.HitPointWorld.X) * 1000f) / 1000 + "m";
                    rulerEndPoint.transform.position = rayResult.HitPointWorld.ToUnity();
                    rulerStartPoint.GetComponent<LineRenderer>().SetPosition(1, rulerEndPoint.transform.position);
                }
            }
        }

        /// <summary>
        /// Disable the ruler
        /// </summary>
        private void DisableRuler()
        {
            ignoreClick = true;
            firstPoint = BulletSharp.Math.Vector3.Zero;
            usingRuler = false;
            rulerStartPoint.GetComponent<LineRenderer>().enabled = false;
            rulerStartPoint.SetActive(false);
            rulerEndPoint.SetActive(false);
            Auxiliary.FindObject(canvas, "RulerPanelExtension").SetActive(true);
        }
        #endregion
        #region Stopwatch Functions
        /// <summary>
        /// Toggle the stopwatch window on/off according to the show boolean passed in
        /// </summary>
        /// <param name="show"></param>
        public void ToggleStopwatchWindow(bool show)
        {
            if (show)
            {
                EndProcesses(true);
                stopwatchWindow.SetActive(true);
            }
            else
            {
                stopwatchWindow.SetActive(false);
            }
        }

        /// <summary>
        /// Toggle the stopwatch window on/off according to its current state
        /// </summary>
        public void ToggleStopwatchWindow()
        {
            ToggleStopwatchWindow(!stopwatchWindow.activeSelf);
        }

        /// <summary>
        /// Start/stop the stopwatch
        /// </summary>
        public void ToggleStopwatch()
        {
            if (!stopwatchOn)
            {
                stopwatchTime = 0f;
                stopwatchStartButtonText.text = "Stop";
                stopwatchOn = true;
            }

            else
            {
                stopwatchStartButtonText.text = "Start";
                stopwatchOn = false;
            }
        }

        public void ResetStopwatch()
        {
            stopwatchTime = 0f;
            stopwatchText.text = (Mathf.Round(stopwatchTime * 100) / 100).ToString();
        }

        private void UpdateStopwatch()
        {
            if (stopwatchOn)
            {
                stopwatchTime += Time.deltaTime;
                stopwatchText.text = (Mathf.Round(stopwatchTime * 100) / 100).ToString();
            }
        }


        #endregion
        #region Stats Functions
        /// <summary>
        /// Toggle the tookit window on/off according to the show boolean passed in
        /// </summary>
        /// <param name="show"></param>
        public void ToggleStatsWindow(bool show)
        {
            if (show) EndProcesses(true);
            statsOn = show;
            statsWindow.SetActive(show);
        }

        /// <summary>
        /// Toggle the toolkit window on/off according to its current state
        /// </summary>
        public void ToggleStatsWindow()
        {
            ToggleStatsWindow(!statsWindow.activeSelf);
        }

        /// <summary>
        /// Update the stats window and give it correct units when statsOn
        /// </summary>
        public void UpdateStatsWindow()
        {
            speedEntry.GetComponent<InputField>().text = State.ActiveRobot.Speed.ToString();
            accelerationEntry.GetComponent<InputField>().text = State.ActiveRobot.Acceleration.ToString();
            angularVelocityEntry.GetComponent<InputField>().text = State.ActiveRobot.AngularVelocity.ToString();
            weightEntry.GetComponent<InputField>().text = State.ActiveRobot.Weight.ToString();
            //Use correct units depending on the measure system used
            if (State.IsMetric)
            {
                speedUnit.text = "m/s";
                accelerationUnit.text = "m/s^2";
                weightUnit.text = "kg";
            }
            else
            {
                speedUnit.text = "ft/s";
                accelerationUnit.text = "ft/s^2";
                weightUnit.text = "lbs";
            }
        }
        #endregion

        public void EndProcesses(bool toolkitWindowOn = false)
        {
            ToggleRulerWindow(false);

            //Reset the stopwatch stuff
            if (stopwatchOn)
            {
                ResetStopwatch();
                ToggleStopwatch();
            }

            ToggleStatsWindow(false);
            ToggleStopwatchWindow(false);
            sensorManagerGUI.EndProcesses();
        }

    }
}