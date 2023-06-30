using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MatchResultsTracker
{
    public Dictionary<string, ITrackedData> TrackedResults = new Dictionary<string, ITrackedData>();

    public MatchResultsTracker()
    {
        TrackedResults.Add("BluePoints", new BluePoints());
        TrackedResults.Add("RedPoints", new RedPoints());
    }
    
    public interface ITrackedData
    {
        public string GetName();
    }

    public class BluePoints : ITrackedData
    {
        public int Points;

        public override string ToString()
        {
            return Points.ToString();
        }

        public string GetName()
        {
            return "Blue Points";
        }
    }

    public class RedPoints : ITrackedData
    {
        public int Points;
        
        public override string ToString()
        {
            return Points.ToString();
        }
        
        public string GetName()
        {
            return "Blue Points";
        }
    }
}
