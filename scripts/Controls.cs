using UnityEngine;
using System.Collections.Generic;
using ErrorHandling;

public class Controls : MonoBehaviour
{
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
		public static KeyCode GetKeyCodeFormString (string userInput)
		{
				// Firstly, we are going to grab an array of all of the different KeyCode objects
				System.Array codes = System.Enum.GetValues (typeof(KeyCode));
                
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

}