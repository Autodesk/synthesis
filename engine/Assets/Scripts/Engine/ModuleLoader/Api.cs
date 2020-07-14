using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Component = SynthesisAPI.EnvironmentManager.Component;
using Debug = UnityEngine.Debug;

using System.IO.Compression;
using Assets.Scripts.Engine.Util;
using SynthesisAPI.AssetManager;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EventBus;
using SynthesisAPI.Modules;
using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.Runtime;
using SynthesisAPI.Utilities;
using SynthesisAPI.VirtualFileSystem;
using Directory = System.IO.Directory;

using PreloadedModule = System.ValueTuple<System.IO.Compression.ZipArchive, Engine.ModuleLoader.ModuleMetadata>;
using Core.ModuleLoader;

namespace Engine.ModuleLoader
{
    public class Api : MonoBehaviour
	{
		private static readonly string ModulesSourcePath = FileSystem.BasePath + "modules";
		private static readonly string BaseModuleTargetPath = SynthesisAPI.VirtualFileSystem.Directory.DirectorySeparatorChar + "modules";

		public void Awake()
		{
			SynthesisAPI.Runtime.ApiProvider.RegisterApiProvider(new ApiProvider());
			LoadApi();
			LoadModules();
		}

		private void LoadModules()
        {
			if (!Directory.Exists(ModulesSourcePath))
			{
				Directory.CreateDirectory(ModulesSourcePath);
			}

			var modules = PreloadModules();
			ResolveDependencies(modules);

			foreach (var (arhive, metadata) in modules)
			{
				LoadModule((arhive, metadata));
				EventBus.Push(new LoadModuleEvent(metadata.Name));
				ModuleManager.AddToLoadedModuleList(metadata.Name);
				// Debug.Log("Loaded " + metadata.Name);
			}
			ModuleManager.MarkFinishedLoading();
		}

		private List<PreloadedModule> PreloadModules()
        {
			var modules = new List<PreloadedModule>();

			// Discover and preload all modules
			foreach (var file in Directory.GetFiles(ModulesSourcePath).Where(fn => Path.GetExtension(fn) == ".zip"))
			{
				var module = PreloadModule(Path.GetFileName(file));
				if (module is null)
					continue;
				foreach (var (_, metadata) in modules)
				{
					if (metadata.Name == module?.Item2.Name)
					{
						throw new LoadModuleException($"Attempting to load module with duplicate name: {metadata.Name}");
					}
					if (metadata.TargetPath == module?.Item2.TargetPath)
					{
						throw new LoadModuleException($"Attempting to load modules into same target path: {metadata.TargetPath}");
					}
				}
				modules.Add(module.Value);
			}
			return modules;
		}

        private PreloadedModule? PreloadModule(string filePath)
        {
			var fullPath = $"{ModulesSourcePath}{Path.DirectorySeparatorChar}{filePath}";

			var module = ZipFile.Open(fullPath, ZipArchiveMode.Read);

			// Ensure module contains metadata
			if (module.Entries.All(e => e.Name != ModuleMetadata.MetadataFilename))
            {
				Debug.LogWarning($"Potential module missing is metadata file: {filePath}");
				return null;
            }

			// Parse module metadata
			try
			{
				var metadata = ModuleMetadata.Deserialize(module.Entries
					.First(e => e.Name == ModuleMetadata.MetadataFilename).Open());
				return (module, metadata);
			}
			catch (Exception e)
			{
				throw new LoadModuleException($"Failed to deserialize metadata in module: {fullPath}", e);
			}
        }

        private void ResolveDependencies(List<(ZipArchive archive, ModuleMetadata metadata)> moduleList)
        {
			// Use Kahns algorithm to resolve module dependencies, ordering modules in list
			// in the order they should be loaded

			// TODO check for cyclic dependencies and throw
			var resolvedEntries = moduleList.Where(t => !t.metadata.Dependencies.Any()).ToList();
            var solutionSet = new Queue<(ZipArchive archive, ModuleMetadata metadata)>();
            while (resolvedEntries.Count > 0)
            {
                var element = resolvedEntries.PopAt(0);
                solutionSet.Enqueue(element);
                foreach (var dep in moduleList.Where(t =>
	                t.metadata.Dependencies.Contains(element.metadata.Name)).ToList())
                {
	                dep.metadata.Dependencies.Remove(element.metadata.Name);
					if(dep.metadata.Dependencies.Count == 0)
						resolvedEntries.Add(dep);
                }
            }

            moduleList.Clear();
			moduleList.AddRange(solutionSet.ToList());
        }

        private void LoadModule((ZipArchive archive, ModuleMetadata metadata) moduleInfo)
        {
			var fileManifest = new List<string>();
			fileManifest.AddRange(moduleInfo.metadata.FileManifest);

			foreach (var entry in moduleInfo.archive.Entries.Where(e =>
				e.Name != ModuleMetadata.MetadataFilename && moduleInfo.metadata.FileManifest.Contains(e.Name)))
			{

				fileManifest.Remove(entry.Name);
				var extension = Path.GetExtension(entry.Name);
				var stream = entry.Open();
				if (extension == ".dll")
				{
					if (!LoadModuleAssembly(stream))
					{
						throw new LoadModuleException($"Failed to load assembly: {entry.Name}");
					}
				}
				else
				{
					var targetPath = BaseModuleTargetPath + SynthesisAPI.VirtualFileSystem.Directory.DirectorySeparatorChar + moduleInfo.metadata.TargetPath;
					var perm = Permissions.PublicReadWrite;
					if (AssetManager.Import(AssetManager.GetTypeFromFileExtension(extension),
						new DeflateStreamWrapper(stream, entry.Length), targetPath, entry.Name, perm, "") == null)
					{
						throw new LoadModuleException($"Failed to import asset: {entry.Name}");
					}
				}
				// Debug.Log("Loaded " + entry.Name);
			}
			foreach(var file in fileManifest)
            {
				Debug.LogWarning($"Module \"{moduleInfo.metadata.Name}\" is missing file from manifest: {file}");
            }
			moduleInfo.archive.Dispose();
		}

		private bool LoadApi()
        {
			// Set up Api
			var apiAssembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Api");
			foreach (var type in apiAssembly.GetTypes().Where(e => e.IsSubclassOf(typeof(SystemBase))))
			{
				var entity = EnvironmentManager.AddEntity();
				entity.AddComponent(type);
			}
			foreach (var type in apiAssembly.GetTypes().Where(e => e.GetMethods().Any(
					m => m.GetCustomAttribute<CallbackAttribute>() != null || m.GetCustomAttribute<TaggedCallbackAttribute>() != null)))
			{
				var instance = Activator.CreateInstance(type);
				foreach (var callback in type.GetMethods()
					.Where(m => m.GetCustomAttribute<CallbackAttribute>() != null))
				{
					RegisterTypeCallback(callback, instance);
				}
				foreach (var callback in type.GetMethods()
					.Where(m => m.GetCustomAttribute<TaggedCallbackAttribute>() != null))
				{
					RegisterTagCallback(callback, instance);
				}
			}
			return true;
		}

        private bool LoadModuleAssembly(Stream stream)
		{
			// Load module assembly
		    var memStream = new MemoryStream();
			stream.CopyTo(memStream);
			stream.Close();
			var assembly = Assembly.Load(memStream.ToArray());

			// Set up module
			foreach (var exportedModuleClass in assembly.GetTypes()
				.Where(t => t.GetCustomAttribute<ModuleExportAttribute>() != null))
			{
				var exportedModuleClassInstance = Activator.CreateInstance(exportedModuleClass);

				if (exportedModuleClass.IsSubclassOf(typeof(SystemBase)))
				{
					var entity = EnvironmentManager.AddEntity();
					entity.AddComponent(exportedModuleClass);
				}

				foreach (var callback in exportedModuleClass.GetMethods()
					.Where(m => m.GetCustomAttribute<CallbackAttribute>() != null))
				{
					RegisterTypeCallback(callback, exportedModuleClassInstance);
				}

				foreach (var callback in exportedModuleClass.GetMethods()
					.Where(m => m.GetCustomAttribute<TaggedCallbackAttribute>() != null))
				{
					RegisterTagCallback(callback, exportedModuleClassInstance);
				}
			}
			return true;
		}

		private void RegisterTagCallback(MethodInfo callback, object instance)
		{
			if (instance.GetType() != callback.DeclaringType) // Sanity check
			{
				throw new LoadModuleException(
					$"Type of instance variable \"{instance.GetType()}\" does not match declaring type of callback \"{callback.Name}\" (expected \"{callback.DeclaringType}\"");
			}
			var eventType = callback.GetParameters().First().ParameterType;
			var tag = callback.GetCustomAttribute<TaggedCallbackAttribute>().Tag;
			typeof(EventBus).GetMethod("NewTagListener").Invoke(null, new object[]
				{
					tag,
					CreateEventCallback(callback, instance, eventType)
				});
		}

		private void RegisterTypeCallback(MethodInfo callback, object instance)
        {
	        if (instance.GetType() != callback.DeclaringType) // Sanity check
			{
				throw new LoadModuleException(
					$"Type of instance variable \"{instance.GetType()}\" does not match declaring type of callback \"{callback.Name}\" (expected \"{callback.DeclaringType}\"");
	        }
	        var eventType = callback.GetParameters().First().ParameterType;
			typeof(EventBus).GetMethod("NewTypeListener")
				?.MakeGenericMethod(eventType).Invoke(null, new object[]
				{
					CreateEventCallback(callback, instance, eventType)
				});
        }

        EventBus.EventCallback CreateEventCallback(MethodInfo m, object instance, Type eventType)
        {
	        return (e) => m.Invoke(instance,
		        new []
		        {
			        typeof(ReflectHelper).GetMethod("CastObject")
				        ?.MakeGenericMethod(eventType)
				        .Invoke(null, new object[] {e})
		        });
        }

		private class ApiProvider : IApiProvider
		{
			private GameObject _entityParent;
			private Dictionary<uint, GameObject> _gameObjects;
			private readonly Dictionary<Type, Type> _builtins;

			public ApiProvider()
			{
				_entityParent = new GameObject("Entities");
				_gameObjects = new Dictionary<uint, GameObject>();
				_builtins = new Dictionary<Type, Type>();
				_builtins.Add(typeof(MeshAdapter), typeof(Mesh));
			}

			public void Log(object o)
			{
				Debug.Log(o);
			}

			public void AddEntityToScene(uint entity)
			{
				if (_gameObjects.ContainsKey(entity))
					throw new Exception($"Entity \"{entity}\" already exists");
				var gameObject = new GameObject($"Entity {entity >> 16}"); // TODO replace with entity.GetId()
				gameObject.transform.SetParent(_entityParent.transform);
				_gameObjects.Add(entity, gameObject);
			}

			public void RemoveEntityFromScene(uint entity)
			{
				GameObject gameObject;
				if (!_gameObjects.TryGetValue(entity, out gameObject))
					throw new Exception($"Entity \"{entity}\" does not exist");
				Destroy(gameObject);
			}

			public Component AddComponentToScene(uint entity, Type t)
			{
				GameObject gameObject;
				if (!_gameObjects.TryGetValue(entity, out gameObject))
					throw new Exception($"Entity \"{entity}\" does not exist");
				dynamic component;
				Type type;
				if (_builtins.ContainsKey(t))
				{
					type = _builtins[t];
					component = type.GetMethod("NewInstance")?.Invoke(null, null);
					if (component == null)
					{
						throw new Exception("Builtin type lacked way to create new instance");
					}
				}
				else if (t.IsSubclassOf(typeof(SystemBase)))
				{
					component = (SystemBase) Activator.CreateInstance(t);
					type = typeof(SystemMonoBehavior);
				}
				else
				{
					component = (Component) Activator.CreateInstance(t);
					type = typeof(ComponentAdapter);
				}

				gameObject.AddComponent(type);
				dynamic gameObjectComponent = gameObject.GetComponent(type);
				gameObjectComponent.SetInstance(component);

				return component;
			}

			public void RemoveComponentFromScene(uint entity, Type t)
            {
				GameObject gameObject;
				if (!_gameObjects.TryGetValue(entity, out gameObject))
					throw new Exception($"Entity \"{entity}\" does not exist");
				Type type;
				if (_builtins.ContainsKey(t))
				{
					type = _builtins[t];
				}
				else if (t.IsSubclassOf(typeof(SystemBase)))
				{
					type = typeof(SystemMonoBehavior);
				}
				else
				{
					type = typeof(ComponentAdapter);
				}
				Destroy(gameObject.GetComponent(type));
			}
		}
	}
}