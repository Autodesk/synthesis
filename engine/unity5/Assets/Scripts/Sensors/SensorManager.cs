using System;
using System.Collections.Generic;
using UnityEngine;
using BulletSharp;
using BulletUnity;
using Assets.Scripts.FSM;

class SensorManager : MonoBehaviour
{
    public GameObject Ultrasonic;
    public GameObject BeamBreaker;
    public GameObject Gyro;
    public GameObject OutputPanel;
    public MainState main;

    //Lists of sensors
    private List<GameObject> activeSensorList = new List<GameObject>();
    private List<GameObject> inactiveSensorList = new List<GameObject>();
    private List<GameObject> sensorList = new List<GameObject>();

    public bool SelectingNode { get; set; }
    public GameObject SelectedNode { get; private set; }
    public bool SelectingSensor { get; set; }
    public GameObject SelectedSensor { get; private set; }
    void Start()
    {
        //Hold a list of prefabs for instantiate later in the game
        Ultrasonic = Resources.Load("Prefabs/UltrasonicSensor") as GameObject;
        BeamBreaker = Resources.Load("Prefabs/BeamBreaker") as GameObject;
        Gyro = Resources.Load("Prefabs/Gyro") as GameObject;
        OutputPanel = Resources.Load("Prefabs/SensorOutput") as GameObject;
    }

    private void Update()
    {
        if(main == null)
        {
            main = GameObject.Find("StateMachine").GetComponent<StateMachine>().CurrentState as MainState;
        }
        //Handle the state where the user is selecting a node for attachment or selecting a sensor to configure
        if (SelectingNode)
        {
            SetNode();
        }
        else if (SelectingSensor)
        {
            SetSensor();
        }
    }

    /// <summary>
    /// Instantiate an ultrasonic sensor (a distance sensor actually) and set its name, local position, local rotation, and add it to 
    /// both sensor list and active sensor list
    /// </summary>
    /// <param name="parent"></param> the parent node to which the sensor is attached
    /// <param name="position"></param> local position of the sensor
    /// <param name="rotation"></param> local rotation of the sensor
    public SensorBase AddUltrasonic(GameObject parent, Vector3 position, Vector3 rotation)
    {
        GameObject ultrasonic = GameObject.Instantiate(Ultrasonic, parent.transform);
        ultrasonic.transform.localPosition = position;
        ultrasonic.transform.localRotation = Quaternion.Euler(rotation);
        ultrasonic.name = "Ultrasonic_" + sensorList.Count;
        ultrasonic.GetComponent<SensorBase>().Robot = main.activeRobot;
        sensorList.Add(ultrasonic);
        activeSensorList.Add(ultrasonic);
        return ultrasonic.GetComponent<UltraSensor>();
    }

    /// <summary>
    /// Instantiate an beam breaker sensor and set its name, local position, local rotation, and add it to 
    /// both sensor list and active sensor list
    /// </summary>
    /// <param name="parent"></param> the parent node to which the sensor is attached
    /// <param name="position"></param> local position of the sensor
    /// <param name="rotation"></param> local rotation of the sensor
    /// <param name="distance"></param> the distance offset between the emitter and receiver
    public SensorBase AddBeamBreaker(GameObject parent, Vector3 position, Vector3 rotation, float distance = 0)
    {
        GameObject beamBreaker = GameObject.Instantiate(BeamBreaker, parent.transform);
        beamBreaker.transform.localPosition = position;
        beamBreaker.transform.localRotation = Quaternion.Euler(rotation);
        beamBreaker.name = "BeamBreaker_" + sensorList.Count;
        beamBreaker.GetComponent<SensorBase>().Robot = main.activeRobot;
        sensorList.Add(beamBreaker);
        activeSensorList.Add(beamBreaker);
        BeamBreaker sensor = beamBreaker.GetComponent<BeamBreaker>();
        sensor.SetSensorRange(distance);
        return sensor;
    }

    /// <summary>
    /// Instantiate an gyro sensor (measure angular rotation rate) and set its name, local position, local rotation, and add it to 
    /// both sensor list and active sensor list
    /// </summary>
    /// <param name="parent"></param> the parent node to which the sensor is attached
    /// <param name="position"></param> local position of the sensor
    /// <param name="rotation"></param> local rotation of the sensor
    public SensorBase AddGyro(GameObject parent, Vector3 position, Vector3 rotation)
    {
        GameObject gyro = GameObject.Instantiate(Gyro, parent.transform);
        gyro.transform.localPosition = position;
        gyro.transform.localRotation = Quaternion.Euler(rotation);
        gyro.name = "Gyro_" + sensorList.Count;
        gyro.GetComponent<SensorBase>().Robot = main.activeRobot;
        sensorList.Add(gyro);
        activeSensorList.Add(gyro);

        Gyro sensor = gyro.GetComponent<Gyro>();
        return sensor;
    }
    
    /// <summary>
    /// Start the state of selecting node for attachment
    /// </summary>
    public void DefineNode()
    {
        SelectingNode = true;
    }

    /// <summary>
    /// When user click left mouse, use raycast to select a node for attachment
    /// </summary>
    public void SetNode()
    {
        if (Input.GetMouseButtonDown(0))
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

            Debug.Log("Selected:" + rayResult.CollisionObject);
            //If there is a collision object and it is a robot part, set that to be new attachment point
            if (rayResult.CollisionObject != null)
            {
                GameObject selectedObject = ((BRigidBody)rayResult.CollisionObject.UserObject).gameObject;
                if (selectedObject.transform.parent != null && selectedObject.transform.parent.name == "Robot")
                {
                    string name = selectedObject.name;

                    SelectedNode = selectedObject;

                    UserMessageManager.Dispatch(name + " has been selected as the node for sensor attachment", 5);
                }
                else
                {
                    UserMessageManager.Dispatch("Please select a robot node", 3);
                }
            }
        }
    }

    /// <summary>
    /// When user click left mouse, use raycast to select a sensor for configuration
    /// </summary>
    public void SetSensor()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hitInfo = new RaycastHit();
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out hitInfo);
            if(hitInfo.transform != null && hitInfo.transform.gameObject.tag == "Sensor")
            {
                SelectedSensor = hitInfo.transform.gameObject;
                UserMessageManager.Dispatch(SelectedSensor.name + " has been selected as the current sensor", 5);

            }
            else
            {
                UserMessageManager.Dispatch("Please select a sensor!", 3f);
            }
        }
    }

    /// <summary>
    /// Set SelectedNode to null
    /// </summary>
    public void ClearSelectedNode()
    {
        SelectedNode = null;
    }

    /// <summary>
    /// Set SelectedSensor to null
    /// </summary>
    public void ClearSelectedSensor()
    {
        SelectedSensor = null;
    }

    /// <summary>
    /// Get the sensor index of the given sensor from active sensor list (excluding those that got deleted)
    /// </summary>
    /// <param name="sensor"></param> the sensor that you need to look up index for
    /// <returns></returns> the index of given sensor
    public int GetSensorIndex(GameObject sensor)
    {
        return activeSensorList.IndexOf(sensor);
    }

    /// <summary>
    /// Remove the given sensor from the active sensor list
    /// </summary>
    /// <param name="sensor"></param> the sensor that you want to remove from the active sensor list
    public void RemoveSensor(GameObject sensor)
    {
        activeSensorList.Remove(sensor);
        inactiveSensorList.Add(sensor);
    }

    /// <summary>
    /// Get a list of sensors attached to the given robot
    /// </summary>
    /// <param name="robot"></param> The robot where sensors are attached to
    /// <returns></returns> A list of sensors attached to the robot
    public List<GameObject> GetSensorsFromRobot(Robot robot)
    {
        List<GameObject> sensorsOnRobot = new List<GameObject>();
        foreach(GameObject sensor in activeSensorList)
        {
            if (sensor.GetComponent<SensorBase>().Robot.Equals(robot))
            {
                sensorsOnRobot.Add(sensor);
            }
        }
        return sensorsOnRobot;
    }


    /// <summary>
    /// Remove all sensors attached to the given robot and destroy them, reset all lists
    /// </summary>
    /// <param name="robot"></param> The robot where sensors are attached to
    public void RemoveSensorsFromRobot(Robot robot)
    {
        List<GameObject> sensorsOnRobot = GetSensorsFromRobot(robot);
        foreach(GameObject removingSensors in sensorsOnRobot)
        {
            if (activeSensorList.Contains(removingSensors))
            {
                activeSensorList.Remove(removingSensors);
                removingSensors.transform.parent = null;
                Destroy(removingSensors.gameObject);
                inactiveSensorList.Add(removingSensors);
            }
        }
    }

    /// <summary>
    /// Clear all sensor lists (sensor, active, inactive). Used when a robot is initialized. Hopefully no one use this in multiplayer :)
    /// </summary>
    public void ResetSensorLists()
    {
        activeSensorList.Clear();
        sensorList.Clear();
        inactiveSensorList.Clear();
    }

    public List<GameObject> GetActiveSensors()
    {
        return activeSensorList;
    }

    public List<GameObject> GetInactiveSensors()
    {
        return inactiveSensorList;
    }
}
