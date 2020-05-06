using System;
using System.Collections.Generic;
using System.Text;

namespace SynthesisAPI
{
    public class ObjectManager
    {
        public static void CreateObject(string n) => EngineGateway.handle.CreateObject(n);
    }
}
