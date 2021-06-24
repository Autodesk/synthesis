using Synthesis.Util;
using Synthesis.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Threading.Tasks;

public class SampleTest : MonoBehaviour
{
    // Start is called before the first frame update
    
    void Start()
    {

        //Thread mainThread = Thread.CurrentThread;
        string mainThreadName = Thread.CurrentThread.Name;
        Debug.Log($"Main Thread is {mainThreadName}");

        Thread threadTest = new Thread(() =>
        {           
            Debug.Log($"Current Thread is {Thread.CurrentThread.Name}");

            var r = UnityResyncer.Resync(() =>
            {
                Debug.Log($"Thread Resynched is {Thread.CurrentThread.Name}");
            });

        });

        threadTest.Name = "new thread";
        threadTest.Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
