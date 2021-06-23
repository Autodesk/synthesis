using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Google.Protobuf;
using Synthesis.Import;
using UnityEngine;

namespace Synthesis.Proto {
    /// <summary>
    /// Partial class to add utility functions and properties to Protobuf types
    /// </summary>
    public partial class ProtoField: IMessage<ProtoField> {
        public string Serialize(string outputDir) {
            string outputPath;
            if (outputDir == null) {
                string tempPath = Path.GetTempPath() + Path.AltDirectorySeparatorChar + "synth_temp";
                if (!Directory.Exists(tempPath))
                    Directory.CreateDirectory(tempPath);
                outputPath = tempPath + Path.AltDirectorySeparatorChar + $"{Name}.spf";
            } else {
                if (!Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);
                outputPath = outputDir + Path.AltDirectorySeparatorChar + $"{Name}.spf";
            }
            var stream = File.Create(outputPath);
            this.WriteTo(stream); // Try just using any old stream???
            stream.Close();
            return outputPath;
        }
        
        public static GameObject ImportFromFile(string path, Translator.TranslationFunc transFunc = null, bool forceTranslation = false)
            => Importer.Import(path, Importer.SourceType.PROTOBUF_FIELD, transFunc, forceTranslation);
    }
}
