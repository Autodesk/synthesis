/*using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Google.Protobuf;
using SynthesisAPI.Translation;
using UnityEngine;

namespace SynthesisAPI.Proto {
    /// <summary>
    /// Partial class to add utility functions and properties to Protobuf types
    /// </summary>
    public partial class ProtoField: IMessage<ProtoField> {
        public const string FILE_ENDING = "spf";
        
        public string Serialize(string outputDir) {
            string outputPath;
            
            var ms = new MemoryStream();
            this.WriteTo(ms);
            int size = this.CalculateSize();
            ms.Seek(0, SeekOrigin.Begin);
            byte[] content = new byte[size];
            ms.Read(content, 0, size);
            
            if (outputDir == null) {
                string tempPath = Path.GetTempPath() + Path.AltDirectorySeparatorChar + "synth_temp";
                if (!Directory.Exists(tempPath))
                    Directory.CreateDirectory(tempPath);
                outputPath = tempPath + Path.AltDirectorySeparatorChar + $"{Translator.TempFileName(content)}.{FILE_ENDING}";
            } else {
                if (!Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);
                outputPath = outputDir + Path.AltDirectorySeparatorChar + $"{Object.Name}.{FILE_ENDING}";
            }
            var stream = File.Create(outputPath);
            // this.WriteTo(stream); // Try just using any old stream???
            stream.Write(content, 0, content.Length);
            stream.Close();
            return outputPath;
        }
    }
}
*/