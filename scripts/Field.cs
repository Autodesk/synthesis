using UnityEngine;
using System;
using System.Collections.Generic;

public class Field
{
	/// <summary>
	/// The field instance.
	/// </summary>
	GameObject field;

	List<Transform> childList;

	/// <summary>
	/// Initializes a new instance of the <see cref="Field"/> class.
	/// </summary>
	public Field (string filename, Vector3 position, Vector3 scale)
	{
		field = (GameObject) GameObject.Instantiate (Resources.Load(filename), position, Quaternion.identity);
		field.transform.localScale = scale;
		field.AddComponent ("Rigidbody");
		field.rigidbody.constraints = RigidbodyConstraints.FreezeAll;

		childList = GetAllChildren (field.transform);
		Debug.Log (childList.Count);
	}

	/// <summary>
	/// Enables collision for the desired objects.
	/// </summary>
	/// <param name="collisionObjects">Collision objects.</param>
	public void EnableCollisionObjects(params string[] collisionObjects)
	{
		foreach (Transform child in childList)
		{
			foreach (string s in collisionObjects)
			{
				if (child.name.Equals(s))
				{
					if (child.GetComponent<MeshFilter>() == null)
					{
						MeshFilter[] filters = child.GetComponentsInChildren<MeshFilter>();

						foreach (MeshFilter f in filters)
						{
							f.gameObject.AddComponent<MeshCollider>();
						}
					}
					else
					{
						child.gameObject.AddComponent<MeshCollider>();
					}
				}
			}
		}

	}

	/// <summary>
	/// Gets all child transforms of the field transform.
	/// </summary>
	/// <returns>The all children.</returns>
	/// <param name="transform">Transform.</param>
	private List<Transform> GetAllChildren(Transform transform)
	{
		List<Transform> children = new List<Transform> ();

		foreach (Transform t in transform)
		{
			children.Add(t);
			if (t.childCount > 0)
				children.AddRange(GetAllChildren(t));
		}

		return children;
	}

}
