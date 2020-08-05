using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.Utilities;
using System;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;
using Component = SynthesisAPI.EnvironmentManager.Component;

namespace SynthesisAPI.Runtime
{
	internal interface IApiProvider
	{
		void AddEntityToScene(Entity entity);

		void RemoveEntityFromScene(Entity entity);

		#nullable enable
		Component? AddComponentToScene(Entity entity, Type t);

		void AddComponentToScene(Entity entity, Component component);

		void RemoveComponentFromScene(Entity entity, Type t);

		T CreateUnityType<T>(params object[] args) where T : class;
		VisualTreeAsset GetDefaultUIAsset(string assetName);

        #region UI

        // TUnityType InstantiateFocusable<TUnityType>() where TUnityType : UnityEngine.UIElements.Focusable;
		UnityEngine.UIElements.VisualElement GetRootVisualElement();

        #endregion
	}
}
