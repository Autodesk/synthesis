using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using UnityEngine.UI;

public class ScoreZoneSimSceneManager : MonoBehaviour
{
	public GameObject CubeScoreZonePrefab;

	public float BluePrimaryScore { get; set; }
	public float BlueSecondaryScore { get; set; }
	public float RedPrimaryScore { get; set; }
	public float RedSecondaryScore { get; set; }

	private Text m_blueScoreText;
	private Text m_redScoreText;

	private SimUI m_simUI;
	
	// Use this for initialization
	void Start ()
	{
		m_simUI = GameObject.Find("StateMachine").GetComponent<SimUI>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	// TODO 
	public void IncreaseBlueScore(float score, ScoreZoneSettingsContainer.ScoreTypes scoreType)
	{

	}

	public void IncreaseRedScore(float score, ScoreZoneSettingsContainer.ScoreTypes scoreType)
	{
		
	}

	private GameObject InstantiateZone(int zoneType)
	{
		// 0 is cube
		// 1 is cylinder
		if (zoneType == 0)
			return (GameObject) Instantiate(CubeScoreZonePrefab);
		else return null;
	}

	public void LoadScoreZones()
	{
		string path = PlayerPrefs.GetString("simSelectedField") + "\\ScoreZones.xml";

		// First check if files exist
		if (!File.Exists(path)) return;

		List<ScoreZoneSettingsContainer> containerList = new List<ScoreZoneSettingsContainer>();
		
		var xmlSerializer = new XmlSerializer(typeof(List<ScoreZoneSettingsContainer>));
		Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
		containerList = (List<ScoreZoneSettingsContainer>) xmlSerializer.Deserialize(stream);

		foreach (ScoreZoneSettingsContainer i in containerList)
		{
			GameObject zone = InstantiateZone(i.ZoneType);
			zone.GetComponent<ScoreZoneActive>().SetContainer(i);
		}
	}
}
