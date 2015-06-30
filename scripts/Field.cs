using UnityEngine;
using System;
using System.Collections.Generic;

public class Field
{
	/// <summary>
	/// The field instance.
	/// </summary>
	GameObject field;

	/// <summary>
	/// A dictionary of every child transform.
	/// </summary>
	Dictionary<string, Transform> allTransforms;

	/// <summary>
	/// Contains the objects that have been enabled for collision (used to improve performance).
	/// </summary>
	Dictionary<string, Transform> collisionTransforms;

	/// <summary>
	/// Initializes a new instance of the <see cref="Field"/> class.
	/// </summary>
	public Field (string filename, Vector3 position, Vector3 scale)
	{
		field = (GameObject) GameObject.Instantiate (Resources.Load(filename), position, Quaternion.identity);
		field.transform.localScale = scale;
		field.AddComponent ("Rigidbody");
		field.rigidbody.constraints = RigidbodyConstraints.FreezeAll;

		allTransforms = GetAllChildren (field.transform);
		collisionTransforms = new Dictionary<string, Transform>();
	}

	/// <summary>
	/// Adds and enables collision functionality for the supplied objects.
	/// </summary>
	/// <param name="collisionObjects">Collision objects.</param>
	public void AddCollisionObjects(params string[] collisionObjects)
	{
		foreach (string s in collisionObjects)
		{
			if (allTransforms[s] != null && !collisionTransforms.ContainsKey(s))
			{
				collisionTransforms.Add(allTransforms[s].name, allTransforms[s]);

				if (collisionTransforms[s].GetComponent<MeshFilter> () == null)
				{
					MeshFilter[] filters = collisionTransforms[s].GetComponentsInChildren<MeshFilter> ();
					
					foreach (MeshFilter f in filters)
					{
						f.gameObject.AddComponent<MeshCollider> ();
					}
				}
				else
				{
					collisionTransforms[s].gameObject.AddComponent<MeshCollider> ();
				}
			}
		}
	}

	/// <summary>
	/// Sets the enabled state of each registered collision object.
	/// </summary>
	/// <param name="enabled">If set to <c>true</c> enabled.</param>
	public void SetCollisionObjectsEnabled(bool enabled)
	{
		foreach (Transform t in collisionTransforms.Values)
		{
			if (t.GetComponent<MeshCollider> () == null)
			{
				MeshCollider[] colliders = t.GetComponentsInChildren<MeshCollider>();

				foreach (MeshCollider c in colliders)
				{
					c.gameObject.GetComponent<MeshCollider>().enabled = enabled;
				}
			}
			else
			{
				t.gameObject.GetComponent<MeshCollider>().enabled = enabled;
			}
		}
	}

	/// <summary>
	/// Sets the enabled state of the supplied registered collision object.
	/// </summary>
	/// <param name="collisionObjects">Collision objects.</param>
	public void SetCollisionObjectsEnabled(bool enabled, params string[] collisionObjects)
	{
		foreach (string s in collisionObjects)
		{
			if (collisionTransforms.ContainsKey (s))
			{
				if (collisionTransforms[s].GetComponent<MeshCollider> () == null)
				{
					MeshCollider[] colliders = collisionTransforms[s].GetComponentsInChildren<MeshCollider>();
					
					foreach (MeshCollider c in colliders)
					{
						c.gameObject.GetComponent<MeshCollider>().enabled = enabled;
					}
				}
				else
				{
					collisionTransforms[s].gameObject.GetComponent<MeshCollider>().enabled = enabled;
				}
			}
		}
	}

	/// <summary>
	/// Gets all child transforms of the field transform.
	/// </summary>
	/// <returns>The all children.</returns>
	/// <param name="transform">Transform.</param>
	private Dictionary<string, Transform> GetAllChildren(Transform transform)
	{
		Dictionary<string, Transform> children = new Dictionary<string, Transform> ();

		foreach (Transform t in transform)
		{
			children.Add(t.name, t);
			if (t.childCount > 0)
			{
				children.AddAll(GetAllChildren(t));
			}
		}

		return children;
	}

}
