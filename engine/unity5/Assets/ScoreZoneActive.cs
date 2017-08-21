using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreZoneActive : MonoBehaviour {
	
    private ScoreZoneSettingsContainer m_settingsContainer = new ScoreZoneSettingsContainer();
    private Collider m_collider;
    private MeshRenderer m_cubeMeshRenderer;
    private ScoreZoneSimSceneManager m_scoreZoneSimSceneManager;
    private DriverPracticeRobot m_dpmRobot; 

    // Use this for initialization
    void Start ()
    {
        m_collider = GetComponent<Collider>();
        m_cubeMeshRenderer = transform.Find("Cube").gameObject.GetComponent<MeshRenderer>();
        m_scoreZoneSimSceneManager = GameObject.Find("StateMachine").GetComponent<ScoreZoneSimSceneManager>();
        m_dpmRobot = GameObject.Find("StateMachine").GetComponent<DriverPracticeMode>().dpmRobot;
    }
	
    // Update is called once per frame
    void Update () {
		
    }

    void OnTriggerEnter(Collider other)
    {
        GameObject g = other.gameObject;
        if (g.tag != "Gamepiece_Primary" && g.tag != "Gamepiece_Secondary") return; // We don't care if the robot or field entered
		
        if (m_settingsContainer.TeamZone == ScoreZoneSettingsContainer.Team.Blue)
            m_scoreZoneSimSceneManager.IncreaseBlueScore(m_settingsContainer.Score, m_settingsContainer.ScoreType);
        else m_scoreZoneSimSceneManager.IncreaseRedScore(m_settingsContainer.Score, m_settingsContainer.ScoreType);

        if (m_settingsContainer.ReinstantiateGamePieceOnScore)
        {
            // if reinstantiate and destroy, just reset the gamepace to spawn pose and save the cpu cycles
            if (m_settingsContainer.DestroyGamePieceOnScore)
            {
                g.transform.position =
                    m_dpmRobot.gamepieceSpawn[g.GetComponent<GamePieceRememberSpawnParams>().PieceType];
                g.transform.rotation = Quaternion.identity;
            }

            // If we don't destroy, we can't get away with moving the piece, so we have to reinstantiate
            else m_dpmRobot.SpawnGamepiece(g.GetComponent<GamePieceRememberSpawnParams>().PieceType);
        }

        // Simply destroy if we don't care about reinstantiation
        else if (m_settingsContainer.DestroyGamePieceOnScore)
        {
            m_dpmRobot.RemoveGamepiece(g);
            Destroy(g);
        }
        
        // We don't do anything if we neither destroy or reinstantiate
    }


    public void SetContainer(ScoreZoneSettingsContainer container)
    {
        m_settingsContainer = container;
		
        transform.position = container.Position;
        transform.rotation = container.Rotation;
        transform.localScale = container.Scale;

// Set material color of scorezone based on team
        m_cubeMeshRenderer.material.color = (container.TeamZone == ScoreZoneSettingsContainer.Team.Blue)
            ? ScoreZoneManipulatorManager.blueDeselectColor // If we're blue scorezone
            : ScoreZoneManipulatorManager.redDeselectColor; // else (if we're red)

    }
}