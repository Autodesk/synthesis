using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Synthesis.UI
{
    public abstract class UIContainer
    {
        protected GameObject parentUIObject;

        protected UIContainer(GameObject parentUIObject)
        {
            this.parentUIObject = parentUIObject;
        }

        protected void RegisterCallbacks(UIContainer state)
        {
            Button[] buttons = parentUIObject.GetComponentsInChildren<Button>();
            MethodInfo methodInfo;
            foreach (Button b in buttons)
            {
                methodInfo = state.GetType().GetMethod("on" + b.name + "Clicked");
                if (methodInfo != null)
                {
                    b.onClick.AddListener(() => methodInfo.Invoke(null, null));
                }
            }
        }
    }
}