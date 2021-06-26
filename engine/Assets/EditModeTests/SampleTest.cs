using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SampleTest
{
    // A Test behaves as an ordinary method
    [Test]
    public void SampleTestSimplePasses()
    {
        Debug.Log("Test");
        Assert.IsTrue(true);
    }
}
