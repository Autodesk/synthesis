using SynthesisAPI.EnvironmentManager.Components;
using UnityEngine.EventSystems;

namespace Engine.ModuleLoader.Adapters
{
	public class CameraAdapter : UnityEngine.MonoBehaviour, IApiAdapter<Camera>
	{
		private Camera instance;
		private new UnityEngine.Camera camera;
		private static UnityEngine.Camera defaultCamera;

		public void SetInstance(Camera camera)
		{
			instance = camera;
		}

		public static Camera NewInstance()
		{
			return new Camera();
		}

		public void Awake()
		{
			camera = gameObject.GetComponent<UnityEngine.Camera>();
			if (camera == null)
			{
                if (defaultCamera == null)
                {
					defaultCamera = Util.Utilities.FindGameObject("Main Camera").GetComponent<UnityEngine.Camera>();
					 // TODO rename and deactivate Main Camera
                }
				camera = gameObject.AddComponent<UnityEngine.Camera>();
				camera.CopyFrom(defaultCamera);
				gameObject.AddComponent<PhysicsRaycaster>();
				camera.tag = "MainCamera"; // TODO manage which is main camera?
			}
		}
	}
}