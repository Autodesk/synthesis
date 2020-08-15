// using System.Collections;
// using NUnit.Framework;
// using UnityEngine;
// using UnityEngine.TestTools;
// using Engine.ModuleLoader;
// using System.IO;
// using System.IO.Compression;
// using System;
// using UnityEngine.SceneManagement;
// using SynthesisAPI.AssetManager;
//
// namespace Tests
// {
//     public class TestModuleLoader : IPrebuildSetup
//     {
//         private static readonly string sourcePath = SynthesisAPI.VirtualFileSystem.FileSystem.BasePath + "modules" + Path.DirectorySeparatorChar;
//         private static readonly string zipPath = sourcePath + "test_module.zip";
//
//         private static readonly string TestTextFileName = "test.txt";
//         private static readonly string TestTextFileFolder = "test_dir";
//         private static readonly string TestTextFileContents = "Hello world!";
//
//         private static readonly ModuleMetadata TestModuleMetadata = new ModuleMetadata("Test Module", "0.1.0", "test_module",
//             manifest: new[] { TestTextFileFolder + SynthesisAPI.VirtualFileSystem.Directory.DirectorySeparatorChar + TestTextFileName });
//
//         private static void CreateTestModule()
//         {
//             if (File.Exists(zipPath))
//             {
//                 Debug.LogWarning($"Test module path {zipPath} already exists, deleting it.");
//                 File.Delete(zipPath);
//                 if (File.Exists(zipPath))
//                 {
//                     throw new Exception($"Test module already exists: {zipPath}");
//                 }
//             }
//
//             string sourceFolderPath = sourcePath + TestModuleMetadata.TargetPath;
//             Directory.CreateDirectory(sourceFolderPath);
//             string textFileLoc = sourceFolderPath + Path.DirectorySeparatorChar + TestTextFileFolder;
//             Directory.CreateDirectory(textFileLoc);
//
//             Stream metadataFile = File.Open(sourceFolderPath + Path.DirectorySeparatorChar + ModuleMetadata.MetadataFilename, FileMode.OpenOrCreate);
//             TestModuleMetadata.Serialize(metadataFile);
//             metadataFile.Close();
//
//             Stream textFile = File.Open(textFileLoc + Path.DirectorySeparatorChar + TestTextFileName, FileMode.OpenOrCreate);
//             var writer = new StreamWriter(textFile);
//             writer.Write(TestTextFileContents);
//             writer.Flush();
//             textFile.Close();
//
//             ZipFile.CreateFromDirectory(sourceFolderPath, zipPath);
//
//             Directory.Delete(sourceFolderPath, true);
//         }
//     
//         [SetUp]
//         public void Setup() {
//             CreateTestModule();
//         }
//
//         [TearDown]
//         public void Teardown()
//         {
//             if (File.Exists(zipPath))
//             {
//                 File.Delete(zipPath);
//             }
//         }
//
//         public class ApiTest : MonoBehaviour, IMonoBehaviourTest
//         {
//             public bool IsTestFinished { get; private set; }
//
//             private DateTime endTime;
//
//             public const int Timeout = 10000; // ms
//
//             public void Awake()
//             {
//                 endTime = DateTime.Now.AddMilliseconds(Timeout);
//             }
//
//             public void Update()
//             {
//                 if(DateTime.Now > endTime)
//                 {
//                     IsTestFinished = true;
//                     throw new Exception("Unity test timeout");
//                 }
//
//                 foreach (var e in SynthesisAPI.Modules.ModuleManager.GetLoadedModules())
//                 {
//                     Debug.Log("Loaded module: " + e);
//                 }
//
//                 if (SynthesisAPI.Modules.ModuleManager.IsFinishedLoading)
//                 {
//                     var hasTestModule = SynthesisAPI.Modules.ModuleManager.GetLoadedModules().Contains(new SynthesisAPI.Modules.ModuleManager.ModuleInfo(TestModuleMetadata.Name, TestModuleMetadata.Version));
//                     var textAsset = AssetManager.GetAsset<SynthesisAPI.AssetManager.TextAsset>(
//                         $"/modules/{TestModuleMetadata.TargetPath}/{TestTextFileFolder}/{TestTextFileName}");
//                     var hasTextContents = textAsset != null && textAsset.ReadToEnd() == TestTextFileContents;
//
//                     IsTestFinished = hasTestModule && hasTextContents;
//                 }
//             }
//         }
//
//         [UnityTest]
//         public IEnumerator TestLoadModule()
//         {
//             SceneManager.LoadScene("MainSim", LoadSceneMode.Single);
//             yield return null;
//
//             var test = new MonoBehaviourTest<ApiTest>();
//             yield return test;
//         }
//     }
//
// }
