using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BulletSharp;
using BulletUnity;

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

    public virtual void UpdateTransform()
    {
        if (IsChangingPosition)
        {

            if (IsChangingAngle) //Control rotation (only when the angle panel is active)
            {
                transform.Rotate(new Vector3(-Input.GetAxis("CameraVertical") * rotationSpeed, Input.GetAxis("CameraHorizontal") * rotationSpeed, 0) * Time.deltaTime);
            }
            else if (!IsChangingHeight) //Control horizontal plane transform
            {
                transform.Translate(new Vector3(Input.GetAxis("CameraHorizontal") * positionSpeed, 0, Input.GetAxis("CameraVertical") * positionSpeed) * Time.deltaTime);
            }
            else //Control height transform
            {
                transform.Translate(new Vector3(0, Input.GetAxis("CameraVertical") * positionSpeed, 0) * Time.deltaTime);
            }

        }
    }

    public virtual float GetSensorRange()
    {
        return 0;
    }

    public virtual void SetSensorRange(float range)
    {

    }

    
}
