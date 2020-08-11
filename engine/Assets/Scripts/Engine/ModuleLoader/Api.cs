using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
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
using UnityEngine;
using Engine.ModuleLoader.Adapters;
using Logger = SynthesisAPI.Utilities.Logger;
using Directory = System.IO.Directory;

using PreloadedModule = System.ValueTuple<System.IO.Compression.ZipArchive, Engine.ModuleLoader.ModuleMetadata>;

namespace Engine.ModuleLoader
{
	public class Api : MonoBehaviour
	{
		private static readonly string ModulesSourcePath = FileSystem.BasePath + "modules";
		private static readonly string BaseModuleTargetPath = SynthesisAPI.VirtualFileSystem.Directory.DirectorySeparatorChar + "modules";

		private static MemoryStream _newConsoleStream = new MemoryStream();
		private static long _lastConsoleStreamPos = 0;

		public void Awake()
		{
			SetupApplication(); // Always do this first

			DontDestroyOnLoad(gameObject); // Do not destroy this game object when loading a new scene

			ModuleManager.RegisterModuleAssemblyName(Assembly.GetExecutingAssembly().GetName().Name, "Core Engine");
			Logger.RegisterLogger(new LoggerImpl());
			Logger.RegisterLogger(new ToastLogger());
			ApiProvider.RegisterApiProvider(new ApiProviderImpl());

			try
			{
				LoadApi();
			}
			catch (Exception e)
			{
				Logger.Log($"Failed to load API\n{e}", LogLevel.Error);
			}
			LoadModules();
			RerouteConsoleOutput();
		}

		public void Update()
		{
			ToastLogger.ScrollToBottom();
		}

		private void LoadModules()
		{
			if (!Directory.Exists(ModulesSourcePath))
			{
				Directory.CreateDirectory(ModulesSourcePath);
			}
			List<PreloadedModule> modules = new List<PreloadedModule>();
			try
			{
				modules = PreloadModules();
			}
			catch (Exception e)
			{
				Logger.Log($"Failed to preload modules\n{e}", LogLevel.Error);
			}
			try
			{
				ResolveDependencies(modules);
			}
			catch (Exception e)
			{
				Logger.Log($"Failed to resolve module dependencies\n{e}", LogLevel.Error);
			}
			foreach (var (archive, metadata) in modules)
			{
				try
				{
					LoadModule((archive, metadata));

					EventBus.Push(new LoadModuleEvent(metadata.Name, metadata.Version));
					ModuleManager.AddToLoadedModuleList(new ModuleManager.ModuleInfo(metadata.Name, metadata.Version));
				}
				catch (Exception e)
				{
					Logger.Log($"Failed to load module {metadata.Name}\n{e}", LogLevel.Error);
					// TODO error screen
					throw e;
				}
			}
			foreach (var (archive, _) in modules)
			{
				archive.Dispose();
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
						foreach (var (archive, _) in modules)
						{
							archive.Dispose();
						}
						throw new LoadModuleException($"Attempting to load module with duplicate name: {metadata.Name}");
					}
					if (metadata.TargetPath == module?.Item2.TargetPath)
					{
						foreach (var (archive, _) in modules)
						{
							archive.Dispose();
						}
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
				Logger.Log($"Potential module missing is metadata file: {filePath}", LogLevel.Warning);
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
					if (moduleList.All(m => m.metadata.Name != dependency.Name))
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
						throw new LoadModuleException($"Failed to load assembly: {entry.Name}");
					}
				}
				else
				{
					var targetPath = BaseModuleTargetPath + SynthesisAPI.VirtualFileSystem.Directory.DirectorySeparatorChar +
						moduleInfo.metadata.TargetPath + SynthesisAPI.VirtualFileSystem.Directory.DirectorySeparatorChar +
						GetPath(RemovePath(metadataPath, entry.FullName));
					var perm = Permissions.PublicReadWrite;
					var type = AssetManager.GetTypeFromFileExtension(extension);
					if (type == null)
					{
						throw new LoadModuleException($"Failed to determine asset type from file extension of asset: {entry.Name}");
					}
					else if (AssetManager.Import(type,
						new DeflateStreamWrapper(stream, entry.Length), targetPath, entry.Name, perm, "") == null)
					{
						throw new LoadModuleException($"Failed to import asset: {entry.Name}");
					}
				}
				// Debug.Log("Loaded " + entry.Name);
			}
			foreach (var file in fileManifest)
			{
				Logger.Log($"Module \"{moduleInfo.metadata.Name}\" is missing file from manifest: {file}", LogLevel.Warning);
			}
		}

		private bool LoadApi()
		{
			// Set up Api
			try
			{
				var apiAssembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Api");
				foreach (var type in apiAssembly.GetTypes()
					.Where(t => t.GetCustomAttribute<ModuleOmitAttribute>() == null &&
					(t.IsSubclassOf(typeof(SystemBase)) ||
						t.GetMethods().Any(m => m.GetCustomAttribute<CallbackAttribute>() != null || 
						m.GetCustomAttribute<TaggedCallbackAttribute>() != null))))
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
				ModuleManager.RegisterModuleAssemblyName(apiAssembly.GetName().Name, "Api");
			}
			catch (Exception e)
			{
				Logger.Log($"Failed to load API\n{e}", LogLevel.Error);
			}
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
					foreach (var inner in reflectionTypeLoadException.LoaderExceptions)
					{
						Logger.Log($"Loading module {owningModule} resulted in type errors\n{inner}", LogLevel.Error);
					}
				}
				return false;
			}
			catch (Exception e)
			{
				Logger.Log($"Failed to get types from module {owningModule}\n{e}", LogLevel.Error);
				return false;
			}

			foreach (var exportedModuleClass in types
				.Where(t => t.GetCustomAttribute<ModuleOmitAttribute>() == null && 
					(t.IsSubclassOf(typeof(SystemBase)) ||
						t.GetMethods().Any(m => m.GetCustomAttribute<CallbackAttribute>() != null
						|| m.GetCustomAttribute<TaggedCallbackAttribute>() != null))))
			{
				try
				{
					object exportedModuleClassInstance = null;

					if (exportedModuleClass.IsSubclassOf(typeof(SystemBase)))
					{
						var entity = EnvironmentManager.AddEntity();
						exportedModuleClassInstance = entity.AddComponent(exportedModuleClass);
					}

					if(exportedModuleClassInstance == null)
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
				catch (TypeLoadException e)
				{
					Logger.Log($"Module loader failed to process type {exportedModuleClass} from module {owningModule}\nType: {e.TypeName}, Site: {e.TargetSite}\n{e}", LogLevel.Error);
				}
				catch (Exception e)
				{
					Logger.Log($"Module loader failed to process type {exportedModuleClass} from module {owningModule}\n{e}", LogLevel.Error);
					// TODO unload assembly? return false?
					continue;
				}
			}

			ModuleManager.RegisterModuleAssemblyName(assembly.GetName().Name, owningModule);
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

		private static void SetupApplication()
		{
			Application.logMessageReceivedThreaded +=
				(condition, stackTrace, type) =>
				{
					if (type == LogType.Exception)
					{
						Logger.Log($"Unhandled exception: {condition}\n{stackTrace}", LogLevel.Error);
						// TODO show some module-independent error screen
						if (!ModuleManager.IsFinishedLoading)
						{
							Task.Run(() =>
							{
								while (Application.isPlaying)
								{
									Thread.Sleep(250);
									if (ModuleManager.IsFinishedLoading)
									{
										Logger.Log($"Unhandled exception: {condition}\n{stackTrace}", LogLevel.Error);
										break;
									}
								}
							});
						}
					}
				};
			Screen.fullScreen = false;

			var _ = SynthesisAPI.UIManager.UIManager.RootElement;
		}

		private class LoggerImpl : SynthesisAPI.Utilities.ILogger
		{
			private bool debugLogsEnabled = true;

			public void Log(object o, LogLevel logLevel = LogLevel.Info, string memberName = "", string filePath = "", int lineNumber = 0)
			{
				var type = new StackTrace().GetFrame(2).GetMethod().DeclaringType;
				var msg = $"{ModuleManager.GetDeclaringModule(type)}\\{filePath.Split('\\').Last()}:{lineNumber}: {o}";
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
		}
		private void RerouteConsoleOutput()
		{
			var writer = new StreamWriter(_newConsoleStream);
			Console.SetOut(writer);

			var reader = new StreamReader(_newConsoleStream);
			bool firstUse = true;
			Task.Run(() =>
			{
				while (Application.isPlaying)
				{
					if (_newConsoleStream.Position != _lastConsoleStreamPos)
					{
						if (firstUse)
						{
							Logger.Log("Please use ApiProvider.Log intsead of Console.WriteLine", LogLevel.Warning);
							firstUse = false;
						}
						writer.Flush();
						var pos = _newConsoleStream.Position;
						_newConsoleStream.Position = _lastConsoleStreamPos;

						Logger.Log(reader.ReadToEnd());

						_lastConsoleStreamPos = pos;
						_newConsoleStream.Position = pos;
					}
				}
			});
		}

		public static class ApiProviderData
		{
			public static GameObject EntityParent { get; set; }
			public static Dictionary<Entity, GameObject> GameObjects { get; set; }

			static ApiProviderData()
			{
				EntityParent = new GameObject("Entities");
				GameObjects = new Dictionary<Entity, GameObject>();
			}
		}

		private class ApiProviderImpl : IApiProvider
		{
			private readonly Dictionary<Type, Type> _builtins;

			public ApiProviderImpl()
			{
				_builtins = new Dictionary<Type, Type>
				{
					{ typeof(SynthesisAPI.EnvironmentManager.Components.Mesh), typeof(MeshAdapter) },
					{ typeof(SynthesisAPI.EnvironmentManager.Components.Camera), typeof(CameraAdapter) },
					{ typeof(SynthesisAPI.EnvironmentManager.Components.Transform), typeof(TransformAdapter) },
					{ typeof(SynthesisAPI.EnvironmentManager.Components.Selectable), typeof(SelectableAdapter) },
					{ typeof(SynthesisAPI.EnvironmentManager.Components.Parent), typeof(ParentAdapter) }
				};
			}

			public void AddEntityToScene(Entity entity)
			{
				if (ApiProviderData.GameObjects.ContainsKey(entity))
					throw new Exception($"Entity \"{entity}\" already exists");
				var gameObject = new GameObject($"Entity {entity.Index}");
				gameObject.transform.SetParent(ApiProviderData.EntityParent.transform);
				ApiProviderData.GameObjects.Add(entity, gameObject);
			}

			public void RemoveEntityFromScene(Entity entity)
			{
				GameObject gameObject;
				if (!ApiProviderData.GameObjects.TryGetValue(entity, out gameObject))
					throw new Exception($"Entity \"{entity}\" does not exist");
				Destroy(gameObject);
			}

			public Component AddComponentToScene(Entity entity, Type t)
			{
				GameObject gameObject;
				if (!ApiProviderData.GameObjects.TryGetValue(entity, out gameObject))
					throw new Exception($"Entity \"{entity}\" does not exist");

				Type type;
				Component component;

				if (_builtins.ContainsKey(t))
				{
					type = _builtins[t];
					component = (Component)type.GetMethod("NewInstance")?.Invoke(null, null);

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

				var gameObjectComponent = gameObject.AddComponent(type);
				type.GetMethod("SetInstance").Invoke(gameObjectComponent, new[] { component });

				return component;
			}

			public void AddComponentToScene(Entity entity, Component component)
			{
				GameObject gameObject;
				if (!ApiProviderData.GameObjects.TryGetValue(entity, out gameObject))
					throw new Exception($"Entity \"{entity}\" does not exist");
				Type componentType = component.GetType();
				Type type;
				if (_builtins.ContainsKey(componentType))
				{
					type = _builtins[componentType];
				}
				else if (componentType.IsSubclassOf(typeof(SystemBase)))
				{
					type = typeof(SystemAdapter);
				}
				else
				{
					type = typeof(ComponentAdapter);
				}

				var gameObjectComponent = gameObject.AddComponent(type);
				type.GetMethod("SetInstance").Invoke(gameObjectComponent, new[] { component });
			}

			public void RemoveComponentFromScene(Entity entity, Type t)
			{
				GameObject gameObject;
				if (!ApiProviderData.GameObjects.TryGetValue(entity, out gameObject))
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