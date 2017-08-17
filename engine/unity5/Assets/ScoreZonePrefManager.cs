using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI;

struct MinMax
{
	public float min;
	public float max;
}

public class ScoreZonePrefManager : MonoBehaviour
{
	private Slider xSlider, ySlider, zSlider;
	private Text xMinT, xMaxT, yMinT, yMaxT, zMinT, zMaxT;
	private MinMax xMM, yMM, zMM;
		
	private GameObject currentlySelected { get; set; }

	public GameObject SelectableManager { get; private set; }
	private ScoreZoneSelectableManager scoreZoneSelectableManager;
		
	// Use this for initialization
	void Start ()
	{
		scoreZoneSelectableManager = SelectableManager.GetComponent<ScoreZoneSelectableManager>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		currentlySelected = scoreZoneSelectableManager.GetCurrentSelected();
		
		currentlySelected.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
	}

	// public void OnPointerUp()
	// {
	// 	currentlySelected = scoreZoneSelectableManager.CurrentlySelected;
	// 	calculateMinMax(currentlySelected.transform.localScale.x, ref xMM);
	// 	calculateMinMax(currentlySelected.transform.localScale.y, ref yMM);
	// 	calculateMinMax(currentlySelected.transform.localScale.z, ref zMM);

	// 	float xSliderVal = xSlider.value;
	// 	float ySliderVal = ySlider.value;
	// 	float zSliderVal = zSlider.value;
	// 	
	// 	currentlySelected.transform.localScale = new Vector3(
	// 		xSliderVal + currentlySelected.transform.localScale.x, 
	// 		ySliderVal + currentlySelected.transform.localScale.y, 
	// 		zSliderVal + currentlySelected.transform.localScale.z);

	// 	xMinT.text = xMM.min.ToString();
	// 	xMaxT.text = xMM.max.ToString();
	// 	yMinT.text = yMM.min.ToString();
	// 	yMaxT.text = yMM.max.ToString();
	// 	zMinT.text = zMM.min.ToString();
	// 	zMaxT.text = zMM.max.ToString();
	// }

	// private void calculateMinMax(float current, ref MinMax MM)
	// {
	// 	MM.min = current / 2;
	// 	MM.max = current * 2;
	// }
}
