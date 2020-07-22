using System;
using System.Collections;
using System.Collections.Generic;
using SynthesisAPI.Modules;
using UnityEngine;
using UnityEngine.UIElements;
using Component = SynthesisAPI.EnvironmentManager.Component;

namespace SynthesisAPI.Runtime
{
	public interface IApiProvider
	{
		
		void Log(object o);

		uint AddEntity();

		Component AddComponent(Type t, uint entity);
		TComponent AddComponent<TComponent>(uint entity) where TComponent : Component;

		Component GetComponent(Type t, uint entity);
		TComponent GetComponent<TComponent>(uint entity) where TComponent : Component;
		List<Component> GetComponents(uint entity);
		T CreateUnityType<T>(params object[] args) where T : class;
		VisualTreeAsset GetDefaultUIAsset(string assetName);

        #region UI

        // TUnityType InstantiateFocusable<TUnityType>() where TUnityType : UnityEngine.UIElements.Focusable;
		UnityEngine.UIElements.VisualElement GetRootVisualElement();

        #endregion
	}
}