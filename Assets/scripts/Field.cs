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
	/// Contains any additional components added manually.
	/// </summary>
	Dictionary<string, Component> extraComponents;

	/// <summary>
	/// Contains every child transform.
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
		field.rigidbody.isKinematic = true;

		extraComponents = new Dictionary<string, Component> ();
		allTransforms = GetAllChildren (field.transform);
		collisionTransforms = new Dictionary<string, Transform>();
	}

	/// <summary>
	/// Destroy the field.
	/// </summary>
	public void Destroy()
	{
		GameObject.Destroy (field);
		extraComponents.Clear ();
		allTransforms.Clear ();
		collisionTransforms.Clear ();
	}

	/// <summary>
	/// Adds the specified type of component to the field and returns the added component.
	/// </summary>
	/// <returns>The component.</returns>
	/// <param name="id">Identifier.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public T AddComponent<T>(string id) where T : Component
	{
		T component = field.AddComponent<T> ();
		extraComponents.Add (id, component);
		return component;
	}

	/// <summary>
	/// Returns the component containing the specified ID.
	/// </summary>
	/// <returns>The component.</returns>
	/// <param name="id">Identifier.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public T GetComponent<T>(string id) where T : Component
	{
		return (T) extraComponents [id];
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

	public Transform getCollisionObjects(string collisionObject)
	{
		if (collisionTransforms.ContainsKey (collisionObject))
		{
			if (collisionTransforms [collisionObject].GetComponent<MeshCollider> () == null)
				return collisionTransforms [collisionObject].transform;
			else
				return collisionTransforms [collisionObject];
		}
		else
		{
			return null;
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
