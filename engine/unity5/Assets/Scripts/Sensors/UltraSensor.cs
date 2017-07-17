using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BulletUnity;
using BulletSharp;

/// <summary>
/// Ultrasonic sensor class, must be attached to a node gameobject
/// </summary>
public class UltraSensor : SensorBase
{

    private float ultraAngle; //angle in degrees of the sensor's range
    private float maxRange; //maximmum range of the sensor
    private UnityEngine.Vector3 offset = Vector3.zero; //offset from node in world coordinates
    private UnityEngine.Vector3 rotation = Vector3.forward; //rotation difference from the node rotation

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
        Debug.DrawRay(transform.position + offset, transform.rotation * Vector3.forward + rotation, Color.green);

    }

    //Step #2
    public override float ReturnOutput()
    {
        //this is the radius of the ultrasonic cone; how wide its range is
        float radius = maxRange * (float)Math.Tan((double)ultraAngle / 2);

        //setting shortest distance of a collider to the maxRange, then if any colliders are closer to the sensor, 
        //their distanceToCollider value becomes the new shortest distance
        float shortestDistance = maxRange;

        //looping to cover all directions of ultrasonic cone by creating levels of circles 
        for (int i = 0; i <= radius / 2; i++)
        {
            for (int j = 0; j <= ultraAngle / 2; j++)
            {
                //since ultraVec has been assigned to the x-axis, there are vectors in the y and z directions to create circles
                Vector3 yVec = new Vector3(0f, (float)(i * Math.Sin(j)), 0f);
                Vector3 zVec = new Vector3(0f, 0f, (float)(i * Math.Cos(j)));

                //Vector3 toCollider = (transform.position + yVec + zVec);
                //BulletSharp.Math.Vector3 fromUltra = transform.position.ToBullet();
                //BulletSharp.Math.Vector3 toCollider = (transform.position + yVec + zVec).ToBullet();

                //Raycasting begins
                Ray ray = new Ray(transform.forward, transform.forward);
                BulletSharp.Math.Vector3 fromUltra = ray.origin.ToBullet();
                BulletSharp.Math.Vector3 toCollider = ray.GetPoint(maxRange).ToBullet();

                //BulletSharp.Math.Vector3 fromUltra = transform.position.ToBullet();
                //BulletSharp.Math.Vector3 toCollider = (transform.position + yVec + zVec).ToBullet();

                Vector3 toColliderUnity = toCollider.ToUnity();

                //Callback returns the closest result only
                ClosestRayResultCallback rayCallback = new ClosestRayResultCallback(ref fromUltra, ref toCollider);

                //Retrieves bullet physics world and does a ray test with the given coordinates and updates the callback object
                BPhysicsWorld world = BPhysicsWorld.Get();
                world.world.RayTest(fromUltra, toCollider, rayCallback);

                BulletSharp.Math.Vector3 colliderPosition = rayCallback.HitPointWorld;

                //Magnitude (or length) of difference between start and end vectors is the distance between them
                float distanceToCollider = ((transform.position + offset).ToBullet() - colliderPosition).Length;

                if (distanceToCollider < shortestDistance)
                {
                    distanceToCollider = shortestDistance;
                }
            }
        }

        if (shortestDistance == maxRange)
        {
            //read out to user that nothing within range was detected by ultrasonic sensor;
            Debug.Log("False");
        }
        else
        {
            //read out to user that first object detected was 'shortestDistance' away;
            Debug.Log("True");
        }

        return shortestDistance;
    }
}