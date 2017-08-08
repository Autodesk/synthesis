using System;
using System.Collections.Generic;
using UnityEngine;
using BulletSharp;
using BulletUnity;

class SensorManager : MonoBehaviour
{
    public GameObject Ultrasonic;
    public GameObject BeamBreaker;
    public GameObject Gyro;

    //Lists of sensors
    private List<GameObject> ultrasonicList = new List<GameObject>();
    private List<GameObject> beamBreakerList = new List<GameObject>();
    private List<GameObject> gyroList = new List<GameObject>();

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
    }

    private void Update()
    {
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
    /// Instantiate an ultrasonic sensor (a distance sensor actually) and set its name, local position, local rotation, and add it to the list
    /// </summary>
    /// <param name="parent"></param> the parent node to which the sensor is attached
    /// <param name="position"></param> local position of the sensor
    /// <param name="rotation"></param> local rotation of the sensor
    public SensorBase AddUltrasonicSensor(GameObject parent, Vector3 position, Vector3 rotation)
    {
        GameObject ultrasonic = GameObject.Instantiate(Ultrasonic, parent.transform);
        ultrasonic.transform.localPosition = position;
        ultrasonic.transform.localRotation = Quaternion.Euler(rotation);
        ultrasonic.name = "Ultrasonic_" + ultrasonicList.Count;
        ultrasonicList.Add(ultrasonic);
        return ultrasonic.GetComponent<UltraSensor>();
    }

    public SensorBase AddBeamBreaker(GameObject parent, Vector3 position, Vector3 rotation, float distance = 0)
    {
        GameObject beamBreaker = GameObject.Instantiate(BeamBreaker, parent.transform);
        beamBreaker.transform.localPosition = position;
        beamBreaker.transform.localRotation = Quaternion.Euler(rotation);
        beamBreaker.name = "BeamBreaker_" + beamBreakerList.Count;
        beamBreakerList.Add(beamBreaker);

        BeamBreaker sensor = beamBreaker.GetComponent<BeamBreaker>();
        sensor.SetSensorRange(distance);
        return beamBreaker.GetComponent<BeamBreaker>();
    }

    public void AddGyroSensor(GameObject parent, Vector3 position, Vector3 rotation)
    {
        GameObject gyro = GameObject.Instantiate(Gyro, parent.transform);
        gyro.transform.localPosition = position;
        gyro.transform.localRotation = Quaternion.Euler(rotation);
        gyro.name = "Gyro_" + gyroList.Count;
        ultrasonicList.Add(gyro);
    }

    public void DefineNode()
    {
        SelectingNode = true;
    }

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

    public void ClearSelectedNode()
    {
        SelectedNode = null;
    }

    public void ClearSelectedSensor()
    {
        SelectedSensor = null;
    }
}
