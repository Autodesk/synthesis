using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreZoneBehavior : MonoBehaviour
{

	public float Score { get; set; }

	public bool DestroyGamePieceOnScore { get; set; }
	public bool ReinstantiateGamePieceOnScore { get; set; }
	
	
	// We can't reliably detect what is a gamepiece and what's not, so we're simply going to look for things that
	// arent fields, aren't robots, and have colliders (no scoring when .

	public Collider collider { get; private set; }

	// Use this for initialization
	void Start ()
	{
		collider = GetComponent<Collider>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
