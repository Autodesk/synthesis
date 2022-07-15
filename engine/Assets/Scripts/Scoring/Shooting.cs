using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Shooting
{
    public static float timeBetweenShots = 0.5f;
    public static float currentTime;
    public static float lastShotTime;

    public static Vector3 shotLocation;

    public static Queue<GameObject> shootingQueue = new Queue<GameObject>();
    

    public static void ConfigureGamepieces()
    {
        //loop through all gamepieces and attach objects
        //call this from the mode manager
    }
    
    public static void Update()
    {
        //call this from the mode manager update loop
        //update the current time, detect input, and perform shooting actions
    }

    public static void AddGamepiece()
    {
        //add gamepiece to shooting queue
    }
    
    public static void Reset()
    {
        //clear gamepieces from queue and reset each one
    }
}
