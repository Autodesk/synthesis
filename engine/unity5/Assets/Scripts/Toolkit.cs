using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BulletSharp;
using BulletUnity;

/// <summary>
/// Helps the user with various helper functions such as stopwatch, and ruler
/// </summary>
public class Toolkit : MonoBehaviour {

    private bool ignoreClick = true;

    private bool usingRuler;
    private BulletSharp.Math.Vector3 firstPoint = BulletSharp.Math.Vector3.Zero;
    private BulletSharp.Math.Vector3 secondPoint;

    private GameObject rulerStartPoint;
    private GameObject rulerEndPoint;

	// Use this for initialization
	void Start () {
        rulerStartPoint = GameObject.Find("RulerStartPoint");
        rulerEndPoint = GameObject.Find("RulerEndPoint");
	}
	
	// Update is called once per frame
	void Update () {
        if (usingRuler)
        {
            if (ignoreClick) ignoreClick = false;
            else ClickRuler();
        }
	}

    public void BeginRuler()
    {
        usingRuler = true;
        rulerStartPoint.SetActive(true);
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
                    secondPoint = rayResult.HitPointWorld;
                    EndRuler();
                }
            }
            
            if (firstPoint != null) Debug.DrawRay(firstPoint.ToUnity(), Vector3.up);
            if (firstPoint == BulletSharp.Math.Vector3.Zero)
            {
                rulerStartPoint.transform.position = rayResult.HitPointWorld.ToUnity();
            }
            else
            {
                rulerEndPoint.transform.position = rayResult.HitPointWorld.ToUnity();
                rulerStartPoint.GetComponent<LineRenderer>().SetPosition(1, rulerEndPoint.transform.position);
            }
        }
    }

    private void EndRuler()
    {
        ignoreClick = true;
        usingRuler = false;
        float distance = BulletSharp.Math.Vector3.Distance(firstPoint, secondPoint) * 3.28084f;
        UserMessageManager.Dispatch("Distance is: " + distance + " feet.", 10f);
        firstPoint = BulletSharp.Math.Vector3.Zero;
        rulerStartPoint.GetComponent<LineRenderer>().enabled = false;
        rulerStartPoint.SetActive(false);
        rulerEndPoint.SetActive(false);
    }

    public bool IsBusy()
    {
        if (usingRuler) return false;
        else return true;
    }

    
}