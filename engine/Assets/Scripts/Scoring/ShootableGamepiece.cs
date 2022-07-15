using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootableGamepiece : MonoBehaviour
{
    
    void Start()
    {
        
    }

    void Update()
    {
        
    }
    
    public bool currentlyHeld = false;

    public void OnShoot(Vector3 Impulse, float Magnitude)
    {
        //configure being shown
        //add impulse
    }
    private void OnCollisionEnter(Collision collision)
    {
        //check if other is robot
        //add self to queue
        //disable self
    }
    public void ResetGamepiece()
    {
        
    }
}
