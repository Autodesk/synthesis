using UnityEngine;
using System.Collections.Generic;

public class AnimateWheels : MonoBehaviour 
{

        // Will contain a list of  wheel colliders (so we can tell them to move) - may or may not be changed depending on how we set up our code interpreters
        private List<WheelCollider> leftSideColliders = new List<WheelCollider> ();

        // Will contain a list of wheel gameObjects (useful for driving) - may or may not be deleted depending on how we set up our drive code interpreters
        private List<GameObject> leftSideWheels = new List<GameObject>();

        // These will hold the names of keyboard keys. Each string will correspond to a function. This will probably change, if we want more than just a basic drive system
        private string forward;
        private string backward;
        private string left;
        private string right;

        // Allows you to assign custom buttons (will most likely be altered in the future)
        void setControls(string f, string b, string l, string r) {
                this.forward = f;
                this.backward = b;
                this.left = l;
                this.right = r;
        }


        /*	This next one is a bit complicated.
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
        public KeyCode GetKeyCodeFormString(string userInput) 
        {
                // Firstly, we are going to grab an array of all of the different KeyCode objects
                System.Array codes = System.Enum.GetValues (typeof(KeyCode));
		
                // Now we will go through each one, and get the name of the object (so when it finds the object for Enter,
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

        // Takes a gameObject as a Parameter and modifies it so that its pivot point is in the center of the object
        void centerPivot(GameObject culprit) {
                /********Based on script from Yilmaz Kiymaz (@VoxelBoy)*******/
                // Quote: "License: Free to use and distribute, in both free and commercial projects. Do not try to sell as your own work. Simply put, play nice "
                // I want to play nice :)
		
                // Interrogating the culprit (getting the data we need from the GameObject)
                Mesh mesh = culprit.GetComponent<MeshFilter>().mesh;
                Vector3[] vertices = mesh.vertices;
                Vector3[] newVertices = mesh.vertices;
                //Coordinates of the center of the object
                Vector3 centerOffset = -1 * mesh.bounds.center;
                MeshCollider meshCollider = culprit.GetComponent<MeshCollider>();
		
                // Calculating how much we need to move the verticies of the wheels so that the pivot point is in the center of the wheeel
                // pivotValue is the distance from the edge of the object to the center (at each axis)
                Vector3 pivotValue = new Vector3 (centerOffset.x/mesh.bounds.extents.x, centerOffset.y/mesh.bounds.extents.y, centerOffset.z/mesh.bounds.extents.z);
                // difference is the amonut that the mesh will need to be moved so that the pivot point is in the middle of the object
                Vector3 difference = Vector3.Scale (mesh.bounds.extents, pivotValue);
		
                // Now, we are going to translate (move) the object so that the pivot is in the center of where the object originaly was (using the difference value we calculated aboive)
                culprit.transform.position -= difference;
		
		
                // Alright, we moved the object, but how does this get us any closer to having a pivot point in the center?
                // It gets us very close actually.
                // All we need to do now is move the verticies of the object (using the difference value we calculated above) so that the object is back to its original position
                // Note that in doing so, the pivot point does not change--so now the pivot point is in the middle of the object :)
                for (int i=0; i< vertices.Length; i++) {
                        newVertices[i] += difference;
                }
                mesh.vertices = newVertices;
		
                // Lastly, we need to recalculate the object's bounds (so they are accurate) and update the mesh collider (if it has one), otherwise you WILL have problems.
                mesh.RecalculateBounds ();
                meshCollider.sharedMesh = mesh;
        }


        /*
        * TODO:
        * 	-Set Up a basic drive system (forward and back for now)
        * 		-In doing so, we will need to grab wheelobject dynamically (using the API)
        * 		-We will need to grab wheel gameObjects in the same way
        * 	-Test
        */

}
