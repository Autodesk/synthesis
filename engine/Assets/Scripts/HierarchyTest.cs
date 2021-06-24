using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Synthesis.UI.Hierarchy;
using Synthesis.UI.Hierarchy.HierarchyItems;

public class HierarchyTest : MonoBehaviour
{
    public Hierarchy hierarchy; 
    // Start is called before the first frame update
    void Start()
    {
         hierarchy.rootFolder.Title = "Hello World";
         hierarchy.rootFolder.Add(new HierarchyItem() { Title = "Sub Item" });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
