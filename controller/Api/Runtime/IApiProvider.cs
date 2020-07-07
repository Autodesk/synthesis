using System;
using System.Collections;
using System.Collections.Generic;
using SynthesisAPI.Modules;
using UnityEngine;
using Component = SynthesisAPI.EnvironmentManager.Component;

namespace SynthesisAPI.Runtime
{
	public interface IApiProvider
	{

		void Log(object o);

		uint AddEntity();

        #region UI

        T CreateUnityType<T>(params object[] args);
        TUnityType InstantiateFocusable<TUnityType>() where TUnityType : UnityEngine.UIElements.Focusable;
		UnityEngine.UIElements.VisualElement GetRootVisualElement();

        #endregion
		TUnityType InstantiateFocusable<TUnityType>() where TUnityType : UnityEngine.UIElements.Focusable;
		Component AddComponent(Type t, uint entity);
		TComponent AddComponent<TComponent>(uint entity) where TComponent : Component;

		Component GetComponent(Type t, uint entity);
		TComponent GetComponent<TComponent>(uint entity) where TComponent : Component;
		List<Component> GetComponents(uint entity);
	}
}