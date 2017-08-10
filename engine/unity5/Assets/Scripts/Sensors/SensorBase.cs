using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BulletSharp;
using BulletUnity;
using UnityEngine.UI;

/// <summary>
/// This is the template/parent class for all sensors within Synthesis.
/// </summary>
public abstract class SensorBase : MonoBehaviour
{

    public bool IsChangingPosition { get; set; }
    public bool IsChangingHeight { get; set; }
    public bool IsChangingAngle { get; set; }
    public bool IsChangingRange { get; set; }
    private static float positionSpeed = 0.5f;
    private static float rotationSpeed = 25;

    
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }

    public abstract float ReturnOutput();

    public void UpdateTransform()
    {
        if (IsChangingPosition)
        {
            if (IsChangingRange) //Control range transform
            {
                UpdateRangeTransform();
            }
            else if (IsChangingAngle) //Control rotation (only when the angle panel is active)
            {
                UpdateAngleTransform();
            }
            else if (!IsChangingHeight) //Control horizontal plane transform
            {
                UpdateHorizontalPlaneTransform();
            }
            else //Control height transform
            {
                UpdateHeightTransform();
            }

        }
    }

    /// <summary>
    /// Return the range of sensor
    /// </summary>
    /// <returns></returns> range of sensor
    public virtual float GetSensorRange()
    {
        return 0;
    }

    /// <summary>
    /// Set the range of sensor
    /// </summary>
    /// <param name="range"></param>
    public virtual void SetSensorRange(float range)
    {

    }

    public virtual void UpdateAngleTransform()
    {
        transform.Rotate(new Vector3(-Input.GetAxis("CameraVertical") * rotationSpeed, Input.GetAxis("CameraHorizontal") * rotationSpeed, 0) * Time.deltaTime);
    }
    public virtual void UpdateHeightTransform()
    {
        transform.Translate(new Vector3(0, Input.GetAxis("CameraVertical") * positionSpeed, 0) * Time.deltaTime);
    }
    public virtual void UpdateHorizontalPlaneTransform()
    {
        transform.Translate(new Vector3(Input.GetAxis("CameraHorizontal") * positionSpeed, 0, Input.GetAxis("CameraVertical") * positionSpeed) * Time.deltaTime);
    }
    public virtual void UpdateRangeTransform()
    {
        
    }

    /// <summary>
    /// Update the sensor output at the corresponding sensorOutputPanel
    /// </summary>
    public virtual void UpdateOutputDisplay()
    {
        GameObject outputPanel = GameObject.Find(gameObject.name + "_Panel");
        if (outputPanel != null)
        {
            GameObject inputField = AuxFunctions.FindObject(outputPanel, "Entry");
            inputField.GetComponent<InputField>().text = ReturnOutput().ToString();
        }
    }

    /// <summary>
    /// Only used for gyro right now
    /// </summary>
    public virtual void ResetSensorReading()
    {

    }
}
