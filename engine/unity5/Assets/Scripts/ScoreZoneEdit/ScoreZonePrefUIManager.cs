using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SocialPlatforms.Impl;
using System.Runtime.Serialization;

struct MinMax
{
    public float min;
    public float max;
}

public class ScoreZonePrefUIManager : MonoBehaviour
{
    public Slider xSlider, ySlider, zSlider;
    private Text xMinT, xMaxT, yMinT, yMaxT, zMinT, zMaxT;

    public Toggle DestroyOnScoreToggle;
    public Toggle InstantiateOnScoreToggle;
    public Button DestroyButton;
    public InputField ScoreInput;
    public Slider TeamInput;
	
    private MinMax xMM, yMM, zMM;
		
    private GameObject currentlySelected { get; set; }

    //public GameObject SelectableManager { get; private set; }
    private ScoreZoneSelectableManager scoreZoneSelectableManager;

    private Vector3 PointerDownSize;
		
    // Use this for initialization
    void Start ()
    {
        scoreZoneSelectableManager = GameObject.Find("EditScoreZoneManager").GetComponent<ScoreZoneSelectableManager>();

        // xSlider = transform.Find("xSize").gameObject.GetComponent<Slider>();
	    

        xMinT = xSlider.transform.Find("xMin").GetComponent<Text>();
        xMaxT = xSlider.transform.Find("xMax").GetComponent<Text>();
        yMinT = ySlider.transform.Find("yMin").GetComponent<Text>();
        yMaxT = ySlider.transform.Find("yMax").GetComponent<Text>();
        zMinT = zSlider.transform.Find("zMin").GetComponent<Text>();
        zMaxT = zSlider.transform.Find("zMax").GetComponent<Text>();
	    
        ScoreInput.onEndEdit.AddListener(delegate
        {
            float scoreVal;
            bool ret = float.TryParse(ScoreInput.text, out scoreVal);
            if (!ret) ScoreInput.text = "";
            else scoreZoneSelectableManager.SetScore(scoreVal); // Even if we can't parse, 0 is acceptable score
        });
	    
        TeamInput.onValueChanged.AddListener(delegate
        {
            scoreZoneSelectableManager.SetTeam(
                TeamInput.value == 1 ? ScoreZoneSettingsContainer.Team.Red : ScoreZoneSettingsContainer.Team.Blue
            );
        });
	    
        DestroyOnScoreToggle.onValueChanged.AddListener(delegate
        {
            scoreZoneSelectableManager.SetDestroyPref(DestroyOnScoreToggle.isOn);
        });
	    
        InstantiateOnScoreToggle.onValueChanged.AddListener(delegate
        {
            scoreZoneSelectableManager.SetReinstantationPref(InstantiateOnScoreToggle.isOn);
        });
    }
	
    // Update is called once per frame
    void Update ()
    {
        currentlySelected = scoreZoneSelectableManager.GetCurrentSelected();
    }

    // OnGui is called once per UI draw
    void OnGUI()
    {
        UIEnableDisable();
    }

    public void LoadPrefs(ScoreZoneSettingsContainer container)
    {
        calculateMinMax(container.Scale.x, ref xMM);
        calculateMinMax(container.Scale.y, ref yMM);
        calculateMinMax(container.Scale.z, ref zMM);

        xSlider.value = 0.5f;
        ySlider.value = 0.5f;
        zSlider.value = 0.5f;
		
        xMinT.text = xMM.min.ToString();
        xMaxT.text = xMM.max.ToString();
        yMinT.text = yMM.min.ToString();
        yMaxT.text = yMM.max.ToString();
        zMinT.text = zMM.min.ToString();
        zMaxT.text = zMM.max.ToString();

        // Set button and score to correct values from object
        DestroyOnScoreToggle.isOn = container.DestroyGamePieceOnScore;
        InstantiateOnScoreToggle.isOn = container.ReinstantiateGamePieceOnScore;
        ScoreInput.text = container.Score.ToString();

        // Set slider to correct team
        TeamInput.value = (container.TeamZone == ScoreZoneSettingsContainer.Team.Blue) ? 0 : 1;
    }

    // TODO
    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("On Pointer Up");
        currentlySelected = scoreZoneSelectableManager.GetCurrentSelected();
	    
        calculateMinMax(currentlySelected.transform.localScale.x, ref xMM);
        calculateMinMax(currentlySelected.transform.localScale.y, ref yMM);
        calculateMinMax(currentlySelected.transform.localScale.z, ref zMM);

        float xSliderVal = xSlider.value;
        float ySliderVal = ySlider.value;
        float zSliderVal = zSlider.value;

        xSlider.value = 0.5f;
        ySlider.value = 0.5f;
        zSlider.value = 0.5f;
	    

        xSlider.minValue = xMM.min;
        xSlider.maxValue = xMM.max;
        ySlider.minValue = yMM.min;
        ySlider.maxValue = yMM.max;
        zSlider.minValue = zMM.min;
        zSlider.maxValue = zMM.max;
    	
        // scoreZoneSelectableManager.SetScale(
        //  /    new Vector3(
        //  /	xSliderVal + currentlySelected.transform.localScale.x, 
        //  /	ySliderVal + currentlySelected.transform.localScale.y, 
        //  /	zSliderVal + currentlySelected.transform.localScale.z)
        //  /);

        xMinT.text = xSlider.minValue.ToString();
        xMaxT.text = xSlider.maxValue.ToString();
        yMinT.text = ySlider.minValue.ToString();
        yMaxT.text = ySlider.maxValue.ToString();
        zMinT.text = zSlider.minValue.ToString();
        zMaxT.text = zSlider.maxValue.ToString();
	    
        xSlider.value = ySlider.value = zSlider.value = 0.5f;
    }

    public void OnPointerDown()
    {
        PointerDownSize = currentlySelected.transform.localScale;
    }

    private void calculateMinMax(float current, ref MinMax MM)
    {
        MM.min = current / 2;
        MM.max = current * 2;
    }

    private void UIEnableDisable()
    {
        bool enable = (currentlySelected != null);
		
        xSlider.interactable = enable;
        ySlider.interactable = enable;
        zSlider.interactable = enable;
        DestroyButton.interactable = enable;
        DestroyOnScoreToggle.interactable = enable;
        InstantiateOnScoreToggle.interactable = enable;
        TeamInput.interactable = enable;
        ScoreInput.interactable = enable;
    }
}