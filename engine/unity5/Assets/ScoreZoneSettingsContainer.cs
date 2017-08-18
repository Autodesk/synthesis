using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ScoreZoneSettingsContainer
{
	public float Score { get; set; }

	public bool DestroyGamePieceOnScore { get; set; }
	public bool ReinstantiateGamePieceOnScore { get; set; }

	public Vector3 Scale { get; set; }
	public Vector3 Position { get; set; }
	public Quaternion Rotation { get; set; }

	public enum Team
	{
		Red,
		Blue
	};

	public Team TeamZone { get; set; }
}
