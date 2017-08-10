using System;
using System.Collections.Generic;
using UnityEngine;

class SensorManager : MonoBehaviour
{
    public GameObject Ultrasonic;

    //Lists of sensors
    private List<GameObject> sensorList = new List<GameObject>();
    void Start()
    {
        //Hold a list of prefabs for instantiate later in the game
        Ultrasonic = Resources.Load("Prefabs/UltrasonicSensor") as GameObject;
    }

    /// <summary>
    /// Instantiate an ultrasonic sensor (a distance sensor actually) and set its name, local position, local rotation, and add it to the list
    /// </summary>
    /// <param name="parent"></param> the parent node to which the sensor is attached
    /// <param name="position"></param> local position of the sensor
    /// <param name="rotation"></param> local rotation of the sensor
    public void AddUltrasonicSensor(GameObject parent, Vector3 position, Vector3 rotation)
    {
        GameObject ultrasonic = GameObject.Instantiate(Ultrasonic, parent.transform);
        ultrasonic.transform.localPosition = position;
        ultrasonic.transform.localRotation = Quaternion.Euler(rotation);
        ultrasonic.name = "Ultrasonic_" + sensorList.Count;
        sensorList.Add(ultrasonic);
    }
}
