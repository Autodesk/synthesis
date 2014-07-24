using UnityEngine;
using System.Collections.Generic;
using System;

// I would like to use a tuple, but .NET 3.5 does not have any... Lets make one
// Some explanation will follow (because I did not know what this was, and writing things down helps me remember)
// The Tuple class is a template class, and the two variables (Item2 and Item2) can be set to any variable upon creation
// Note that X and Y are types. When you create this class in particular, a variable, Item1, is set to the type X (which your provide when you create an instance of the class). Item2 works in the same way, except that it is assigned to the type you specify in Y
// For example, if i create a instance of the Tuple class, Tuple<int, string>, Item1 (which is assigned the type of X) wil be declared as a public integer. Likewise, Item2 will be declared as a public string.
// You can set them to use any type of variable in this way (though I am using to make a Tuple-like variable)
public class Tuple<X,Y> {
	public X Item1 { get; private set;}
	public Y Item2 { get; private set;}
	
	public Tuple(X item1, Y item2) {
		Item1 = item1;
		Item2 = item2;
	}
}

public class JointContainer : MonoBehaviour
{
	// PWM Port, Node that contains the wheelCollider
	public static Dictionary<int, List<UnityRigidNode>> wheels;

	// Solenoid Value Pair, UnityRigidNode that contains them - may need to change later
	public static Dictionary<Tuple<int, int>, UnityRigidNode> solneoids;
}

