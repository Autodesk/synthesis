using BenchmarkDotNet.Attributes;
using SynthesisAPI.AssetManager;
using SynthesisAPI.Utilities;
using SynthesisAPI.VirtualFileSystem;
using System;
using System.IO;
using System.Linq;

namespace BenchmarkApi
{
    [MemoryDiagnoser]
    [Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class BenchmarkAssetManager
    {
        private static readonly string LargeTextFileName = "test_large.txt";

        [GlobalSetup]
        public void Setup()
        {

            if (!System.IO.Directory.Exists(FileSystem.TestPath))
            {
                System.IO.Directory.CreateDirectory(FileSystem.TestPath);
            }

            string text_string = String.Concat(Enumerable.Repeat("Hello ", 10000));

            if (!File.Exists(FileSystem.TestPath + LargeTextFileName))
            {
                File.WriteAllText(FileSystem.TestPath + LargeTextFileName, text_string);
            }
        }

        [Benchmark]
        public void DirectLoadLargeTextAsset()
        {
            byte[] data = File.ReadAllBytes(FileSystem.TestPath + LargeTextFileName);
        }

        [Benchmark]
        public void OpenLargeTextAssetStream()
        {
            FileStream stream = File.Open(FileSystem.TestPath + LargeTextFileName, FileMode.Open);
            stream.Close();
        }

        private static System.Threading.ReaderWriterLockSlim RWLock = new System.Threading.ReaderWriterLockSlim();

        [Benchmark]
        public void LargeTextAssetSharedStream()
        {
            FileStream stream = File.Open(FileSystem.TestPath + LargeTextFileName, FileMode.Open);
            SharedBinaryStream sharedStream = new SharedBinaryStream(stream, RWLock);
            byte[] data = sharedStream.ReadToEnd();
            stream.Close();
        }

        [Benchmark]
        public void ImportLargeTextAsset()
        {
            string fileName = FileSystem.TestPathLocal + LargeTextFileName;
            string type = AssetManager.GetTypeFromFileExtension(fileName);
            AssetManager.Import(type, false, "/temp/", LargeTextFileName, Permissions.PublicReadWrite, fileName).Delete();
        }

        /*
        [Benchmark]
        public void ImportLargeTextAssetLazy()
        {
            AssetManager.Import("temp/", "test_large.txt", Permissions.PrivateReadWrite, "");
        }
        */
    }
}
