using System;
using NUnit.Framework;
using SynthesisAPI.AssetManager;
using SynthesisAPI.Utilities;
using System.Threading;
using System.Collections.Generic;

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

            ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();
            Dictionary<string, UpdateMessage.Types.ModifiedFields> packets = new Dictionary<string, UpdateMessage.Types.ModifiedFields>();

            packets["r1"] = new UpdateMessage.Types.ModifiedFields();
            packets["r1"].DOs["DigitalOutput1"] = new DigitalOutput
            {
                Name = "DO1"
                Type =  ""
            };

            packets["r2"] = new UpdateMessage.Types.ModifiedFields();

            RobotManager.Instance.Update(TcpServerManager.Packets, TcpServerManager.PacketsLock);
        }

    }
}
