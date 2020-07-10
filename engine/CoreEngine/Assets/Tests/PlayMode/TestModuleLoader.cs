using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Engine.ModuleLoader;
using System.IO;
using System.IO.Compression;
using System;
using UnityEngine.SceneManagement;

namespace Tests
{
    public class TestModuleLoader : IPrebuildSetup
    {
        private static readonly ModuleMetadata TestModuleMetadata = new ModuleMetadata("Test Module", "0.1.0", "test_module");

        private static void CreateTestModule()
        {

            string sourcePath = SynthesisAPI.VirtualFileSystem.FileSystem.BasePath + "modules" + Path.DirectorySeparatorChar;
            string zipPath = sourcePath + "test_module.zip";
            if (File.Exists(zipPath))
            {
                return;
            }
            string sourceFolderPath = sourcePath + "test_module";
            Directory.CreateDirectory(sourceFolderPath);

            Stream metadataFile = File.Open(sourceFolderPath + Path.DirectorySeparatorChar + ModuleMetadata.MetadataFilename, FileMode.OpenOrCreate);
            
            TestModuleMetadata.Serialize(metadataFile);

            metadataFile.Close();

            ZipFile.CreateFromDirectory(sourceFolderPath, zipPath);

            Directory.Delete(sourceFolderPath, true);
        }
    
        [SetUp]
        public void Setup() {
            CreateTestModule();
        }

        public class ApiTest : MonoBehaviour, IMonoBehaviourTest
        {
            public bool IsTestFinished { get; private set; }

            private DateTime endTime;

            public const int Timeout = 10000; // ms

            public void Awake()
            {
                endTime = DateTime.Now.AddMilliseconds(Timeout);
            }

            public void Update()
            {
                if(DateTime.Now > endTime)
                {
                    IsTestFinished = true;
                    throw new Exception("Unity test timeout");
                }

                foreach (var e in SynthesisAPI.Modules.ModuleManager.GetLoadedModules())
                {
                    Debug.Log("Loaded module: " + e);
                }
                IsTestFinished = SynthesisAPI.Modules.ModuleManager.IsFinishedLoading;
            }
        }

        [UnityTest]
        public IEnumerator TestLoadModule()
        {
            SceneManager.LoadScene("MainSim", LoadSceneMode.Single);
            yield return null;

            var test = new MonoBehaviourTest<ApiTest>();
            yield return test;
        }
    }

}
