using System;
using System.Collections;
using System.Collections.Generic;
using BulletSharp;
using UnityEngine;
using BulletSharp.Math;
using BulletUnity;


public class ScoreZoneActive : MonoBehaviour {
	
    public ScoreZoneSettingsContainer m_settingsContainer { get; private set; }
    public Collider m_collider { get; private set; }
    public MeshRenderer m_cubeMeshRenderer { get; private set; }
    public GameObject subCube { get; private set; }
    public ScoreZoneSimSceneManager m_scoreZoneSimSceneManager { get; private set; }
    private DriverPracticeRobot m_dpmRobot;

    public DriverPracticeMode m_dpm;

    public ScoreZoneActive() {
        m_settingsContainer = new ScoreZoneSettingsContainer();
    }


    void Start()
    {
        m_collider = GetComponent<Collider>();
        // m_cubeMeshRenderer = transform.Find("Cube").gameObject.GetComponent<MeshRenderer>();
        m_cubeMeshRenderer = subCube.GetComponent<MeshRenderer>();
        m_scoreZoneSimSceneManager = GameObject.Find("StateMachine").GetComponent<ScoreZoneSimSceneManager>();
        m_dpmRobot = GameObject.Find("StateMachine").GetComponent<DriverPracticeMode>().dpmRobot;
        m_dpm = GameObject.Find("StateMachine").GetComponent<DriverPracticeMode>();

        this.GetComponent<BBoxShape>().Extents = transform.localScale * 0.5f;
    }


    public void SetContainer(ScoreZoneSettingsContainer container)
    {
        m_settingsContainer = container;
		
        transform.position = container.Position;
        transform.rotation = container.Rotation;
        transform.localScale = container.Scale;

        // Set material color of scorezone based on team
        Debug.Log(subCube);
        Debug.Log(GetComponentInChildren<MeshRenderer>());
        Debug.Log(container);
        Debug.Log(container.TeamZone);
        GetComponentInChildren<MeshRenderer>().material.color = (container.TeamZone == ScoreZoneSettingsContainer.Team.Blue)
            ? ScoreZoneManipulatorManager.blueDeselectColor // If we're blue scorezone
            : ScoreZoneManipulatorManager.redDeselectColor; // else (if we're red)

    }

    public DriverPracticeRobot getRobot()
    {
        return m_dpmRobot;
    }

    public void SpawnGamepiece(int index)
    {
        m_dpmRobot.SpawnGamepiece(index);
    }

    public void RemoveGamepiece(GameObject g)
    {
        m_dpmRobot.RemoveGamepiece(g);
        Destroy(g);
    }


    public void CollisionHandler(GameObject g)
    {
            
        Debug.Log("g.tag: " + g.tag);
        if (g.tag != "Gamepiece_Primary" && g.tag != "Gamepiece_Secondary") return; // We don't care if the robot or field entered

        if (m_settingsContainer.TeamZone == ScoreZoneSettingsContainer.Team.Blue)
        {
            if ((g.tag == "Gamepiece_Primary" &&
                 m_settingsContainer.ScoreType == ScoreZoneSettingsContainer.ScoreTypes.Primary) ||
                (g.tag == "Gamepiece_Secondary" &&
                 m_settingsContainer.ScoreType == ScoreZoneSettingsContainer.ScoreTypes.Secondary))
                m_scoreZoneSimSceneManager.IncreaseBlueScore(m_settingsContainer.Score, m_settingsContainer.ScoreType);
        }

        else if ((g.tag == "Gamepiece_Primary" &&
                  m_settingsContainer.ScoreType == ScoreZoneSettingsContainer.ScoreTypes.Primary) ||
                 (g.tag == "Gamepiece_Secondary" &&
                  m_settingsContainer.ScoreType == ScoreZoneSettingsContainer.ScoreTypes.Secondary))
            m_scoreZoneSimSceneManager.IncreaseRedScore(m_settingsContainer.Score, m_settingsContainer.ScoreType);

        if (m_settingsContainer.ReinstantiateGamePieceOnScore)
        {
            // if reinstantiate and destroy, just reset the gamepace to spawn pose and save the cpu cycles
            // if (m_settingsContainer.DestroyGamePieceOnScore)
            // {
            //     g.transform.position =
            //         m_dpmRobot.gamepieceSpawn[g.GetComponent<GamePieceRememberSpawnParams>().PieceType];
            //     g.transform.rotation = UnityEngine.Quaternion.identity;
            // }

            // If we don't destroy, we can't get away with moving the piece, so we have to reinstantiate
            SpawnGamepiece(g.GetComponent<GamePieceRememberSpawnParams>().PieceType);
        }

        // Simply destroy if we don't care about reinstantiation
        else if (m_settingsContainer.DestroyGamePieceOnScore)
        {
            RemoveGamepiece(g);
        }
    }
}