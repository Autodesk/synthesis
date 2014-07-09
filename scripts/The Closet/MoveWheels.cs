using UnityEngine;
using System.Collections.Generic;

public class MoveWheels : MonoBehaviour {

	// The Game Object that contains the car
	GameObject car;

	// Creating a Dictionary that stores the HingeJoint objects for the wheels
	private Dictionary<string, HingeJoint> HingeJointDictionary;
	private Dictionary<string, JointMotor> MotorDictionary;

	// Creating the motors for each hingeJoint
	private HingeJoint hTopLeft;
	private HingeJoint hTopRight;
	private HingeJoint hBottomLeft;
	private HingeJoint hBottomRight;

	// These will hold the names of keyboard keys. Each string will correspond to a function.
	private string forward;
	private string backward;
	private string left;
	private string right;

	//Speed Constants
	float motorForce = 100;
	float motorTargetVelocity = 2000;
	bool motorFreeSpin = false;
	
	// Allows you to set what buttons you want to be forward, backward, etc.
	void setControls(string f, string b, string l, string r) {
		this.forward = f;
		this.backward = b;
		this.left = l;
		this.right = r;
	}

	// Allows you to easily set motor controls without having to use multiple statements
	JointMotor setMotor(float force, float targetVelocity, bool freeSpin) {
		JointMotor motor = new JointMotor ();
		motor.force = force;
		motor.targetVelocity = targetVelocity;
		motor.freeSpin = freeSpin;

		return motor;
	}

	void setAllMotors(float force, float targetVelocity, bool freeSpin) {
		hTopLeft.motor = setMotor(force, targetVelocity, freeSpin);
		hTopRight.motor = setMotor(force, targetVelocity, freeSpin);
		hBottomLeft.motor = setMotor(force, targetVelocity, freeSpin);
		hBottomRight.motor = setMotor(force, targetVelocity, freeSpin);
	}


	/*	This next one might be a bit complicated.
		In order for Unity to recognize input, the user has to press a keyboard button that corresponds to a KeyCode
		This actually makes it harder for the user to select custom keys.
		For example, what if you want to set the F key to forward?
			In order for that to happen, you will need to use: Input.GetKey(KeyCode.F)
				(Sadly, you can't use Input.GetKey("F") without first setting it up in the input manager first,
				which would require user input and a GUI because you can't access the input manager via script)
			We have to use KeyCode.GetKey(KeyCode.F).
			Simple enought right?
			No.
			You can't put Input.GetKey(KeyCode."F") (or whatever key the user wants to use), 
			because KeyCode.F is its own object (all compatible keyboard commands are).
			This is where the following function comes in.

			Credit for this one goes to Westin Miller (Double check with him if I get this explanation wrong)
	*/
	public KeyCode GetKeyCodeFormString(string userInput) {
		// Firstly, we are going to grab an array of all of the different KeyCode objects
		System.Array codes = System.Enum.GetValues (typeof(KeyCode));
		
		// Now we will go through each one, and get the name of the object (so when it finds the object for Enter,
		// for example, it will create a string "Enter").
		// It then compares it to the input the user typed in.
		foreach (KeyCode code in codes) {
			// If the name of the object matches the user string, it will return the KeyCode object that we need.
			if (System.Enum.GetName (typeof(KeyCode), code).Equals (userInput)) {
				return code;
			}
		}
		return KeyCode.None;
	}
	
	// Use this for initialization
	void Start () {
		car = GameObject.Find ("Car");
		//setControls ("UpArrow", "DownArrow", "LeftArrow", "RightArrow");
		setControls ("W", "S", "A", "D");
		//setControls ("T", "G", "F", "H");
		HingeJointDictionary = new Dictionary<string, HingeJoint> ();
		MotorDictionary  = new Dictionary<string, JointMotor> ();

		Component[] listOfHingeJoints;
		listOfHingeJoints = gameObject.GetComponentsInChildren<HingeJoint>();
		foreach (HingeJoint joint in listOfHingeJoints) {
			// There must be a better way to do this
			switch (joint.name)
			{
			case "TopLeft":
				hTopLeft = joint;
				break;
			case "TopRight":
				hTopRight = joint;
				break;
			case "BottomLeft":
				hBottomLeft = joint;
				break;
			case "BottomRight":
				hBottomRight = joint;
				break;
			}
		}

		foreach (KeyValuePair<string, HingeJoint> entry in HingeJointDictionary) {
			Debug.Log(entry.Key);
		}
		// Lets get all of our HingeJoints
		// Now, we need to find the HingeJoints and their motors. This does not work right now, we will have to figure out how to get it working
		/*hTopLeft = gameObject.GetComponent("TopLeft") as HingeJoint;
		hTopRight = gameObject.GetComponent("TopRight") as HingeJoint;
		hBottomLeft = gameObject.GetComponent("BottomLeft") as HingeJoint;
		hBottomRight = gameObject.GetComponent("BottomRight") as HingeJoint;

		mTopLeft = hTopLeft.motor;
		mTopRight = hTopLeft.motor;
		mBottomLeft = hTopLeft.motor;
		mBottomRight = hTopLeft.motor;*/
	}

	// Update is called once per frame
	void Update () {
		// I'm using a try statement in case things go south
		try {
			if (Input.anyKey) {
				// N00B Code (Basic, unomptimized, messy)

				if (Input.GetKey(GetKeyCodeFormString(forward))) {
					setAllMotors (motorForce, motorTargetVelocity, motorFreeSpin);
				}

				if (Input.GetKey(GetKeyCodeFormString(backward))) {
					setAllMotors (motorForce, -motorTargetVelocity, motorFreeSpin);
				}
				if (Input.GetKey(GetKeyCodeFormString(left))) {
					hTopRight.motor = setMotor(motorForce, motorTargetVelocity, motorFreeSpin);
					hBottomRight.motor = setMotor(motorForce, motorTargetVelocity, motorFreeSpin);
					hTopLeft.motor = setMotor(motorForce, -motorTargetVelocity, motorFreeSpin);
					hBottomLeft.motor = setMotor (motorForce, -motorTargetVelocity, motorFreeSpin);
				}
				if (Input.GetKey(GetKeyCodeFormString(right))) {
					hTopRight.motor = setMotor(motorForce, -motorTargetVelocity, motorFreeSpin);
					hBottomRight.motor = setMotor(motorForce, -motorTargetVelocity, motorFreeSpin);
					hTopLeft.motor = setMotor(motorForce, motorTargetVelocity, motorFreeSpin);
					hBottomLeft.motor = setMotor (motorForce, motorTargetVelocity, motorFreeSpin);
				}
			}
			else {
				setAllMotors (500, 0, false);
			}
		} catch {
		}

	}
}
