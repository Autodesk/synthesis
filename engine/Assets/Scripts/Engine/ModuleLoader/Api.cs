using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.Modules;
using SynthesisAPI.Runtime;
using SynthesisAPI.Utilities;
using SynthesisAPI.VirtualFileSystem;
using Unity.UIElements.Runtime;
using UnityEngine.UIElements;
using UnityEngine;
using Engine.ModuleLoader.Adapters;
using Logger = SynthesisAPI.Utilities.Logger;
using Component = SynthesisAPI.EnvironmentManager.Component;
using Debug = UnityEngine.Debug;


namespace Engine.ModuleLoader
{
	public class Api : MonoBehaviour
	{
		private static readonly string ModulesSourcePath = FileSystem.BasePath + "modules";
		private static readonly string BaseModuleTargetPath = SynthesisAPI.VirtualFileSystem.Directory.DirectorySeparatorChar + "modules";

		private static MemoryStream _newConsoleStream = new MemoryStream();
		private static long _lastConsoleStreamPos = 0;

		public void Start() // Must happen after ResourceLedger is initialized (in Awake)
		{
			SetupApplication(); // Always do this first

			DontDestroyOnLoad(gameObject); // Do not destroy this game object when loading a new scene

			ModuleManager.RegisterModuleAssemblyName(Assembly.GetExecutingAssembly().GetName().Name, "Core Engine");
			Logger.RegisterLogger(new LoggerImpl());
			ApiProvider.RegisterApiProvider(new ApiProviderImpl());
			Logger.RegisterLogger(new ToastLogger()); // Must happen after ApiProvider is registered

			ModuleLoader.PreloadApi();
			ModuleLoader.LoadModules(ModulesSourcePath, BaseModuleTargetPath);

			RerouteConsoleOutput();
		}

		public void Update()
		{
			ToastLogger.ScrollToBottom();
		}

		private void SetupApplication()
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
						Debug.Log(msg);
						break;
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

			public T CreateUnityType<T>(params object[] args) where T : class =>
				(T)Activator.CreateInstance(typeof(T), args);

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