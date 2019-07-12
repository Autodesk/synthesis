using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class GIFPlayer : MonoBehaviour
{
    public string[] GifNames;
    public float FrameRate;

    private GIF gif = null;
    private float LastFrame = 0;
    private Image img = null;

    public void Start()
    {
        gif = MediaManager.getInstance().GetGif(GifNames[0]);
        img = GetComponent<Image>();
    }

    public void OnEnable()
    {
        int gifInd = UnityEngine.Random.Range(0, GifNames.Length);
        Debug.Log(gifInd.ToString());
        gif = MediaManager.getInstance().GetGif(GifNames[gifInd]);
    }

    public void OnDisable()
    {
        img.sprite = null;
    }

    public void Update()
    {
        if (LastFrame + (1 / FrameRate) <= Time.unscaledTime)
        {
            img.sprite = gif.NextFrame();
            LastFrame = Time.unscaledTime;
        }
    }
}
