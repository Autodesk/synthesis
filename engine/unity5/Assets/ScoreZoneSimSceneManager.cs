using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using UnityEngine.UI;
using System;
using System.Globalization;

public class ScoreZoneSimSceneManager : MonoBehaviour
{
	public GameObject CubeScoreZonePrefab;

	public float BluePrimaryScore { get; set; }
	public float BlueSecondaryScore { get; set; }
	public float RedPrimaryScore { get; set; }
	public float RedSecondaryScore { get; set; }

	public Text m_blueScoreText;
	public Text m_redScoreText;

	public Slider m_timeSlider;
	public Text m_timerText;

	public Image fill;

	private SimUI m_simUI;

	private bool m_timerEnabled = false;
	private float m_timerTime = 150;
	private float m_lastTimerIncrementTime; 
	
	// Use this for initialization
	void Start ()
	{
		m_simUI = GameObject.Find("StateMachine").GetComponent<SimUI>();
		BluePrimaryScore = BlueSecondaryScore = RedPrimaryScore = RedSecondaryScore = 0.0f;
		
		ResetTimer();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	string TimerParse(float num)
	{
		int intNum = (int) num;
		int seconds = intNum % 60;
		int minutes = intNum / 60;
		if (seconds >= 10)
			return minutes + ":" + seconds;
		return minutes + ":0" + seconds;
	}

	void OnGUI()
	{
		m_blueScoreText.text = (Math.Round((double) BluePrimaryScore + BlueSecondaryScore)).ToString();
		m_redScoreText.text = (Math.Round((double) RedPrimaryScore + RedSecondaryScore)).ToString();

		if (m_timerTime >= 30f) fill.color = Color.green;
		else if (m_timerTime < 1f) fill.color = Color.red;
		else fill.color = Color.yellow;

		if (m_timerEnabled && m_timerTime > 0)
		{
			m_timerTime -= Time.time - m_lastTimerIncrementTime;
			m_timeSlider.value = 150-m_timerTime;
			m_lastTimerIncrementTime = Time.time;
			m_timerText.text = TimerParse(m_timerTime);
		}
	}

	public void StartTimer()
	{
		m_timerEnabled = true;
		m_lastTimerIncrementTime = Time.time;
	}
	
	public void PauseTimer()
	{
		m_timerEnabled = false;
	}

	public void ResetTimer()
	{
		m_timerTime = 150;
		StartTimer();

		BluePrimaryScore = RedPrimaryScore = BlueSecondaryScore = RedSecondaryScore = 0f;
	}

	// TODO 
	public void IncreaseBlueScore(float score, ScoreZoneSettingsContainer.ScoreTypes scoreType)
	{
		if (scoreType == ScoreZoneSettingsContainer.ScoreTypes.Primary) BluePrimaryScore += score;
		else BlueSecondaryScore += score;
	}

	public void IncreaseRedScore(float score, ScoreZoneSettingsContainer.ScoreTypes scoreType)
	{
		if (scoreType == ScoreZoneSettingsContainer.ScoreTypes.Primary) RedPrimaryScore += score;
		else RedSecondaryScore += score;
	}

	private GameObject InstantiateZone(ScoreZoneSettingsContainer.Shapes zoneType)
	{
		if (zoneType == ScoreZoneSettingsContainer.Shapes.Cube)
			return (GameObject) Instantiate(CubeScoreZonePrefab);
		else return null;
	}

	public void LoadScoreZones()
	{
		string path = PlayerPrefs.GetString("simSelectedField") + "\\ScoreZones.xml";

		// First check if files exist
		if (!File.Exists(path)) Debug.Log("Could NOT find any score zone defs at " + path);
		else Debug.Log("Found score zone defs at " + path);

		List<ScoreZoneSettingsContainer> containerList = new List<ScoreZoneSettingsContainer>();
		
		var xmlSerializer = new XmlSerializer(typeof(List<ScoreZoneSettingsContainer>));
		Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
		containerList = (List<ScoreZoneSettingsContainer>) xmlSerializer.Deserialize(stream);
		
		Debug.Log("Found " + containerList.Count.ToString() + " zones");

		for (int i=0; i<containerList.Count; i++)
		{
			Debug.Log("Instantiating " + containerList[i].ToString());
			Debug.Log(containerList[i].Position.ToString());
			Debug.Log(containerList[i].Rotation.ToString());
			Debug.Log(containerList[i].Scale.ToString());
			Debug.Log(containerList[i].TeamZone.ToString());
			Debug.Log(containerList[i].ZoneType == ScoreZoneSettingsContainer.Shapes.Cube);
			GameObject zone = InstantiateZone(containerList[i].ZoneType);
			Debug.Log(zone);
			zone.GetComponent<ScoreZoneActive>().SetContainer(containerList[i]);
		}
	}
}
