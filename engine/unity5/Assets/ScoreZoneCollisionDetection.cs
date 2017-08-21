using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BulletSharp;
using UnityEngine;
using BulletSharp.Math;
using BulletUnity;

public class ScoreZoneCollisionDetection : BCollisionCallbacksDefault
{

    public ScoreZoneActive sza;
    
    public override void BOnCollisionEnter(CollisionObject otherObj, BCollisionCallbacksDefault.PersistentManifoldList mList) {
        
        BRigidBody obj = (BRigidBody) otherObj.UserObject;
        GameObject g = obj.gameObject;
        
        sza.CollisionHandler(g);
        
        Debug.Log("Got collision with " + g);
        
        // We don't do anything if we neither destroy or reinstantiate
    }
}
