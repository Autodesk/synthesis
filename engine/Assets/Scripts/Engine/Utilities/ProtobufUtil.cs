using System;
using System.Collections.Generic;
using Synthesis.Simulator;
using UnityEngine;
using System.IO;

namespace Synthesis.Util
{
    public class ProtobufUtil
    {
        public static ProtoField GetFieldFromFile(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            ProtoField field = ProtoField.Parser.ParseFrom(fs);
            fs.Close();
            return field;
        }
    }
}