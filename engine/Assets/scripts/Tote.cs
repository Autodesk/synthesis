using UnityEngine;
using System;

public class Tote : MonoBehaviour
{
	
	public static GameObject Create(Vector3 position, Vector3 rotation, Vector3 scale)
	{
		GameObject tote = (GameObject) GameObject.Instantiate (Resources.Load ("tote"), position, Quaternion.identity);
		tote.transform.Rotate (rotation);
		tote.transform.localScale = scale;
		tote.AddComponent<Rigidbody> ();
		tote.rigidbody.mass = 0.01f;

		PhysicMaterial material = new PhysicMaterial ("toteMaterial");
		material.dynamicFriction = 0.3f;
		material.staticFriction = 0.4f;
		material.frictionCombine = PhysicMaterialCombine.Minimum;

		BoxCollider bodyCollider = tote.AddComponent<BoxCollider> ();
		bodyCollider.center = new Vector3 (-0.6053294f, 0f, 0f);
		bodyCollider.size = new Vector3 (1.210662f, 2.416819f, 1.434109f);
		bodyCollider.material = material;

		BoxCollider lidCollider = tote.AddComponent<BoxCollider> ();
		lidCollider.center = new Vector3 (-1.107795f, 0f, 0f);
		lidCollider.size = new Vector3 (0.2009298f, 2.680527f, 1.625f);
		lidCollider.material = material;

		return tote;
	}

	void Start()
	{

	}

	void Update()
	{

	}

}

