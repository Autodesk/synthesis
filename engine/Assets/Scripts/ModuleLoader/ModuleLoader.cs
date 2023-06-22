#if UNITY_STANDALONE_WIN
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Assets.Scripts.Engine.Util;
using Ionic.Zip;
using SynthesisAPI.AssetManager;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EventBus;
using SynthesisAPI.Modules;
using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.Utilities;
using SynthesisAPI.VirtualFileSystem;
using Logger = SynthesisAPI.Utilities.Logger;
using PreloadedModule = System.ValueTuple<Ionic.Zip.ZipFile, Engine.ModuleLoader.ModuleMetadata>;

using Directory = System.IO.Directory;
using Type = System.Type;
using ZipFile = Ionic.Zip.ZipFile;

namespace Engine.ModuleLoader
{
    public static class ModuleLoader
	{
		private static string _moduleSourcePath;
		private static string _baseModuleTargetPath;

		public static void LoadModules(string moduleSourcePath, string baseModuleTargetPath)
		{
			_moduleSourcePath = moduleSourcePath;
			_baseModuleTargetPath = baseModuleTargetPath;

			if (!Directory.Exists(_moduleSourcePath))
			{
				Directory.CreateDirectory(_moduleSourcePath);
			}
			var modules = new List<PreloadedModule>();
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
					ModuleManager.AddToLoadedModuleList(new ModuleManager.ModuleInfo(metadata.Name, metadata.Version, metadata.Author, metadata.Description));
				}
				catch (Exception e)
				{
					Logger.Log($"Failed to load module {metadata.Name}\n{e}", LogLevel.Error);
					// TODO error screen
					break;
				}
			}
			foreach (var (archive, _) in modules)
			{
				archive.Dispose();
			}
			ModuleManager.MarkFinishedLoading();
		}

		private static List<PreloadedModule> PreloadModules()
		{
			var modules = new List<PreloadedModule>();

			// Discover and preload all modules
			foreach (var file in Directory.GetFiles(_moduleSourcePath).Where(fn => Path.GetExtension(fn) == ".zip"))
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

		private static PreloadedModule? PreloadModule(string filePath)
		{
			var fullPath = $"{_moduleSourcePath}{Path.DirectorySeparatorChar}{filePath}";

			var module = ZipFile.Read(fullPath);

			if (module.Entries.All(e => e.FileName != ModuleMetadata.MetadataFilename))
			{
				Logger.Log($"Potential module missing is metadata file: {filePath}", LogLevel.Warning);
				return null;
			}

			// Parse module metadata
			try
			{

				var tmp = module.Entries.First(e => e.FileName == ModuleMetadata.MetadataFilename);
				var metadata = ModuleMetadata.Deserialize(module.Entries
					.First(e => e.FileName == ModuleMetadata.MetadataFilename).OpenReader());
				return (module, metadata);
			}
			catch (Exception e)
			{
				module.Dispose();
				throw new LoadModuleException($"Failed to deserialize metadata in module: {fullPath}", e);
			}
		}

		public static void ResolveDependencies(List<(ZipFile archive, ModuleMetadata metadata)> moduleList)
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
			var solutionSet = new Queue<(ZipFile archive, ModuleMetadata metadata)>();
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

		public static void LoadModule((ZipFile archive, ModuleMetadata metadata) moduleInfo)
		{
			var fileManifest = new List<string>();
			fileManifest.AddRange(moduleInfo.metadata.FileManifest);

			var metadataPath = GetPath(moduleInfo.archive.Entries.First(e => e.FileName == ModuleMetadata.MetadataFilename).FileName);

			List<(Assembly assembly, string owningModule)> loadedAssemblies = new List<(Assembly assembly, string owningModule)>();

			foreach (var entry in moduleInfo.archive.Entries.Where(e =>
			{
				var name = RemovePath(metadataPath, e.FileName);
				return name != ModuleMetadata.MetadataFilename && moduleInfo.metadata.FileManifest.Contains(name);
			}))
			{
				fileManifest.Remove(RemovePath(metadataPath, entry.FileName));
				var extension = Path.GetExtension(entry.FileName);
				var stream = entry.OpenReader();
				if (extension == ".dll")
				{
					if (!LoadModuleAssembly(stream, moduleInfo.metadata.Name, loadedAssemblies))
					{
						throw new LoadModuleException($"Failed to load assembly: {entry.FileName}");
					}
				}
				else
				{
					var targetPath = _baseModuleTargetPath + SynthesisAPI.VirtualFileSystem.Directory.DirectorySeparatorChar +
						moduleInfo.metadata.TargetPath + SynthesisAPI.VirtualFileSystem.Directory.DirectorySeparatorChar +
						GetPath(RemovePath(metadataPath, entry.FileName));
					var perm = Permissions.PublicReadWrite;
					var type = AssetManager.GetTypeFromFileExtension(extension);
					if (type == null)
					{
						throw new LoadModuleException($"Failed to determine asset type from file extension of asset: {entry.FileName}");
					}
					else if (AssetManager.Import(type,
						new DeflateStreamWrapper(stream, entry.UncompressedSize), targetPath, entry.FileName, perm, "") == null)
					{
						throw new LoadModuleException($"Failed to import asset: {entry.FileName}");
					}
				}
			}
			ProcessLoadedAssemblies(loadedAssemblies);
			foreach (var file in fileManifest)
			{
				Logger.Log($"Module \"{moduleInfo.metadata.Name}\" is missing file from manifest: {file}", LogLevel.Warning);
			}
		}

		public static bool PreloadApi()
		{
			// Set up Api
			try
			{
				var apiAssembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Api");
				var types = apiAssembly.GetTypes()
					.Where(t => t.GetCustomAttribute<ModuleOmitAttribute>() == null &&
					            (t.IsSubclassOf(typeof(SystemBase)) ||
					             t.GetMethods().Any(m => m.GetCustomAttribute<CallbackAttribute>() != null ||
					                                     m.GetCustomAttribute<TaggedCallbackAttribute>() != null))).ToList();
				var prioritizedTypes = ResolveInitializationOrder(types, "API");
				var instances = new Dictionary<Type, object>();
				foreach (var (_, system, callback) in prioritizedTypes)
				{
					if (system != null)
					{
						var entity = EnvironmentManager.AddEntity();
						_ = entity.AddComponent(system);
					}
					else if (callback != null)
					{
						object instance = null;
						var declaringType = callback.DeclaringType;
						if (instances.ContainsKey(declaringType))
						{
							instance = instances[declaringType];
						}
						else
						{
							instance = Activator.CreateInstance(declaringType);
							instances[declaringType] = instance;
						}

						if (callback.GetCustomAttribute<TaggedCallbackAttribute>() != null)
						{
							RegisterTagCallback(callback, instance);
						}

						if (callback.GetCustomAttribute<CallbackAttribute>() != null)
						{
							RegisterTypeCallback(callback, instance);
						}
					}
					else
					{
						throw new LoadModuleException("Type prioritization failed");
					}
					ModuleManager.RegisterModuleAssemblyName(apiAssembly.GetName().Name, "Api");
				}
			}
			catch (Exception e)
			{
				Logger.Log($"Failed to load API\n{e}", LogLevel.Error);
			}

			return true;
		}
		public static bool LoadModuleAssembly(Stream stream, string owningModule, List<(Assembly assembly, string owningModule)> loadedAssemblies)
			{
			// Load module assembly
			var memStream = new MemoryStream();
			stream.CopyTo(memStream);
			stream.Close();
			var assembly = Assembly.Load(memStream.ToArray());

			// Logger.Log(assembly.FullName);

			loadedAssemblies.Add((assembly, owningModule));

			//return true;

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

			var exportedModuleClasses =
				types.Where(t => t.GetCustomAttribute<ModuleOmitAttribute>() == null &&
					(t.IsSubclassOf(typeof(SystemBase))
					 || t.GetMethods().Any(
						 m => m.GetCustomAttribute<CallbackAttribute>() != null
						      || m.GetCustomAttribute<TaggedCallbackAttribute>() != null))).ToList();
			var prioritizedInitializations = ResolveInitializationOrder(exportedModuleClasses, owningModule);
			var instances = new Dictionary<Type, object>();

			foreach (var (initializer, system, callback) in prioritizedInitializations)
			{
				try
				{
					if (initializer != null)
					{
						var init = initializer.GetMethod("Initialize");
						if (init != null)
						{
							init.Invoke(null, null);
						}
						else
						{
							Logger.Log($"Initializer for module {owningModule} does not have a valid Initialize method", LogLevel.Warning);
						}
					}
					else if (system != null)
					{
						var entity = EnvironmentManager.AddEntity();
						instances[system] = entity.AddComponent(system);
					}
					else
					{
						object instance = null;
						var declaringType = callback.DeclaringType;
						if (instances.ContainsKey(declaringType))
						{
							instance = instances[declaringType];
						}
						else
						{
							instance = Activator.CreateInstance(declaringType);
							instances[declaringType] = instance;
						}

						if (callback.GetCustomAttribute<TaggedCallbackAttribute>() != null)
						{
							RegisterTagCallback(callback, instance);
						}

						if (callback.GetCustomAttribute<CallbackAttribute>() != null)
						{
							RegisterTypeCallback(callback, instance);
						}
					}
				}
				catch (TypeLoadException e)
				{
					var errorSource = system == null ? callback.DeclaringType : system;
					Logger.Log($"Module loader failed to process type {errorSource} from module {owningModule}\nType: {e.TypeName}, Site: {e.TargetSite}\n{e}", LogLevel.Error);
				}
				catch (Exception e)
				{
					var errorSource = system == null ? callback.DeclaringType : system;
					Logger.Log($"Module loader failed to process type {errorSource} from module {owningModule}\n{e}", LogLevel.Error);
					// TODO unload assembly? return false?
					continue;
				}
			}

			ModuleManager.RegisterModuleAssemblyName(assembly.GetName().Name, owningModule);
			return true;
		}

		private static bool ProcessLoadedAssemblies(List<(Assembly assembly, string owningModule)> assemblies)
        {
			foreach (var loadedAssembly in assemblies)
			{
				Type[] types;
				try
				{
					types = loadedAssembly.assembly.GetTypes();
				}
				catch (ReflectionTypeLoadException e)
				{
					if (e is ReflectionTypeLoadException reflectionTypeLoadException)
					{
						foreach (var inner in reflectionTypeLoadException.LoaderExceptions)
						{
							Logger.Log($"Loading module {loadedAssembly.owningModule} resulted in type errors\n{inner}", LogLevel.Error);
						}
					}
					return false;
				}
				catch (Exception e)
				{
					Logger.Log($"Failed to get types from module {loadedAssembly.owningModule}\n{e}", LogLevel.Error);
					return false;
				}

				var exportedModuleClasses =
					types.Where(t => t.GetCustomAttribute<ModuleOmitAttribute>() == null &&
						(t.IsSubclassOf(typeof(SystemBase))
						 || t.GetMethods().Any(
							 m => m.GetCustomAttribute<CallbackAttribute>() != null
								  || m.GetCustomAttribute<TaggedCallbackAttribute>() != null))).ToList();
				var prioritizedInitializations = ResolveInitializationOrder(exportedModuleClasses, loadedAssembly.owningModule);
				var instances = new Dictionary<Type, object>();

				foreach (var (initializer, system, callback) in prioritizedInitializations)
				{
					try
					{
						if (initializer != null)
						{
							var init = initializer.GetMethod("Initialize");
							if (init != null)
							{
								init.Invoke(null, null);
							}
							else
							{
								Logger.Log($"Initializer for module {loadedAssembly.owningModule} does not have a valid Initialize method", LogLevel.Warning);
							}
						}
						else if (system != null)
						{
							var entity = EnvironmentManager.AddEntity();
							instances[system] = entity.AddComponent(system);
						}
						else
						{
							object instance = null;
							var declaringType = callback.DeclaringType;
							if (instances.ContainsKey(declaringType))
							{
								instance = instances[declaringType];
							}
							else
							{
								instance = Activator.CreateInstance(declaringType);
								instances[declaringType] = instance;
							}

							if (callback.GetCustomAttribute<TaggedCallbackAttribute>() != null)
							{
								RegisterTagCallback(callback, instance);
							}

							if (callback.GetCustomAttribute<CallbackAttribute>() != null)
							{
								RegisterTypeCallback(callback, instance);
							}
						}
					}
					catch (TypeLoadException e)
					{
						var errorSource = system == null ? callback.DeclaringType : system;
						Logger.Log($"Module loader failed to process type {errorSource} from module {loadedAssembly.owningModule}\nType: {e.TypeName}, Site: {e.TargetSite}\n{e}", LogLevel.Error);
					}
					catch (Exception e)
					{
						var errorSource = system == null ? callback.DeclaringType : system;
						Logger.Log($"Module loader failed to process type {errorSource} from module {loadedAssembly.owningModule}\n{e}", LogLevel.Error);
						// TODO unload assembly? return false?
						continue;
					}
				}

				ModuleManager.RegisterModuleAssemblyName(loadedAssembly.assembly.GetName().Name, loadedAssembly.owningModule);
			}
			return true;
		}

		private static List<(Type initializer, Type system, MethodInfo callback)> ResolveInitializationOrder(List<Type> types, string moduleName)
		{
			var systems = types.Where(t => t.IsSubclassOf(typeof(SystemBase))).ToList();
			var callbacks = types.Where(t => !t.IsSubclassOf(typeof(SystemBase)) || t.GetMethods().Any(m =>
				m.GetCustomAttribute<CallbackAttribute>() != null ||
				m.GetCustomAttribute<TaggedCallbackAttribute>() != null)).SelectMany(m =>
				m.GetMethods().Where(m2 =>
					m2.GetCustomAttribute<CallbackAttribute>() != null ||
					m2.GetCustomAttribute<TaggedCallbackAttribute>() != null)).ToList();
			var prioritizedSystems =
				systems.Where(t =>
					t.GetCustomAttribute<InitializationPriorityAttribute>() != null).OrderBy(t =>
					t.GetCustomAttribute<InitializationPriorityAttribute>().Value).ToList();
			var prioritizedCallbacks = callbacks.Where(m => m.GetCustomAttribute<InitializationPriorityAttribute>() != null).OrderBy(m =>
				m.GetCustomAttribute<InitializationPriorityAttribute>().Value).ToList();
			var prioritizedElements = new List<(Type, Type, MethodInfo)>();
			var systemIndex = 0;
			var callbackIndex = 0;

			byte GetPriority(object o)
			{
				if (o is Type t)
					return t.GetCustomAttribute<InitializationPriorityAttribute>().Value;
				else if (o is MethodInfo m)
					return m.GetCustomAttribute<InitializationPriorityAttribute>().Value;
				else
					return 255;
			}

			for (var i = 0; i < prioritizedSystems.Count + prioritizedCallbacks.Count; ++i)
			{
				if (GetPriority(prioritizedSystems.ElementAtOrDefault(systemIndex))-1 <= GetPriority(prioritizedCallbacks.ElementAtOrDefault(callbackIndex))-1)
				{
					prioritizedElements.Add((null, prioritizedSystems.ElementAtOrDefault(systemIndex), null));
					++systemIndex;
				}
				else
				{
					prioritizedElements.Add((null, null, prioritizedCallbacks.ElementAtOrDefault(callbackIndex)));
					++callbackIndex;
				}
			}

			foreach (var system in systems.Where(t => t.GetCustomAttribute<InitializationPriorityAttribute>() == null))
			{
				prioritizedElements.Add((null, system, null));
			}

			foreach (var callback in callbacks.Where(m => m.GetCustomAttribute<InitializationPriorityAttribute>() == null))
			{
				prioritizedElements.Add((null,null, callback));
			}

			var initClass = types.FirstOrDefault(t => t.Name == "ModuleInitializer");
			if (initClass != null)
			{
				if (!(initClass.IsAbstract && initClass.IsSealed))
				{
					Logger.Log($"Initializer for module {moduleName} is not static. Please make this class static.", LogLevel.Warning);
				}
				else
				{
					prioritizedElements.Insert(0, (initClass, null, null));
				}
			}

			return prioritizedElements;
		}


		private static void RegisterTagCallback(MethodInfo callback, object instance)
		{
			if (instance.GetType() != callback.DeclaringType) // Sanity check
			{
				throw new LoadModuleException(
					$"Type of instance variable \"{instance.GetType()}\" does not match declaring type of callback \"{callback.Name}\" (expected \"{callback.DeclaringType}\"");
			}
			var eventType = callback.GetParameters().First().ParameterType;
			var tag = callback.GetCustomAttribute<TaggedCallbackAttribute>().Tag;
			typeof(EventBus).GetMethod("NewTagListener")
				?.Invoke(null, new object[]
				{
					tag,
					CreateEventCallback(callback, instance, eventType)
				});
		}

		private static void RegisterTypeCallback(MethodInfo callback, object instance)
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

		private static EventBus.EventCallback CreateEventCallback(MethodInfo m, object instance, Type eventType)
		{
			return (e) => m.Invoke(instance,
				new[]
				{
					typeof(ReflectHelper).GetMethod("CastObject")
						?.MakeGenericMethod(eventType)
						.Invoke(null, new object[] {e})
				});
		}



	}
}
#endif