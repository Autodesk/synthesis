using UnityEngine;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// Used to display and change AI behaviours in simulator
/// </summary>
public class ChangeBehaviourScrollable : ScrollablePanel
{
    private string directory;
    List<BaseSynthBehaviour> behaviours;
    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        listStyle.fontSize = 14;
        highlightStyle.fontSize = 14;
        toScale = false;
        errorMessage = "No behaviours found in directory!";
    }

    void OnEnable()
    {
        items = new List<string>();
        items.Clear();

        // Retrieve list of behaviours using reflection. Very costly operation. Use sparingly.
        // We only use this once because behaviours are compiled into executable. This should not need to update often.
        behaviours = ChangeBehaviourScrollable.GetEnumerableOfType<BaseSynthBehaviour>();
        if (behaviours != null)
        {
            foreach (BaseSynthBehaviour behaviour in behaviours)
            {
                items.Add(behaviour.ToString());
            }

            if (items.Count > 0) { selectedEntry = items[0]; }
        }
    }

    public BaseSynthBehaviour GetSelectedBehaviour()
    {
        foreach(BaseSynthBehaviour behaviour in behaviours)
        {
            if (selectedEntry.Equals(behaviour.ToString()))
            {
                return behaviour;
            }
        }
        return null;
    }

    protected override void OnGUI()
    {
        position = GetComponent<RectTransform>().position;

        base.OnGUI();
    }

    private static List<T> GetEnumerableOfType<T>(params object[] constructorArgs) where T : class, IComparable<T>
    {
        List<T> objects = new List<T>();
        foreach (Type type in
            Assembly.GetAssembly(typeof(T)).GetTypes()
            .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
        {
            objects.Add((T)Activator.CreateInstance(type, constructorArgs));
        }
        objects.Sort();
        return objects;
    }
}
