using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Unity.UIElements.Runtime;
using UnityEngine.UIElements;
using Directory = System.IO.Directory;

using Engine.ModuleLoader.Adapters;

using PreloadedModule = System.ValueTuple<System.IO.Compression.ZipArchive, Engine.ModuleLoader.ModuleMetadata>;
using System.Threading.Tasks;

namespace Engine.ModuleLoader
{
	public class Api : MonoBehaviour
	{
		private static readonly string ModulesSourcePath = FileSystem.BasePath + "modules";
		private static readonly string BaseModuleTargetPath = SynthesisAPI.VirtualFileSystem.Directory.DirectorySeparatorChar + "modules";

		private static Dictionary<string, string> assemblyOwners = new Dictionary<string, string>();

		private static MemoryStream newConsoleStream = new MemoryStream();
		private static long lastConsoleStreamPos = 0;

		public void Awake()
		{
			assemblyOwners.Add(Assembly.GetExecutingAssembly().GetName().Name, "CoreEngine");
			SynthesisAPI.Runtime.ApiProvider.RegisterApiProvider(new ApiProvider());
			LoadApi();
			LoadModules();
			RerouteConsoleOutput();
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
				SynthesisAPI.Runtime.ApiProvider.Log($"Potential module missing is metadata file: {filePath}", LogLevel.Warning);
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
			foreach (var (_, metadata) in moduleList)
			{
				foreach (var dependency in metadata.Dependencies)
				{
					if (!moduleList.Any(m => m.metadata.Name == dependency.Name))
					{
						throw new LoadModuleException($"Module {metadata.Name} is missing dependency module {dependency.Name}");
					}
					var present_dep = moduleList.First(m => m.metadata.Name == dependency.Name);
					if (present_dep.metadata.Version != dependency.Version)
					{
						throw new LoadModuleException($"Module {metadata.Name} requires dependency module {dependency.Name} version {dependency.Version} but its version is {present_dep.metadata.Version}");
					}
				}
			}

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
					t.metadata.Dependencies.Any(d => d.Name == element.metadata.Name && d.Version == element.metadata.Version)).ToList())
				{
					dep.metadata.Dependencies.RemoveAll(d => d.Name == element.metadata.Name && d.Version == element.metadata.Version);
					if (dep.metadata.Dependencies.Count == 0)
						resolvedEntries.Add(dep);
				}
			}

			moduleList.Clear();
			moduleList.AddRange(solutionSet.ToList());
		}

		private static string GetPath(string fullName)
		{
			var i = fullName.LastIndexOf(SynthesisAPI.VirtualFileSystem.Directory.DirectorySeparatorChar, fullName.Length - 1, fullName.Length - 2);
			if (i == -1 || i == (fullName.Length - 1))
			{
				return "";
			}
			return fullName.Substring(0, i + 1);
		}

		private static string RemovePath(string metadataPath, string fullName)
		{
			if (fullName.StartsWith(metadataPath))
			{
				return fullName.Substring(metadataPath.Length);
			}
			return fullName;
		}

		private void LoadModule((ZipArchive archive, ModuleMetadata metadata) moduleInfo)
		{
			var fileManifest = new List<string>();
			fileManifest.AddRange(moduleInfo.metadata.FileManifest);

			var metadataPath = GetPath(moduleInfo.archive.Entries.First(e => e.Name == ModuleMetadata.MetadataFilename).FullName);

			foreach (var entry in moduleInfo.archive.Entries.Where(e =>
			{
				var name = RemovePath(metadataPath, e.FullName);
				return name != ModuleMetadata.MetadataFilename && moduleInfo.metadata.FileManifest.Contains(name);
			}))
			{
				fileManifest.Remove(RemovePath(metadataPath, entry.FullName));
				var extension = Path.GetExtension(entry.Name);
				var stream = entry.Open();
				if (extension == ".dll")
				{
					if (!LoadModuleAssembly(stream, moduleInfo.metadata.Name))
					{
						moduleInfo.archive.Dispose();
						throw new LoadModuleException($"Failed to load assembly: {entry.Name}");
					}
				}
				else
				{
					var targetPath = BaseModuleTargetPath + SynthesisAPI.VirtualFileSystem.Directory.DirectorySeparatorChar +
						moduleInfo.metadata.TargetPath + SynthesisAPI.VirtualFileSystem.Directory.DirectorySeparatorChar +
						GetPath(RemovePath(metadataPath, entry.FullName));
					var perm = Permissions.PublicReadWrite;
					if (AssetManager.Import(AssetManager.GetTypeFromFileExtension(extension),
						new DeflateStreamWrapper(stream, entry.Length), targetPath, entry.Name, perm, "") == null)
					{
						throw new LoadModuleException($"Failed to import asset: {entry.Name}");
					}
				}
				// Debug.Log("Loaded " + entry.Name);
			}
			foreach (var file in fileManifest)
			{
				SynthesisAPI.Runtime.ApiProvider.Log($"Module \"{moduleInfo.metadata.Name}\" is missing file from manifest: {file}", LogLevel.Warning);
			}
			moduleInfo.archive.Dispose();
		}

		private bool LoadApi()
		{
			// Set up Api
			var apiAssembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Api");
			foreach (var type in apiAssembly.GetTypes())
			{
				object instance = null;

				if (type.IsSubclassOf(typeof(SystemBase)))
				{
					var entity = EnvironmentManager.AddEntity();
					instance = entity.AddComponent(type);
				}

				if (instance == null && type.GetMethods().Any(m =>
					m.GetCustomAttribute<CallbackAttribute>() != null ||
					m.GetCustomAttribute<TaggedCallbackAttribute>() != null))
				{
					instance = Activator.CreateInstance(type);
				}

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
			assemblyOwners.Add(apiAssembly.GetName().Name, "Api");
			return true;
		}

		private bool LoadModuleAssembly(Stream stream, string owningModule)
		{
			// Load module assembly
			var memStream = new MemoryStream();
			stream.CopyTo(memStream);
			stream.Close();
			var assembly = Assembly.Load(memStream.ToArray());

			// Set up module

			Type[] types;
			try
			{
				types = assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException e)
			{
				if (e is ReflectionTypeLoadException reflectionTypeLoadException)
				{
					foreach (Exception inner in reflectionTypeLoadException.LoaderExceptions)
					{
						SynthesisAPI.Runtime.ApiProvider.Log($"Loading module {owningModule} resulted in type errors\n{inner}", LogLevel.Error);
					}
				}
				return false;
			}
			catch (Exception e)
			{
				SynthesisAPI.Runtime.ApiProvider.Log($"Failed to get types from module {owningModule}\n{e}", LogLevel.Error);
				return false;
			}

			foreach (var exportedModuleClass in types
				.Where(t => t.GetCustomAttribute<ModuleExportAttribute>() != null))
			{
				try
				{
					object exportedModuleClassInstance = null;

					if (exportedModuleClass.IsSubclassOf(typeof(SystemBase)))
					{
						var entity = EnvironmentManager.AddEntity();
						exportedModuleClassInstance = entity.AddComponent(exportedModuleClass);
					}

					if (exportedModuleClassInstance == null)
						exportedModuleClassInstance = Activator.CreateInstance(exportedModuleClass);

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
				catch (Exception e)
				{
					SynthesisAPI.Runtime.ApiProvider.Log($"Module loader failed to process type {exportedModuleClass} from module {owningModule}\n{e}", LogLevel.Error);
					// TODO unload assembly? return false?
					continue;
				}
			}

			assemblyOwners.Add(assembly.GetName().Name, owningModule);
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
				new[]
				{
					typeof(ReflectHelper).GetMethod("CastObject")
						?.MakeGenericMethod(eventType)
						.Invoke(null, new object[] {e})
				});
		}

		private void RerouteConsoleOutput()
		{
			var writer = new StreamWriter(newConsoleStream);
			Console.SetOut(writer);

			var reader = new StreamReader(newConsoleStream);
			bool firstUse = true;
			Task.Run(() =>
			{
				while (true)
				{
					if (newConsoleStream.Position != lastConsoleStreamPos)
					{
						if (firstUse)
						{
							SynthesisAPI.Runtime.ApiProvider.Log("Please use ApiProvider.Log intsead of Console.WriteLine", LogLevel.Warning);
							firstUse = false;
						}
						writer.Flush();
						var pos = newConsoleStream.Position;
						newConsoleStream.Position = lastConsoleStreamPos;

						SynthesisAPI.Runtime.ApiProvider.Log(reader.ReadToEnd());

						lastConsoleStreamPos = pos;
						newConsoleStream.Position = pos;
					}
				}
			});
		}

		private class ApiProvider : IApiProvider
		{
			private GameObject _entityParent;
			private Dictionary<Entity, GameObject> _gameObjects;
			private readonly Dictionary<Type, Type> _builtins;
			private bool debugLogsEnabled = true;

			public ApiProvider()
			{
				_entityParent = new GameObject("Entities");
				_gameObjects = new Dictionary<Entity, GameObject>();
				_builtins = new Dictionary<Type, Type>
				{
					{ typeof(SynthesisAPI.EnvironmentManager.Components.Mesh), typeof(MeshAdapter) },
					{ typeof(SynthesisAPI.EnvironmentManager.Components.Camera), typeof(CameraAdapter) },
					{ typeof(SynthesisAPI.EnvironmentManager.Components.Transform), typeof(TransformAdapter) },
					{ typeof(SynthesisAPI.EnvironmentManager.Components.Selectable), typeof(SelectableAdapter) },
					{ typeof(SynthesisAPI.EnvironmentManager.Components.MeshCollider), typeof(MeshColliderAdapter) },
					{ typeof(SynthesisAPI.EnvironmentManager.Components.Rigidbody), typeof(RigidbodyAdapter) },
					{ typeof(SynthesisAPI.EnvironmentManager.Components.HingeJoint), typeof(HingeJointAdapter) }
				};
			}

			public void Log(object o, LogLevel logLevel = LogLevel.Info, string memberName = "", string filePath = "", int lineNumber = 0)
			{
				var callSite = new StackTrace().GetFrame(2).GetMethod().DeclaringType?.Assembly.GetName().Name;
				var msg = $"{(assemblyOwners.ContainsKey(callSite) ? assemblyOwners[callSite] : $"{callSite}.dll")}\\{filePath.Split('\\').Last()}:{lineNumber}: {o}";
				switch (logLevel)
				{
					case LogLevel.Info:
						{
							Debug.Log(msg);
							break;
						}
					case LogLevel.Debug:
						if (!debugLogsEnabled)
						{
							return;
						}
						{
							Debug.Log(msg);
							break;
						}
					case LogLevel.Warning:
						{
							Debug.LogWarning(msg);
							break;
						}
					case LogLevel.Error:
						{
							Debug.LogError(msg);
							break;
						}
					default:
						throw new SynthesisException("Unhandled log level");
				}
			}

			public void SetEnableDebugLogs(bool enable)
			{
				debugLogsEnabled = enable;
			}

			public void AddEntityToScene(Entity entity)
			{
				if (_gameObjects.ContainsKey(entity))
					throw new Exception($"Entity \"{entity}\" already exists");
				var gameObject = new GameObject($"Entity {entity.GetIndex()}");
				gameObject.transform.SetParent(_entityParent.transform);
				_gameObjects.Add(entity, gameObject);
			}

			public void RemoveEntityFromScene(Entity entity)
			{
				GameObject gameObject;
				if (!_gameObjects.TryGetValue(entity, out gameObject))
					throw new Exception($"Entity \"{entity}\" does not exist");
				Destroy(gameObject);
			}

			public Component AddComponentToScene(Entity entity, Type t)
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
						throw new LoadModuleException($"Builtin {type.FullName} type lacked way to create new instance of type {t.FullName}");
					}
				}
				else if (t.IsSubclassOf(typeof(SystemBase)))
				{
					try
					{
						component = (SystemBase)Activator.CreateInstance(t);
						type = typeof(SystemAdapter);
					}
					catch (Exception e)
					{
						throw new LoadModuleException($"Failed to create instance of SystemBase with type {t.FullName}", e);
					}
				}
				else
				{
					try
					{
						component = (Component)Activator.CreateInstance(t);
						type = typeof(ComponentAdapter);
					}
					catch (Exception e)
					{
						throw new LoadModuleException($"Failed to create instance of component with type {t.FullName}", e);
					}
				}

				dynamic gameObjectComponent = gameObject.AddComponent(type);
				gameObjectComponent.SetInstance(component);

				return component;
			}

			public void RemoveComponentFromScene(Entity entity, Type t)
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
					type = typeof(SystemAdapter);
				}
				else
				{
					type = typeof(ComponentAdapter);
				}
				Destroy(gameObject.GetComponent(type));
			}

			public T CreateUnityType<T>(params object[] args) where T : class
			{
				if (args.Length > 0)
					return (T)Activator.CreateInstance(typeof(T), args);
				else
					return (T)Activator.CreateInstance(typeof(T));
			}

			public VisualTreeAsset GetDefaultUIAsset(string assetName)
			{
				int index = Array.IndexOf(ResourceLedger.Instance.Keys, assetName);
				if (index != -1)
					return ResourceLedger.Instance.Values[index];
				return null;
			}

			public TUnityType InstantiateFocusable<TUnityType>() where TUnityType : Focusable =>
				(TUnityType)Activator.CreateInstance(typeof(TUnityType));

			public VisualElement GetRootVisualElement()
			{
				// TODO: Re-evaluate this
				return PanelRenderer.visualTree;
			}
		}
	}
}