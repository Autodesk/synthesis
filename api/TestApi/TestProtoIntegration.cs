using System;
using NUnit.Framework;
using SynthesisAPI.AssetManager;
using SynthesisAPI.Utilities;
using System.Threading;
using System.Collections.Generic;
using Mirabuf.Signal;
using Mirabuf;

namespace TestApi {
    
    
    [TestFixture]
    public static class TestProtoIntegration
    {
        [Test]
        public static void TestIngtegration()
        {

            // Assert.Pass() or Assert.Fail() pretty self explanatory
            // Assert.IsTrue([condition]) or Assert.IsFalse([condition])
            // Loads more, check with intellisense

            RobotManager.Instance.AddSignalLayout(new Signals() 
            {
                Info = new Mirabuf
            });
        }

    }
}
