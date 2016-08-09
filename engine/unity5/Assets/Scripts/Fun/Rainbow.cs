using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class Rainbow : MonoBehaviour
{
    const float RESOLUTION = 1000f;
    const int PHASE_DURATION = 1000;

    Material material;
    Color color;
    float startTime;

    void Start()
    {
        MeshRenderer m = gameObject.GetComponent<MeshRenderer>();
        material = new Material(gameObject.GetComponentInChildren<MeshRenderer>().sharedMaterial);
        color = new Color();
        startTime = Time.realtimeSinceStartup;

        Update();
    }

    void Update()
    {
        int val = (int)((Time.realtimeSinceStartup - startTime) * RESOLUTION) % (PHASE_DURATION * 3);

        if (val < PHASE_DURATION)
        {
            color.r = (PHASE_DURATION - val) / (float)PHASE_DURATION;
            color.g = val / (float)PHASE_DURATION;
            color.b = 0;
        }
        else if (val < PHASE_DURATION * 2)
        {
            color.r = 0;
            color.g = (PHASE_DURATION * 2 - val) / (float)PHASE_DURATION;
            color.b = (val - PHASE_DURATION) / (float)PHASE_DURATION;
        }
        else
        {
            color.r = (val - PHASE_DURATION * 2) / (float)PHASE_DURATION;
            color.g = 0;
            color.b = (PHASE_DURATION * 3 - val) / (float)PHASE_DURATION;
        }

        material.color = color;
        gameObject.GetComponentInChildren<MeshRenderer>().sharedMaterial = material;
    }
}