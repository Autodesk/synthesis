using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BulletUnity;
using BulletSharp;
using Assets.Scripts.FSM;

/// <summary>
/// Ultrasonic sensor class, must be attached to a node gameobject
/// </summary>
public class UltraSensor : SensorBase
{

    public float MaxRange; //maximmum range of the sensor
    private UnityEngine.Vector3 offset = Vector3.zero; //offset from node in world coordinates
    private UnityEngine.Vector3 rotation = Vector3.forward; //rotation difference from the node rotation
    private bool isChangingRange;

    //Initialization
    void Start()
    {
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        /* <summary>
         * Right now there are not actual values coming from the exporter, so I will define fixed values:
         * 
         *  -multiple ray casts in cone shape coming from sensor
         *  -return length of shortest raycast
         * 
         * 3. This distance will be the output of the sensor, and should read out to the screen?
         * 
         * 4. Feedback to emulation. 
         *
         */

        //var as the type (lambda function)
        //these variables can only live INSIDE a function. 
        Debug.Log(ReturnOutput());
    }

    //Step #2
    public override float ReturnOutput()
    {
        //setting shortest distance of a collider to the maxRange, then if any colliders are closer to the sensor, 
        //their distanceToCollider value becomes the new shortest distance
        float shortestDistance = MaxRange;

        //Raycasting begins
        Ray ray = new Ray(gameObject.transform.position, transform.forward);
        BulletSharp.Math.Vector3 fromUltra = ray.origin.ToBullet();
        BulletSharp.Math.Vector3 toCollider = ray.GetPoint(MaxRange).ToBullet();
        
        Vector3 toColliderUnity = toCollider.ToUnity();

        //Callback returns all hit point results in order to avoid non colliders interfere with the ray test
        AllHitsRayResultCallback raysCallback = new AllHitsRayResultCallback(fromUltra, toCollider);

        //Retrieves bullet physics world and does a ray test with the given coordinates and updates the callback object
        BPhysicsWorld world = BPhysicsWorld.Get();
        world.world.RayTest(fromUltra, toCollider, raysCallback);

        //Gets the position of all hit points of the ray test
        List<BulletSharp.Math.Vector3> colliderPositions = raysCallback.HitPointWorld;
        BulletSharp.Math.Vector3 colliderPosition = BulletSharp.Math.Vector3.Zero;

        float distanceToCollider = MaxRange;
        //Loop through all hit points and get the shortest distance, exclude the origin since it is also counted as a hit point
        foreach (BulletSharp.Math.Vector3 pos in colliderPositions)
        {
            if ((pos - fromUltra).Length <= MaxRange && (pos - fromUltra).Length < distanceToCollider && !pos.Equals(BulletSharp.Math.Vector3.Zero))
            {
                distanceToCollider = (pos - fromUltra).Length;
                colliderPosition = pos;
            }
        }

        Debug.DrawLine(fromUltra.ToUnity(), colliderPosition.ToUnity(), Color.green, 5f);

        if (distanceToCollider < shortestDistance)
        {
            shortestDistance = distanceToCollider;
        }

        return shortestDistance;
    }

    public override void SetSensorRange(float range)
    {
        MaxRange = range;
    }

    public override void UpdateTransform()
    {
        if (IsChangingPosition)
        {
            if (isChangingRange)
            {
                MaxRange += Input.GetAxis("CameraVertical");
            }
        }
        base.UpdateTransform();
    }

    public override float GetSensorRange()
    {
        return MaxRange;
    }
}