using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MediaManager
{
    private List<GIF> gifs;

    private static MediaManager instance;

    public static MediaManager getInstance()
    {
        if (instance == null) instance = new MediaManager();
        return instance;
    }
    
    private MediaManager()
    {
        gifs = new List<GIF>();
        gifs.Add(new GIF("loveseat", 77));
        //gifs.Add(new GIF("kermit", 6));
        gifs.Add(new GIF("darts", 59));
        gifs.Add(new GIF("neonsign", 40));
        gifs.Add(new GIF("pingpong", 35));
    }

    public GIF GetGif(string name)
    {
        return gifs.Find((x) => x.GifName.Equals(name));
    }

}
