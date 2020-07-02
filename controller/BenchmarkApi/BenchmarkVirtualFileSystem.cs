using BenchmarkDotNet.Attributes;
using SynthesisAPI.VirtualFileSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BenchmarkApi
{
    [MemoryDiagnoser]
    [Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class BenchmarkVirtualFileSystem
    {
        // public property
        public IEnumerable<string> ValuesForTestPath()
        {
            string path = "/temp";
            int inc = 5;
            for (var i = FileSystem.DepthOfPath(path); i < FileSystem.MaxDirectoryDepth; i += inc)
            {
                path += String.Concat(Enumerable.Repeat("/newDir", inc));
                yield return path;
            }
        }

        [ParamsSource(nameof(ValuesForTestPath))]
        public string TestPath { get; set; }

        [GlobalSetup]
        public void Setup() {
            string path = "/temp";
            for (var i = FileSystem.DepthOfPath(path); i < FileSystem.MaxDirectoryDepth; i++)
            {
                path += "/newDir";
            }
            FileSystem.CreatePath(path, Permissions.PublicReadWrite);
        }

        [Benchmark]
        public void MaximumDepthTraversal()
        {
            var entry = FileSystem.Traverse(TestPath);
        }
    }
}
