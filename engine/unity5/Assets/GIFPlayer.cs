using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class GIFPlayer : MonoBehaviour
{
    public float frameRate;
    public Sprite[] imgs;

    private float lastNewFrame = 0;
    private int currentFrame = 0;
    private Image img;

    public void Start()
    {
        img = GetComponent<Image>();
        img.sprite = imgs[0];
    }

    public void Update()
    {
        if (lastNewFrame + (1 / frameRate) <= Time.unscaledTime)
        {
            currentFrame++;
            if (currentFrame >= imgs.Length)
            {
                currentFrame = 0;
            }
            img.sprite = imgs[currentFrame];
            lastNewFrame = Time.unscaledTime;
        }
    }
}
