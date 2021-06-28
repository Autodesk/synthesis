using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Synthesis.Util;
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


    [UnityTest]
    public IEnumerator UnityResyncerTest()
    {
        GameObject resyncerComponent = new GameObject("Unity Resyncer");
        var resyncer = resyncerComponent.AddComponent<UnityResyncerComponent>();
        yield return new WaitForSeconds(0.1f);
        if(Thread.CurrentThread.Name==null||!Thread.CurrentThread.Name.Equals("main"))
            Thread.CurrentThread.Name = "main";

        Thread threadTest = new Thread(() =>
        {
            Assert.IsFalse(Thread.CurrentThread.Name=="main");
            var r = UnityResyncer.Resync(() =>
            {
                Assert.IsTrue(Thread.CurrentThread.Name.Equals("main"));
            });
        });
        threadTest.Name = "new thread";
        threadTest.Start();
        yield return new WaitForSeconds(0.1f);
        yield return null;
    }
}
