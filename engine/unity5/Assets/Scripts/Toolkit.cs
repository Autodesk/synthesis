using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BulletSharp;
using BulletUnity;

/// <summary>
/// Helps the user with various helper functions such as stopwatch, and ruler
/// </summary>
public class Toolkit : MonoBehaviour
{

    private bool ignoreClick = true;

    private bool usingRuler;
    private BulletSharp.Math.Vector3 firstPoint = BulletSharp.Math.Vector3.Zero;

    private GameObject canvas;

    private GameObject toolkitWindow;

    private GameObject rulerWindow;
    private GameObject rulerStartPoint;
    private GameObject rulerEndPoint;
    private Text rulerText;
    private Text rulerXText;
    private Text rulerYText;
    private Text rulerZText;

    // Use this for initialization
    void Start()
    {
        canvas = GameObject.Find("Canvas");
        toolkitWindow = AuxFunctions.FindObject(canvas,"ToolkitPanel");

        //Ruler Objects
        rulerStartPoint = GameObject.Find("RulerStartPoint");
        rulerEndPoint = GameObject.Find("RulerEndPoint");
        rulerWindow = AuxFunctions.FindObject(canvas, "RulerPanel");
        rulerText = AuxFunctions.FindObject(canvas, "RulerText").GetComponent<Text>();
        rulerXText = AuxFunctions.FindObject(canvas, "RulerXAxisText").GetComponent<Text>();
        rulerYText = AuxFunctions.FindObject(canvas, "RulerYAxisText").GetComponent<Text>();
        rulerZText = AuxFunctions.FindObject(canvas, "RulerZAxisText").GetComponent<Text>();

    }

    // Update is called once per frame
    void Update()
    {
        if (usingRuler)
        {
            if (ignoreClick) ignoreClick = false;
            else ClickRuler();
        }
    }

    public void ToggleToolkitWindow(bool show)
    {
        if (show)
        {
            toolkitWindow.SetActive(true);
        }
        else
        {
            ToggleRulerWindow(false);
            toolkitWindow.SetActive(false);
        }
    }



    #region Ruler Functions
    public void ToggleRulerWindow(bool show)
    {
        if (show) rulerWindow.SetActive(true);
        else
        {
            rulerWindow.SetActive(false);
            DisableRuler();
        }
    }

    public void BeginRuler()
    {
        usingRuler = true;
        rulerStartPoint.SetActive(true);
        AuxFunctions.FindObject(canvas, "RulerBeginButton").SetActive(false);
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
            else
            {
                rulerText.text = BulletSharp.Math.Vector3.Distance(firstPoint, rayResult.HitPointWorld) * 3.28084f + "ft";
                rulerXText.text = Mathf.Abs(firstPoint.X - rayResult.HitPointWorld.X) * 3.28084f + "ft";
                rulerYText.text = Mathf.Abs(firstPoint.Y - rayResult.HitPointWorld.Y) * 3.28084f + "ft";
                rulerZText.text = Mathf.Abs(firstPoint.Z - rayResult.HitPointWorld.Z) * 3.28084f + "ft";
                rulerEndPoint.transform.position = rayResult.HitPointWorld.ToUnity();
                rulerStartPoint.GetComponent<LineRenderer>().SetPosition(1, rulerEndPoint.transform.position);
            }
        }
    }

    public void DisableRuler()
    {
        ignoreClick = true;
        firstPoint = BulletSharp.Math.Vector3.Zero;
        usingRuler = false;
        rulerStartPoint.GetComponent<LineRenderer>().enabled = false;
        rulerStartPoint.SetActive(false);
        rulerEndPoint.SetActive(false);
        AuxFunctions.FindObject(canvas, "RulerBeginButton").SetActive(true);
        AuxFunctions.FindObject(canvas, "RulerTooltipText").SetActive(false);
    }
    #endregion
}