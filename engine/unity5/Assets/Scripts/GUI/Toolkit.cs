using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.FSM;
using BulletSharp;
using BulletUnity;
using UnityEngine.Analytics;


/// <summary>
/// Helps the user with various helper functions such as stopwatch, and ruler
/// </summary>
public class Toolkit : MonoBehaviour
{

    private bool ignoreClick = true;

    private GameObject canvas;
    private MainState mainState;
    private GameObject toolkitWindow;

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
    private Text stopwatchPauseButtonText;

    private bool stopwatchOn;
    private bool stopwatchPaused;
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

    //dummy robot variables
    private GameObject dummyWindow;
    private DummyScrollable dummyList;
    bool controllingDummy = false;
    private GameObject dummyIndicator;

    // Use this for initialization
    void Start()
    {
        canvas = GameObject.Find("Canvas");
        toolkitWindow = AuxFunctions.FindObject(canvas, "ToolkitPanel");

        //Ruler Objects
        rulerStartPoint = GameObject.Find("RulerStartPoint");
        rulerEndPoint = GameObject.Find("RulerEndPoint");
        rulerWindow = AuxFunctions.FindObject(canvas, "RulerPanel");
        rulerText = AuxFunctions.FindObject(canvas, "RulerText").GetComponent<Text>();
        rulerXText = AuxFunctions.FindObject(canvas, "RulerXAxisText").GetComponent<Text>();
        rulerYText = AuxFunctions.FindObject(canvas, "RulerYAxisText").GetComponent<Text>();
        rulerZText = AuxFunctions.FindObject(canvas, "RulerZAxisText").GetComponent<Text>();

        //Stopwatch Objects
        stopwatchWindow = AuxFunctions.FindObject(canvas, "StopwatchPanel");
        stopwatchText = AuxFunctions.FindObject(canvas, "StopwatchText").GetComponent<Text>();
        stopwatchStartButtonText = AuxFunctions.FindObject(canvas, "StopwatchStartText").GetComponent<Text>();
        stopwatchPauseButtonText = AuxFunctions.FindObject(canvas, "StopwatchPauseText").GetComponent<Text>();

        //Dummy Robot Objects
        dummyWindow = AuxFunctions.FindObject(canvas, "DummyRobotPanel");
        dummyList = AuxFunctions.FindObject(canvas, "DummyList").GetComponent<DummyScrollable>();
        dummyIndicator = GameObject.Find("DummyIndicator");
        dummyIndicator.SetActive(false);

        //Stats Objects
        statsWindow = AuxFunctions.FindObject(canvas, "StatsPanel");
        speedEntry = AuxFunctions.FindObject(statsWindow, "SpeedEntry");
        speedUnit = AuxFunctions.FindObject(speedEntry, "Unit").GetComponent<Text>();
        accelerationEntry = AuxFunctions.FindObject(statsWindow, "AccelerationEntry");
        accelerationUnit = AuxFunctions.FindObject(accelerationEntry, "Unit").GetComponent<Text>();
        angularVelocityEntry = AuxFunctions.FindObject(statsWindow, "AngularVelocityEntry");
        weightEntry = AuxFunctions.FindObject(statsWindow, "WeightEntry");
        weightUnit = AuxFunctions.FindObject(weightEntry, "Unit").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (mainState == null) mainState = ((MainState)StateMachine.Instance.CurrentState);
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

    public void ToggleToolkitWindow(bool show)
    {
        if (show)
        {
            if (SimUI.changeAnalytics)
            {
                Analytics.CustomEvent("Opened Toolkit", new Dictionary<string, object> //for analytics tracking
                {
                });
                toolkitWindow.SetActive(true);
            }
        }
        else
        {
            ToggleRulerWindow(false);
            toolkitWindow.SetActive(false);
        }
    }

    public void ToggleToolkitWindow()
    {
        ToggleToolkitWindow(!toolkitWindow.activeSelf);
    }



    #region Ruler Functions
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

    public void ToggleRulerWindow()
    {
        ToggleRulerWindow(!rulerWindow.activeSelf);
    }

    public void StartRuler()
    {
        usingRuler = true;
        rulerStartPoint.SetActive(true);
        AuxFunctions.FindObject(canvas, "RulerStartButton").SetActive(false);
        AuxFunctions.FindObject(canvas, "RulerTooltipText").SetActive(true);
    }

    private void ClickRuler()
    {
        //Casts a ray from the camera in the direction the mouse is in and returns the closest object hit
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        BulletSharp.Math.Vector3 start = ray.origin.ToBullet();
        BulletSharp.Math.Vector3 end = ray.GetPoint(200).ToBullet();

        //Creates a callback result that will be updated if we do a ray test with it
        ClosestRayResultCallback rayResult = new ClosestRayResultCallback(ref start, ref end);

        //Retrieves the bullet physics world and does a ray test with the given coordinates and updates the callback object
        BPhysicsWorld world = BPhysicsWorld.Get();
        world.world.RayTest(start, end, rayResult);

        if (rayResult.CollisionObject != null)
        {
            if (Input.GetMouseButtonDown(0))
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
            else if (!mainState.IsMetric)
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

    private void DisableRuler()
    {
        ignoreClick = true;
        firstPoint = BulletSharp.Math.Vector3.Zero;
        usingRuler = false;
        rulerStartPoint.GetComponent<LineRenderer>().enabled = false;
        rulerStartPoint.SetActive(false);
        rulerEndPoint.SetActive(false);
        AuxFunctions.FindObject(canvas, "RulerStartButton").SetActive(true);
        AuxFunctions.FindObject(canvas, "RulerTooltipText").SetActive(false);
    }
    #endregion
    #region Stopwatch Functions
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

    public void ToggleStopwatchWindow()
    {
        ToggleStopwatchWindow(!stopwatchWindow.activeSelf);
    }

    public void ToggleStopwatch()
    {
        if (!stopwatchOn)
        {
            stopwatchTime = 0f;
            stopwatchStartButtonText.text = "Stop";
            stopwatchOn = true;
            stopwatchPauseButtonText.text = "Pause";
            stopwatchPaused = false;
        }
        else
        {
            stopwatchStartButtonText.text = "Start";
            stopwatchOn = false;
        }

    }

    public void PauseStopwatch()
    {
        if (stopwatchOn)
        {
            if (!stopwatchPaused)
            {
                stopwatchPauseButtonText.text = "Resume";
                stopwatchPaused = true;
            }
            else
            {
                stopwatchPauseButtonText.text = "Pause";
                stopwatchPaused = false;
            }
        }
    }

    public void ResetStopwatch()
    {
        stopwatchTime = 0f;
        stopwatchText.text = (Mathf.Round(stopwatchTime * 100) / 100).ToString();
    }

    private void UpdateStopwatch()
    {
        if (stopwatchOn && !stopwatchPaused)
        {
            stopwatchTime += Time.deltaTime;
            stopwatchText.text = (Mathf.Round(stopwatchTime * 100) / 100).ToString();
        }
    }


    #endregion
    #region Stats Functions
    public void ToggleStatsWindow(bool show)
    {
        if (show) EndProcesses(true);
        statsOn = show;
        statsWindow.SetActive(show);
    }

    public void ToggleStatsWindow()
    {
        ToggleStatsWindow(!statsWindow.activeSelf);
    }

    /// <summary>
    /// Update the stats window and give it correct units when statsOn
    /// </summary>
    public void UpdateStatsWindow()
    {
        speedEntry.GetComponent<InputField>().text = mainState.activeRobot.Speed.ToString();
        accelerationEntry.GetComponent<InputField>().text = mainState.activeRobot.Acceleration.ToString();
        angularVelocityEntry.GetComponent<InputField>().text = mainState.activeRobot.AngularVelocity.ToString();
        weightEntry.GetComponent<InputField>().text = mainState.activeRobot.Weight.ToString();
        if (mainState.IsMetric)
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
        toolkitWindow.SetActive(toolkitWindowOn);
        ToggleStatsWindow(false);
        ToggleStopwatchWindow(false);
    }
    /*
    #region Dummy Robot

    public void SpawnDummyRobot()
    {
        dummyList.AddDummy();
    }

    public void ControlDummyRobot()
    {
        dummyList.ControlDummy();
        dummyIndicator.SetActive(true);
        controllingDummy = true;
    }

    public void ControlMainRobot()
    {
        mainState.activeRobot = mainState.GetRootNode();
        dummyIndicator.SetActive(false);
        controllingDummy = false;
    }

    public void DeleteDummyRobot()
    {
        dummyList.DeleteDummy();
    }

    private void UpdateControlIndicator()
    {
        if (controllingDummy)
        {
            dummyIndicator.transform.position = ((RigidNode)mainState.activeRobot).MainObject.transform.position + new Vector3(0, 1f) ;
        }
    }

    #endregion*/
}