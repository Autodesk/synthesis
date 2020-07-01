using System;
using System.IO;
using System.IO.Compression;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Engine.ModuleLoader;

namespace Tests
{
    public class TestModuleLoader : IPrebuildSetup
    {
        private static readonly ModuleMetadata TestModuleMetadata = new ModuleMetadata("Test Module", "0.1.0", "test_module");

        /*
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
    */
        [SetUp]
        public void Setup() { }


        public class ApiTest : MonoBehaviour, IMonoBehaviourTest
        {
            public bool IsTestFinished
            {
                get => true;//SynthesisAPI.Modules.ModuleManager.IsFinishedLoading;
            }
        }

        [UnityTest]
        public IEnumerator TestLoadModule()
        {
            // TODO get this to work
            //yield return new WaitForFixedUpdate();
            yield return new WaitForSecondsRealtime(10);
            yield return new MonoBehaviourTest<ApiTest>();
            //Assert.True(SynthesisAPI.Modules.ModuleManager.IsModuleLoaded(TestModuleMetadata.Name));
        }
    }

}
