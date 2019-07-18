using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GIF {

    public string GifName { get; private set; }
    public int FrameCount { get; private set; }

    private int index = 0;

    private List<Sprite> frames;

    public GIF(string Name, int FrameCount)
    {
        //GifName = Name;
        //frames = new List<Sprite>();
        //this.FrameCount = FrameCount;
        //LoadFrames();
    }

    private void LoadFrames()
    {
        for (int i = 0; i < FrameCount; i++)
        {
            //frames.Add(Resources.Load<Sprite>("Gifs/" + GifName + "/" + i + "_" + GifName));
        }
    }

    public Sprite NextFrame()
    {
        //Sprite a = frames[index];
        //index = (index + 1) % FrameCount;
        return null;
    }

}
