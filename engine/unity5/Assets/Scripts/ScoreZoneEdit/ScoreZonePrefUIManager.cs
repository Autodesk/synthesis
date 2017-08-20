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
using System.Globalization;

struct MinMax
{
    public float min;
    public float max;
}

public class ScoreZonePrefUIManager : MonoBehaviour
{
	public InputField xValue, yValue, zValue;

	
	public Button xAdd, xSubtract, yAdd, ySubtract, zAdd, zSubtract;
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


	    xValue.text = 1.ToString();
	    yValue.text = 1.ToString();
	    zValue.text = 1.ToString();
	    
	    
	    xValue.onEndEdit.AddListener(delegate { UpdateScale(); });
	    yValue.onEndEdit.AddListener(delegate { UpdateScale(); });
	    zValue.onEndEdit.AddListener(delegate { UpdateScale(); });

		xAdd.onClick.AddListener (delegate
		{
			//xValue.text = (double.Parse(xValue.text, CultureInfo.InvariantCulture) + .1f).ToString();
			xValue.text = (float.Parse(xValue.text) + .1F).ToString();
			UpdateScale();
		});

		yAdd.onClick.AddListener (delegate
		{
			yValue.text = (float.Parse(yValue.text) + .1F).ToString();
			UpdateScale();
		});

		zAdd.onClick.AddListener (delegate
		{
			zValue.text = (float.Parse(zValue.text) + .1F).ToString();
			UpdateScale();
		});

		xSubtract.onClick.AddListener (delegate
		{
			xValue.text = (float.Parse(xValue.text) - .1F).ToString();
			UpdateScale();
		});

		ySubtract.onClick.AddListener (delegate
		{
			yValue.text = (float.Parse(yValue.text) - .1F).ToString();
			UpdateScale();
		});

		zSubtract.onClick.AddListener (delegate
		{
			zValue.text = (float.Parse(zValue.text) - .1F).ToString();
			UpdateScale();
		});

    }

	void UpdateScale()
	{
		scoreZoneSelectableManager.SetScale(new Vector3(
			float.Parse(xValue.text), float.Parse(yValue.text), float.Parse(zValue.text)
		));
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
        // calculateMinMax(container.Scale.x, ref xMM);
        // calculateMinMax(container.Scale.y, ref yMM);
        // calculateMinMax(container.Scale.z, ref zMM);

		xValue.text = container.Scale.x.ToString();
		yValue.text = container.Scale.y.ToString();
		zValue.text = container.Scale.z.ToString();
		
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
		    	
        // scoreZoneSelectableManager.SetScale(
        //  /    new Vector3(
        //  /	xSliderVal + currentlySelected.transform.localScale.x, 
        //  /	ySliderVal + currentlySelected.transform.localScale.y, 
        //  /	zSliderVal + currentlySelected.transform.localScale.z)
        //  /);

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

		xValue.interactable = enable;
		yValue.interactable = enable;
		zValue.interactable = enable;
		xAdd.interactable = enable;
		xSubtract.interactable = enable;
		yAdd.interactable = enable;
		ySubtract.interactable = enable;
		zAdd.interactable = enable;
		zSubtract.interactable = enable;

        DestroyButton.interactable = enable;
        DestroyOnScoreToggle.interactable = enable;
        InstantiateOnScoreToggle.interactable = enable;
        TeamInput.interactable = enable;
        ScoreInput.interactable = enable;
    }
}