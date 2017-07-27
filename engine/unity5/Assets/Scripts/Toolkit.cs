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
    private BulletSharp.Math.Vector3 firstPoint;
    private BulletSharp.Math.Vector3 secondPoint;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0) && usingRuler)
        {
            if (ignoreClick) ignoreClick = false;
            else ClickRuler();
        }

        if (usingRuler && firstPoint != null)
        {
            Debug.DrawLine(firstPoint.ToUnity(), Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }   
	}

    public void BeginRuler()
    {
        usingRuler = true;
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

        if (rayResult.HasHit)
        {
            GameObject indicator = new GameObject("point");
            indicator.transform.position = rayResult.HitPointWorld.ToUnity();
            if (firstPoint == null) firstPoint = rayResult.HitPointWorld;
            else
            {
                secondPoint = rayResult.HitPointWorld;
                EndRuler();
            }
        }
    }

    private void EndRuler()
    {
        ignoreClick = true;
        usingRuler = false;
        float distance = BulletSharp.Math.Vector3.Distance(firstPoint, secondPoint) * 3.28084f;
        UserMessageManager.Dispatch("Distance is: " + distance + " feet.", 10f);
    }

    
}