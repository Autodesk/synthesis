using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineScript : MonoBehaviour
{
    LineRenderer line;

    // Start is called before the first frame update
    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.sortingLayerName = "Foreground";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
