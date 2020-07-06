﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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

using System.IO.Compression;
using System.Text;
using System.Xml.Serialization;
using Assets.Scripts.Engine.Util;
using JetBrains.Annotations;
using SynthesisAPI.AssetManager;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EventBus;
using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.Utilities;
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
			foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a =>
				a.GetTypes()))
			{

				var instance = Activator.CreateInstance(type);
				foreach (var callback in type.GetMethods()
					.Where(m => m.GetCustomAttribute(typeof(CallbackAttribute)) != null))
				{
					RegisterCallbackByMethodInfo(callback, instance);
				}
			}
			var modules = new List<(ZipArchive, ModuleMetadata)>();
			if (!Directory.Exists(FileSystem.BasePath + "modules"))
			{
				Directory.CreateDirectory(FileSystem.BasePath + "modules");
			}
            foreach (var file in Directory.GetFiles(FileSystem.BasePath + "modules").Where(fn => Path.GetExtension(fn) == ".zip"))
            {
                var (module, metadata) =
	                PreloadModule($"{Path.DirectorySeparatorChar}{file.Split(Path.DirectorySeparatorChar).Last()}") ??
	                (null, null);
                if (module is null)
                    continue;
				modules.Add((module, metadata));
            }

            ResolveDependencies(modules);
			foreach(var (archive, metadata) in modules) {
				ImportAssetsFromModule((archive, metadata));
            }
			foreach (var (_, metadata) in modules)
			{
				ModuleManager.AddToLoadedModuleList(metadata.Name);
			}
			ModuleManager.MarkFinishedLoading();
		}

        private (ZipArchive, ModuleMetadata)? PreloadModule(string filePath)
        {
            var module = ZipFile.Open(FileSystem.BasePath + Path.DirectorySeparatorChar + "modules" + filePath, ZipArchiveMode.Read);
            if (module.Entries.All(e => e.Name != ModuleMetadata.MetadataFilename))
            {
                return null;
            }

            var metadata = ModuleMetadata.Deserialize(module.Entries
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

        private void ImportAssetsFromModule((ZipArchive archive, ModuleMetadata metadata) moduleInfo)
        {
			foreach (var entry in moduleInfo.archive.Entries.Where(e =>
				e.Name != ModuleMetadata.MetadataFilename && moduleInfo.metadata.FileManifest.Contains(e.Name)))
			{
				var extension = Path.GetExtension(entry.Name);
				var targetPath = "/modules/" + moduleInfo.metadata.TargetPath;
				var stream = entry.Open();
				var perm = Permissions.PublicReadWrite;
				if (extension == ".dll" && !LoadAssembly(stream))
				{
					Debug.Log($"Failed to load assembly {entry.Name}");
				}
				else
				{
					if (AssetManager.Import(entry.Open(), targetPath, entry.Name, perm, "") == null)
					{
						throw new Exception("Asset module type");
					}
				}
			}
        }

        private bool LoadAssembly(Stream stream)
		{
		    var memStream = new MemoryStream();
			stream.CopyTo(memStream);
			var assembly = Assembly.Load(memStream.ToArray());

			foreach (var export in assembly.GetTypes()
				.Where(t => t.GetCustomAttribute<ModuleExportAttribute>() != null))
			{
				var instance = Activator.CreateInstance(export);

				if (export.IsSubclassOf(typeof(SystemBase)))
				{
					var entity = SynthesisAPI.Runtime.ApiProvider.AddEntity();
					if (entity != null)
						SynthesisAPI.Runtime.ApiProvider.AddComponent(export, entity.Value);
					else
						throw new Exception("F");
				}

				foreach (var callback in export.GetMethods()
					.Where(m => m.GetCustomAttribute<CallbackAttribute>() != null))
				{
					RegisterCallbackByMethodInfo(callback, instance);
				}
			}
			return true;
		}

        private void RegisterCallbackByMethodInfo(MethodInfo callback, object instance)
        {
	        if (instance.GetType() != callback.DeclaringType)
	        {
				throw new Exception(
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
			private Dictionary<uint, GameObject> _gameObjects;
			private readonly Dictionary<Type, Type> _builtins;

			public ApiProvider()
			{
				_gameObjects = new Dictionary<uint, GameObject>();
				_builtins = new Dictionary<Type, Type>();
			}

			public void Log(object o)
			{
				Debug.Log(o);
			}

			public uint AddEntity()
			{
				var entity = EnvironmentManager.AddEntity();
				var gameObject = new GameObject();
				_gameObjects.Add(entity, gameObject);
				return entity;
			}

			public Component AddComponent(Type t, uint entity)
			{
				var gameObject = _gameObjects[entity];
				if (gameObject == null)
				{
					throw new Exception($"No GameObject exists with id \"{entity}\"");
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
				} else if (t.IsSubclassOf(typeof(SystemBase)))
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

			public TComponent AddComponent<TComponent>(uint entity) where TComponent : Component =>
				(TComponent) AddComponent(typeof(TComponent), entity);

			public Component GetComponent(Type t, uint entity) => entity.GetComponent(t);

			public TComponent GetComponent<TComponent>(uint entity) where TComponent : Component =>
				entity.GetComponent<TComponent>();

			public List<Component> GetComponents(uint entity) => entity.GetComponents();
		}
	}
}