using Synthesis.UI.Panels;
using SynthesisAPI.EventBus;
using TMPro;
using UnityEngine;

public class ScoreboardPanel : Panel
{
    public GameObject redScoreText;
    public GameObject blueScoreText;
    
    private TextMeshProUGUI redScoreTextMesh;
    private TextMeshProUGUI blueScoreTextMesh;

    private int redScore = 0;
    private int blueScore = 0;

    private void Start()
    {
        redScoreTextMesh = redScoreText.GetComponent<TextMeshProUGUI>();
        blueScoreTextMesh = blueScoreText.GetComponent<TextMeshProUGUI>();
        redScoreTextMesh.text = "0";
        blueScoreTextMesh.text = "0";
        
        EventBus.NewTypeListener<OnScoreEvent>(e =>
        {
            var se = (OnScoreEvent) e;
            switch (se.zone.alliance)
            {
                case Alliance.RED:
                    redScore += se.zone.points;
                    redScoreTextMesh.text = redScore.ToString();
                    break;
                case Alliance.BLUE:
                    blueScore += se.zone.points;
                    blueScoreTextMesh.text = blueScore.ToString();
                    break;
            }
        });
    }
}