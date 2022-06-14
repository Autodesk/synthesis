using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections;

using SynthesisAPI.InputManager.Inputs;

namespace TestApi {
    [TestFixture]
    public static class TestJsonSerialization {
        [Test]
        public static void TestAnalogSerialization() {
            var original = new Analog("Joystick 1 Axis 1", true, true, (int)ModKey.LeftShift, 0.1f);
            Assert.IsFalse(original.Equals(new Analog("Joystick 2 Axis 1", true, false, (int)ModKey.LeftShift, 0.1f)));
            var serializedOrignal = JsonConvert.SerializeObject(original);
            var copy = JsonConvert.DeserializeObject<Analog>(serializedOrignal);
            Assert.IsTrue(original.Equals(copy));
        }

        [Test]
        public static void TestDigitalSerialization() {
            var original = new Digital("H", (int)ModKey.RightAlt);
            Assert.IsFalse(original.Equals(new Digital("A")));
            var serializedOrignal = JsonConvert.SerializeObject(original);
            var copy = JsonConvert.DeserializeObject<Digital>(serializedOrignal);
            Assert.IsTrue(original.Equals(copy));
        }

        [Test]
        public static void TestListOfInputsSerialization() {
            var original = new List<Analog>();
            original.Add(new Analog("Joystick 1 Axis 5", false));
            original.Add(new Digital("A", (int)ModKey.LeftControl));
            var data = new List<InputData>();
            original.ForEach(x => {
                data.Add(new InputData {
                    Type = x.GetType(),
                    Data = JsonConvert.SerializeObject(x)
                });
            });
            var serializedOrignal = JsonConvert.SerializeObject(data);
            var copy = JsonConvert.DeserializeObject<List<InputData>>(serializedOrignal);
            var corrected = new List<Analog>();
            var deserializeMethod = typeof(JsonConvert).GetMethods().First(y => y.IsGenericMethod && y.Name.Equals("DeserializeObject"));
            copy.ForEach(x => {
                corrected.Add((Analog)deserializeMethod.MakeGenericMethod(x.Type).Invoke(null, new string[] { x.Data }));
            });
            for (int i = 0; i < copy.Count; i++) {
                Assert.IsTrue(corrected[i].Equals(original[i]));
                Assert.IsTrue(corrected[i].GetType().FullName == original[i].GetType().FullName);
            }
        }

        [JsonObject]
        public struct InputData {
            public Type Type;
            public string Data;
        }
    }
}
