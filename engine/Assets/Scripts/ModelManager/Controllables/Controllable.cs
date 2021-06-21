using System.Collections.ObjectModel;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Synthesis.Attributes;
using System.Linq;

using Getter = System.Func<object, float>;
using Setter = System.Action<object, float>;
// Item1 -> Getter
// Item2 -> Setter
using InputProperty = System.Tuple<System.Func<object, float>, System.Action<object, float>>;

namespace Synthesis.ModelManager.Controllables {
    public abstract class Controllable : MonoBehaviour {
        public Controllable Parent;
        public List<Controllable> Children = new List<Controllable>();

        public Dictionary<string, InputProperty> InputProperties => Controllable.GetInputProperties(this);

        public void Awake() {
            allControllables.Add(this);
        }

        public void OnDestroy() {
            allControllables.RemoveAll(x => x == this);
        }

        public void Adopt<T>(T child) where T : Controllable {
            if (Parent != null)
                throw new Exception("Child already has a parent");
            Children.Add(child);
            child.Parent = this;
        }

        public void Abandon<T>(T child) where T : Controllable {
            Children.RemoveAll(x => {
                if (x == child) {
                    x.Parent = null;
                    return true;
                }
                return false;
            });
        }

        private static List<Controllable> allControllables = new List<Controllable>();
        public static ReadOnlyCollection<Controllable> AllControllables {
            get => allControllables.AsReadOnly();
        }
        private static Dictionary<Type, Dictionary<string, InputProperty>> CachedInputProperties = new Dictionary<Type, Dictionary<string, InputProperty>>();
        public static Dictionary<string, InputProperty> GetInputProperties<T>(T controllable) where T : Controllable {

            var type = controllable.GetType();
            if (CachedInputProperties.ContainsKey(type)) {
                Debug.Log($"{type.Name}");
                return CachedInputProperties[type];
            } else {

                var inputProps = new Dictionary<string, InputProperty>();

                foreach (var property in type.GetProperties()) {

                    if (property.DeclaringType != type) // Figure out how to determine if property is float
                        continue;

                    var attrObj = property.GetCustomAttributes(typeof(ControllableInputAttribute), false);
                    foreach (var attr in attrObj) {
                        if (attr is ControllableInputAttribute) {
                            var inputAttr = attr as ControllableInputAttribute;

                            string propertyName;
                            InputProperty input;

                            if (inputAttr.Name != string.Empty)
                                propertyName = inputAttr.Name;
                            else
                                propertyName = property.Name;

                            input = new Tuple<Func<object, float>, Action<object, float>>(
                                a => (float)property.GetValue(a),
                                (a, b) => property.SetValue(a, b)
                            );
                            inputProps.Add(propertyName, input);

                            break;
                        }
                    }
                }

                CachedInputProperties.Add(type, inputProps);
                return inputProps;
            }
        }

        // public class InputProperty {
        //     private Action<float> SetProperty;
        //     private Func<float> GetProperty;

        //     public float Input {
        //         get => GetProperty();
        //         set => SetProperty(value);
        //     }

        //     public InputProperty(Action<float> setter, Func<float> getter) {
        //         SetProperty = setter;
        //         GetProperty = getter;
        //     }
        // }
    }
}
