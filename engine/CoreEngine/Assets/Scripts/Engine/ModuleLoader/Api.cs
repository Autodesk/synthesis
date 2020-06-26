using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Core.ModuleLoader;
using Synthesis.Core.ModuleLoader;
using SynthesisAPI.Modules;
using SynthesisAPI.Runtime;
using UnityEngine;
using Component = SynthesisAPI.Modules.Component;
using Debug = UnityEngine.Debug;
using Object = SynthesisAPI.Modules.Object;

using System.IO.Compression;
using System.Text;
using System.Xml.Serialization;
using Assets.Scripts.Engine.Util;
using JetBrains.Annotations;
using SynthesisAPI.AssetManager;
using SynthesisAPI.VirtualFileSystem;
using UnityEditor;
using Directory = System.IO.Directory;
using TextAsset = UnityEngine.TextAsset;

using PreloadedModule = System.ValueTuple<System.IO.Compression.ZipArchive, Engine.ModuleLoader.ModuleMetadata>;

namespace Engine.ModuleLoader
{
	public class Api : MonoBehaviour
	{
		public void Awake()
		{
			SynthesisAPI.Runtime.ApiProvider.RegisterApiProvider(new ApiProvider()); 
            var modules = new List<(ZipArchive, ModuleMetadata)>();
			if (!Directory.Exists(FileSystem.BasePath + "modules"))
			{
				Directory.CreateDirectory(FileSystem.BasePath + "modules");
			}
            foreach (var file in Directory.GetFiles(FileSystem.BasePath + "modules"))
            {
                (ZipArchive module, ModuleMetadata metadata) =
	                PreloadModule($"{Path.DirectorySeparatorChar}{file.Split(Path.DirectorySeparatorChar).Last()}") ??
	                (null, null);
                if (module is null)
                    continue;
				modules.Add((module, metadata));
            }

            ResolveDependencies(modules);
			foreach(var (archive, metadata) in modules) {
				foreach (var entry in archive.Entries)
				{

					if (entry.Name == ModuleMetadata.MetadataFilename)
						continue;
					if (!metadata.FileManifest.Contains(entry.Name))
						continue;

					var extension = Path.GetExtension(entry.Name); // Path.GetExtension
					string targetPath = "modules/" + metadata.TargetPath;
					Stream stream = entry.Open();
					Permissions perm = Permissions.PublicReadWrite;
					if (extension == "dll")
					{
						if (!LoadAssembly(stream))
						{
							Debug.Log($"Failed to load assembly {entry.Name}");
						}

						break;
					}
					else
					{
						if (AssetManager.Import(entry.Open(), targetPath, entry.Name, Guid.Empty, perm,
							"") == null)
						{
							throw new Exception("Asset module type");
						}
					}
				}
            }
		}

        private (ZipArchive, ModuleMetadata)? PreloadModule(string filePath)
        {
            var module = ZipFile.Open(FileSystem.BasePath + Path.DirectorySeparatorChar + "modules" + filePath, ZipArchiveMode.Read);
            if (module.Entries.All(e => e.Name != ModuleMetadata.MetadataFilename))
            {
                return null;
            }

            var metadata = ModuleMetadata.Parse(module.Entries
                    .First(e => e.Name == ModuleMetadata.MetadataFilename).Open());
			return (module, metadata);
        }

		// Kahns algorithm
        private void ResolveDependencies(List<(ZipArchive archive, ModuleMetadata metadata)> moduleList)
        {
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

        private bool LoadAssembly(Stream stream)
		{
			var assembly = Assembly.Load(Encoding.UTF8.GetBytes(new StreamReader(stream).ReadToEnd()));
			var plugins = assembly.GetExportedTypes().Where(t => typeof(IModule).IsAssignableFrom(t)).ToList();
			foreach (var module in plugins)
			{
				SynthesisAPI.Runtime.ApiProvider.RegisterModule((IModule) Activator.CreateInstance(module));
				var exports = IModuleExtensions.ExportedTypes(module);
				if (exports != null)
				{
					foreach (var export in exports)
					{
						if (SynthesisAPI.Utilities.ReflectHelper.IsSubclassOfRawGeneric(typeof(GlobalBehavior<>),
							export))
						{
							Activator.CreateInstance(export);
						}
					}
				}
			}
			return true;
		}

		private class ApiProvider : IApiProvider
		{
			private readonly Dictionary<Guid, Object> _objects;
			private readonly Dictionary<Guid, GameObject> _gameObjects;
			private readonly Dictionary<Type, Type> _builtins;

			private readonly List<IModule> _modules;

			private readonly MultiMap<Guid, Component> _components;

			public ApiProvider()
			{
				_objects = new Dictionary<Guid, Object>();
				_gameObjects = new Dictionary<Guid, GameObject>();
				_components = new MultiMap<Guid, Component>();
				_modules = new List<IModule>();
				_builtins = new Dictionary<Type, Type>
				{
					{typeof(Mesh), typeof(MeshAdapter)}
				};
			}

			public (Guid Id, bool valid) Instantiate(Object o)
			{
				var id = Guid.NewGuid();
				var newGameObject = new GameObject(id.ToString());
				_objects.Add(id, o);
				_gameObjects.Add(id, newGameObject);
				return (id, true);
			}

			public void Log(object o)
			{
				Debug.Log(o);
			}

			public Component AddComponent(Type t, Guid objectId)
			{
				var gameObject = _gameObjects[objectId];
				if (gameObject == null)
				{
					throw new Exception($"No GameObject exists with id \"{objectId}\"");
				}
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
				} else if (t.IsSubclassOf(typeof(Behavior)))
				{
					component = (Behavior) Activator.CreateInstance(t);
					type = typeof(BehaviorAdapter);
				}
				else
				{
					component = (Component) Activator.CreateInstance(t);
					type = typeof(ComponentAdapter);
				}

				gameObject.AddComponent(type);
				dynamic gameObjectComponent = gameObject.GetComponent(type);
				gameObjectComponent.SetInstance(component);
				_components.Add(objectId, component);
				return component;
			}

			public TComponent AddComponent<TComponent>(Guid objectId) where TComponent : Component
			{
				return (TComponent) AddComponent(typeof(TComponent), objectId);
			}

			public Component GetComponent(Type t, Guid id)
			{
				return GetComponents(id).First(c => c.GetType() == t);
			}

			public TComponent GetComponent<TComponent>(Guid id) where TComponent : Component
			{
				return GetComponents<TComponent>(id).First();
			}

			public List<Component> GetComponents(Guid objectId)
			{
				return _components[objectId];
			}

			public List<TComponent> GetComponents<TComponent>(Guid id) where TComponent : Component
			{
				return _components[id].OfType<TComponent>().ToList();
			}

			public Object GetObject(Guid objectId)
			{
				return _objects[objectId];
			}

			public List<IModule> GetModules()
			{
				return _modules;
			}

			public void RegisterModule(IModule module)
			{
				if (_modules.Any(m => module.GetType().IsInstanceOfType(m)))
				{
					throw new Exception($"Duplicate entry of {module.GetType().Name} was found");
				}
				_modules.Add(module);
			}

			public Transform GetTransformById(Guid objectId)
			{
				return _gameObjects[objectId].transform;
			}
		}
	}
}