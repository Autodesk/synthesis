using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MatchResultsTracker
{
    public Dictionary<Type, ITrackedData> TrackedResults = new Dictionary<Type, ITrackedData>();

    public MatchResultsTracker()
    {
        TrackedResults.Add(typeof(BluePoints), new BluePoints());
        TrackedResults.Add(typeof(RedPoints), new RedPoints());
        TrackedResults.Add(typeof(TestEntry), new TestEntry());
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
            return "Red Points";
        }
    }

    public class TestEntry : ITrackedData
    {
        public float Value;
        
        public override string ToString()
        {
            return Value.ToString();
        }
        
        public string GetName()
        {
            return "Test Entry";
        }
    }
}
