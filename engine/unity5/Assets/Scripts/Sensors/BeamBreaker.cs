using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BulletUnity;
using BulletSharp;
using UnityEngine.UI;

/// <summary>
/// Gyroscope sensor class, must be attached to a prefab gameobject
/// </summary>
public class BeamBreaker : SensorBase
{
    public bool IsEmitter;
    public GameObject Emitter;
    public GameObject Receiver;
    private float sensorOffset;
    private bool isChangingOffset;
    
    private void FixedUpdate()
    {
        UpdateOutputDisplay();
        float output = ReturnOutput();

        //Debug.Log(ReturnOutput());
    }

    public override float ReturnOutput()
    {
        //Raycasting begins, draw a ray from emitter to the receiver
        Ray ray = new Ray(Emitter.transform.position, Emitter.transform.forward);
        BulletSharp.Math.Vector3 fromUltra = ray.origin.ToBullet();
        BulletSharp.Math.Vector3 toCollider = ray.GetPoint(10).ToBullet();

        Vector3 toColliderUnity = toCollider.ToUnity();

        //Callback returns all hit point results in order to avoid non colliders interfere with the ray test
        AllHitsRayResultCallback raysCallback = new AllHitsRayResultCallback(fromUltra, toCollider);

        //Retrieves bullet physics world and does a ray test with the given coordinates and updates the callback object
        BPhysicsWorld world = BPhysicsWorld.Get();
        world.world.RayTest(fromUltra, toCollider, raysCallback);
        List<BulletSharp.Math.Vector3> colliderPositions = raysCallback.HitPointWorld;
        BulletSharp.Math.Vector3 colliderPosition = BulletSharp.Math.Vector3.Zero;

        //Set the initial distance as the distance between emitter and receiver
        float distanceToCollider = sensorOffset;
        //Loop through all hitpoints (exclude the origin), if there is at least one hitpoint less than the distance between two sensors, 
        //something should block the beam between emitter and receiver
        foreach (BulletSharp.Math.Vector3 pos in colliderPositions)
        {
            if ((pos - fromUltra).Length < distanceToCollider && !pos.Equals(BulletSharp.Math.Vector3.Zero))
            {
                distanceToCollider = (pos - fromUltra).Length;
                colliderPosition = pos;
            }
        }
        //Again if the line connects to the middle of the field nothing is blocking the beam
        Debug.DrawLine(fromUltra.ToUnity(), colliderPosition.ToUnity(), Color.blue);

        if (distanceToCollider < sensorOffset)
        {
            //Something is there
            return 1;
        }
        else
        {
            //Nothing in between
            return 0;
        }
    }

    public override float GetSensorRange()
    {
        return sensorOffset;
    }

    public override void SetSensorRange(float distance)
    {
        Emitter.transform.localPosition = new Vector3(0, 0, -distance / 2);
        Receiver.transform.localPosition = new Vector3(0, 0, distance / 2);
        sensorOffset = distance;
    }

    public override void UpdateRangeTransform()
    {
        //Lower the transform speed
        sensorOffset += Input.GetAxis("CameraVertical") * 0.2f;
        SetSensorRange(sensorOffset);
    }

    public override void UpdateOutputDisplay()
    {
        base.UpdateOutputDisplay();
        GameObject outputPanel = GameObject.Find(gameObject.name + "_Panel");
        if (outputPanel != null)
        {
            GameObject outputText = AuxFunctions.FindObject(outputPanel, "Text");
            outputText.GetComponent<Text>().text = gameObject.name + " Output (1 closed)";
        }
    }
}

