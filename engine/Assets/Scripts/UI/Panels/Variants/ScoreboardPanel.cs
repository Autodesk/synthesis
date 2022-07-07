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
        redScoreTextMesh.text = TempScoreManager.redScore.ToString();
        blueScoreTextMesh.text = TempScoreManager.blueScore.ToString();
        
        EventBus.NewTypeListener<OnScoreEvent>(e =>
        {
            redScoreTextMesh.text = TempScoreManager.redScore.ToString();
            blueScoreTextMesh.text = TempScoreManager.blueScore.ToString();
        });
    }
}