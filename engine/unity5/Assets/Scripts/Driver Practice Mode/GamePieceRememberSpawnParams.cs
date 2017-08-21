using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePieceRememberSpawnParams : MonoBehaviour
{

	public Quaternion StartRotation { get; private set; }
	public Vector3 StartPosition { get; private set; }
	public Vector3 StartScale { get; private set; }
	
	public int PieceType { get; set; }

	// Use this for initialization
	void Awake ()
	{
		StartRotation = transform.rotation;
		StartPosition = transform.position;
		StartScale = transform.localScale;
	}
}
