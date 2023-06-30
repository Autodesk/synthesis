using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MatchResultsTracker
{
    public readonly Dictionary<Type, ITrackedData> MatchResultEntries = new Dictionary<Type, ITrackedData>();

    public MatchResultsTracker()
    {
        MatchResultEntries.Add(typeof(BluePoints), new BluePoints());
        MatchResultEntries.Add(typeof(RedPoints), new RedPoints());
    }
    
    /// The base interface for any tracked match statistics. Implement this to track a new statistic
    public interface ITrackedData
    {
        public string GetName();
    }
    
    /// The number of points scored by the blue team
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
    
    /// The number of points scored by the red team
    public class RedPoints : ITrackedData
    {
        public int Points;
        
        public override string ToString()
        {
            return Points.ToString();
        }
        
        public string GetName()
        {
            return "Red Points";
        }
    }
}
