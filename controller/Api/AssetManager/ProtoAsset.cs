using System;
using System.Dynamic;
using System.IO;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Synthesis.Importbuf;
using SynthesisAPI.VirtualFileSystem;

namespace SynthesisAPI.AssetManager
{
    // path is the source to the protobuf file
    // ProtoAsset reads the data
    public class ProtoAsset<T> : Asset where T : Google.Protobuf.IMessage<T>
    {

        public ProtoAsset(string name, Guid owner, Permissions permissions, string path)
        {
            Init(name, owner, permissions, path);
        }

        //public static Document GetProtoFile(string path)
        //{
        //    FileStream fs = new FileStream(path, FileMode.Open);
        //    Document protoObject = Document.Parser.ParseFrom(fs);
        //    Console.Write(fs);
        //    fs.Close();
        //    return protoObject;
        //}

        public override IEntry Load(byte[] data)
        {
            var stream = new MemoryStream();
            stream.Write(data, 0, data.Length);
            stream.Position = 0;
            Document protoObj = Document.Parser.ParseFrom(stream);
        }
    }
}
