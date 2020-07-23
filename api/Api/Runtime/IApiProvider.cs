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

		void AddEntityToScene(uint entity);

		void RemoveEntityFromScene(uint entity);

		#nullable enable
		Component? AddComponentToScene(uint entity, Type t);

		void RemoveComponentFromScene(uint entity, Type t);
		T CreateUnityType<T>(params object[] args) where T : class;
		VisualTreeAsset GetDefaultUIAsset(string assetName);

        #region UI

        // TUnityType InstantiateFocusable<TUnityType>() where TUnityType : UnityEngine.UIElements.Focusable;
		UnityEngine.UIElements.VisualElement GetRootVisualElement();

        #endregion
	}
}
