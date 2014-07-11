using UnityEngine;
using System.Collections.Generic;

public class Controls : MonoBehaviour 
{
        // For four wheel drive - might be outdated
        private static KeyCode forward;
        private static KeyCode backward;
        private static KeyCode left;
        private static KeyCode right;

        /*  This is a bit complicated.
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
        public static KeyCode GetKeyCodeFormString(string userInput) 
        {
                // Firstly, we are going to grab an array of all of the different KeyCode objects
                System.Array codes = System.Enum.GetValues (typeof(KeyCode));
                
                // for example, it will create a string "Enter").
                // It then compares it to the input the user typed in.
                foreach (KeyCode code in codes) {
                        // If the name of the object matches the user string, it will return the KeyCode object that we need.
                        if (System.Enum.GetName (typeof(KeyCode), code).Equals (userInput)) 
                        {
                                return code;
                        }
                }
                return KeyCode.None;
        }


        // Allows you to set custom buttons, but sadly, this is not dynamic yet, and it based on a basic forward, back, left, and right principal.
        public static void setControls(string vForward, string vBackward, string vLeft, string vRight) 
        {
                forward = GetKeyCodeFormString(vForward);
                backward = GetKeyCodeFormString(vBackward);
                left = GetKeyCodeFormString(vLeft);
                right = GetKeyCodeFormString(vRight);

                // Will be used to make sure that all of the variables are assigned correctly
                Dictionary<string, KeyCode> temp = new Dictionary<string, KeyCode>(){{"forward", forward}, {"backward", backward}, {"left", left}, {"right", right}};

                // If any of the keys are not assigned correctly (that is, they have no set value), it print an error message to the console
                foreach (KeyValuePair<string, KeyCode> key in temp) {
                        if (key.Value == KeyCode.None) {
                                Debug.Log ("Error, Key "  + key.Key + " was not set correctly");
                        }
                }
        }

        // Combs through all of the skeleton data and finds the pwm port numbers that each wheelCollider in the skeleton is attatched to. It is returned in the form of a dictionary with an integer as its key, and a List of wheelColliders as its value
        // So you can reference each motor easily using syntax like this: wheelDictionary[1][0].motorTorque = 5 (or something along those lines)
        public static Dictionary<int, List<WheelCollider>> assignMotors(RigidNode_Base skeleton) 
        {
                // This dictionary will hold all of the data we gather from the skeleton and store it
                Dictionary<int, List<WheelCollider>> functionOutput = new Dictionary<int, List<WheelCollider>>();

                // We will need this to grab and comb through all of the nodes (oarts) or the skeleton
                List<RigidNode_Base> listAllNodes = new List<RigidNode_Base> ();
                skeleton.ListAllNodes (listAllNodes);

                foreach (RigidNode_Base dropTheBase in listAllNodes) 
                {
                        // Unity rigid nodes have the functions we need (it inherits from RigidNodeBase so we can typecast it)
                        UnityRigidNode unityNode = (UnityRigidNode)dropTheBase;
                       
                        // If it finds a wheelCollider
                        if (unityNode.GetWheelCollider() != null) 
                        {
                                // As I hear on the internet: "It is easier to ask for forgiveness than it is to ask for permisson" (A.K.A: try-catch = more efficient than if-else)
                                try 
                                {
                                        // It will assume that there is already an entry for the pwm port of the wheelCollider and try to add the wheel collider to the list of wheelColliders in the dictionary entry
                                        functionOutput[unityNode.GetPortA()].Add(unityNode.GetWheelCollider());
                                }
                                catch 
                                {
                                        // If it does not work, it will create an entry for the pwm port and then add the wheelCollider to the list in the dictionary entry
                                        functionOutput.Add(unityNode.GetPortA(), new List<WheelCollider>());
                                        functionOutput[unityNode.GetPortA()].Add(unityNode.GetWheelCollider());
                                }
                        }
                }

                // Finally, it is done
                return functionOutput;
        }


}