using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigurableJointTest : MonoBehaviour {
    
    private Rigidbody _rb;
    private ConfigurableJoint _joint;

    public void Start() {
        _joint = GetComponent<ConfigurableJoint>();
        _rb = _joint.gameObject.GetComponent<Rigidbody>();
        _rb.sleepThreshold = 0;
        _joint.connectedBody.sleepThreshold = 0;

        _joint.targetPosition = new Vector3(0, 2, 0);
        _joint.xDrive = new JointDrive { maximumForce = 1000 };
        _joint.yDrive = new JointDrive { maximumForce = 1000 };
        _joint.zDrive = new JointDrive { maximumForce = 1000 };
    }

}
