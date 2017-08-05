using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BulletUnity;
using BulletSharp;

public class BeamBreaker : SensorBase
{
    public bool IsEmitter;
    public GameObject Emitter;
    public GameObject Receiver;
    private bool setPosition;
    private Vector3 receivePosition;
    private float sensorOffset;
    private void Start()
    {
    }

    private void Update()
    {
        //receivePosition = Receiver.transform.forward; // + new Vector3(0, 0, Receiver.GetComponent<BoxCollider>().size.z/2 - 1);
        Debug.Log(ReturnOutput());
    }

    public override float ReturnOutput()
    {
        //Raycasting begins
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

        float distanceToCollider = sensorOffset;
        foreach (BulletSharp.Math.Vector3 pos in colliderPositions)
        {
            if ((pos - fromUltra).Length < distanceToCollider && !pos.Equals(BulletSharp.Math.Vector3.Zero))
            {
                distanceToCollider = (pos - fromUltra).Length;
                colliderPosition = pos;
            }
        }
        Debug.DrawLine(fromUltra.ToUnity(), colliderPosition.ToUnity(), Color.blue);

        if (distanceToCollider < sensorOffset)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    public void SetSensorOffset(float distance)
    {
        Emitter.transform.localPosition = new Vector3(0, 0, -distance / 2);
        Receiver.transform.localPosition = new Vector3(0, 0, distance / 2);
        sensorOffset = distance;
    }
}

