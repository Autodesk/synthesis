using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityGameObject = UnityEngine.GameObject;
using UnityEngine.UIElements;
using SynthesisAPI.Runtime;
using SynthesisAPI.UIManager.VisualElements;

namespace SynthesisAPI.UIManager
{
    public static class UIManager
    {
        public static void AddVisualElement(SynVisualElement element) => Instance.Root.Add(element.VisualElement);

        private class Inner
        {
            public VisualElement Root;

            private Inner()
            {
                Root = ApiProvider.GetRootVisualElement().Q<VisualElement>(name: "screen");
            }

            private static Inner _instance;
            public static Inner InnerInstance {
                get {
                    if (_instance == null)
                        _instance = new Inner();
                    return _instance;
                }
            }
        }

        private static Inner Instance => Inner.InnerInstance;
    }
}
