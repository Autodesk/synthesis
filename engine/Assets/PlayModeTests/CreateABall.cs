using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CreateABall
{
    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator CreateABallWithEnumeratorPasses() {
        var g = new GameObject("Test_Ball");
        var sphere = g.AddComponent<SphereCollider>();
        var startPos = Vector3.up * 5.0f;
        sphere.radius = 0.5f;
        g.transform.position = startPos;
        g.AddComponent<Rigidbody>();
        yield return new WaitForSeconds(0.25f);
        Assert.IsFalse(g.transform.position == startPos);
        yield return null;
    }
}
