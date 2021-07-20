using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Google.Protobuf;

namespace SynthesisAPI.Proto {
    /// <summary>
    /// Partial class to add utility functions and properties to Protobuf types
    /// </summary>
    public partial class DynamicObject : IMessage<DynamicObject> {

        public Node this[ProtoGuid guid] {
            get {
                foreach (var n in Nodes) {
                    if (n.Guid.Equals(guid))
                        return n;
                }
                return null;
            }
        }

        public string Serialize(string outputDir, string name) {
            string outputPath;
            if (outputDir == null) {
                string tempPath = Path.GetTempPath() + Path.AltDirectorySeparatorChar + "synth_temp";
                if (!Directory.Exists(tempPath))
                    Directory.CreateDirectory(tempPath);
                outputPath = tempPath + Path.AltDirectorySeparatorChar + $"{name}.dyno";
            } else {
                if (!Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);
                outputPath = outputDir + Path.AltDirectorySeparatorChar + $"{name}.dyno";
            }
            var stream = File.Create(outputPath);
            this.WriteTo(stream); // Try just using any old stream???
            stream.Close();
            return outputPath;
        }

    }
}
