using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirabuf;
using System.IO;
using System.IO.Compression;
using System;
using Google.Protobuf;

namespace Synthesis.Import {
    public class MirabufLive {

        private string _path;
        public Assembly MiraAssembly;

        public MirabufLive(string path) {
            _path = path;
            byte[] buff = File.ReadAllBytes(path);

			if (buff[0] == 0x1f && buff[1] == 0x8b) {

				int originalLength = BitConverter.ToInt32(buff, buff.Length - 4);

				MemoryStream mem = new MemoryStream(buff);
				byte[] res = new byte[originalLength];
				GZipStream decompresser = new GZipStream(mem, CompressionMode.Decompress);
				decompresser.Read(res, 0, res.Length);
				decompresser.Close();
				mem.Close();
				buff = res;
			}

            MiraAssembly = Assembly.Parser.ParseFrom(buff);
        }

        public void Save() {
            if (MiraAssembly == null) {
                File.Delete(_path);
                return;
            }
            byte[] buff = new byte[MiraAssembly.CalculateSize()];
            MiraAssembly.WriteTo(new CodedOutputStream(buff));
            string backupPath = $"{_path}.bak";
            File.Move(_path, backupPath);
            File.WriteAllBytes(_path, buff);
        }
    }
}
