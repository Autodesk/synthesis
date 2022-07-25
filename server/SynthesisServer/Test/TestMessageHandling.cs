using Google.Protobuf.WellKnownTypes;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace SynthesisServer.Test {
    [TestFixture]
    public static class TestMessageHandling {
        [Test]
        public static void DoesThisWork() {
            var msg = new StatusMessage() {
                LogLevel = StatusMessage.Types.LogLevel.Error,
                Msg = "Invalid Message body"
            };
            var any = Any.Pack(msg);
            // string a = any.;
            // string b = StatusMessage.Descriptor.FullName;
            Assert.True(true);
        }
    }
}
