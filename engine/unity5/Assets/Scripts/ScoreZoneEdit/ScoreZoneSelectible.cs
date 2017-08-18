using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using BulletSharp;
using UnityEngine;

public class ScoreZoneSelectible : MonoBehaviour
{
    private ScoreZoneSelectableManager selecManager;
    private ScoreZoneManipulatorManager manipManager;

    public ScoreZoneSettingsContainer SettingsContainer  = new ScoreZoneSettingsContainer()
        {
            Score = 0f,
            DestroyGamePieceOnScore = true,
            ReinstantiateGamePieceOnScore = true,
            TeamZone = ScoreZoneSettingsContainer.Team.Blue
        };
	
    // Use this for initialization
    void Start ()
    {
        SettingsContainer.Scale = transform.localScale;
        SettingsContainer.Position = transform.position;
        SettingsContainer.Rotation = transform.rotation;
        
        selecManager = GameObject.Find("EditScoreZoneManager").GetComponent<ScoreZoneSelectableManager>();
        manipManager = GetComponent<ScoreZoneManipulatorManager>();
    }
	
    // Update is called once per frame
    void Update () {
    }

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            selecManager.SelectZone(this.gameObject);
        }
    }

    public void SetScale(Vector3 scale)
    {
        SettingsContainer.Scale = scale;
        this.transform.localScale = scale;
    }
    
    public void SetReinstantiationPref(bool val)
    {
        SettingsContainer.ReinstantiateGamePieceOnScore = val;
    }
    
    public void SetDestroyPref(bool val)
    {
        SettingsContainer.DestroyGamePieceOnScore = val;
    }
    
    public void SetScore(float score)
    {
        SettingsContainer.Score = score;
    }

    public void SetTeam(ScoreZoneSettingsContainer.Team team)
    {
        SettingsContainer.TeamZone = team;
        // manipManager.SetTeam(team);
    }
}