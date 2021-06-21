﻿using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Synthesis.UI.ContextMenus;
using UnityEngine.UI;
using Synthesis.Attributes;
using Synthesis.Util;
using System.Linq;
using ContextMenu = Synthesis.UI.ContextMenus.ContextMenu;

namespace Synthesis.UI
{
    public class InteractableObject : MonoBehaviour, IPointerClickHandler, IPointerExitHandler, IPointerEnterHandler
    {
        // public class ContextItemEvent : UnityEvent { }
        public bool useReflection = true;

        private bool isBeingInteractedWith = false;
        public bool IsBeingInteractedWith {
            get => isBeingInteractedWith;
            set {
                isBeingInteractedWith = value;
                OnInteraction(value);
                if (OnInteractionStateChanged != null)
                    OnInteractionStateChanged(value);
            }
        }

        public delegate void InteractionStateChange(bool state);
        public event InteractionStateChange OnInteractionStateChanged;

        public string ContextMenuUID = string.Empty;
        public List<(string title, Sprite icon, Action<object> callback)> Options
            = new List<(string title, Sprite icon, Action<object> callback)>();

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log(gameObject.name);
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                //open right click floating window
                // Debug.Log(eventData.position);
                
                Vector2 position = new Vector2(eventData.position.x, eventData.position.y); // Maybe have that 1080 number adjust but for rn it's fine
                OnPointerClick(position);
            } else {
                ContextMenu.Hide();
            }
        }

        public void OnPointerClick(Vector2 position) {
            // Debug.Log($"{position.x}, {position.y}");
            if (useReflection) {
                ContextMenu.Show(this, position, ContextMenuUID, this);
            } else {
                ContextMenu.Show(this, position, ContextMenuUID, Options);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            //open tooltips and highlight on
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            //close tooltips and highlight off
        }

        public void AddOption(string title, Sprite icon, Action<object> callback) => Options.Add((title, icon, callback));

        protected virtual void OnInteraction(bool isInteractedWith) { }

        #region Static Methods

        private static Dictionary<Type, List<(string title, Sprite icon, Action<object> callback)>> InteractableTypes
            = new Dictionary<Type, List<(string, Sprite, Action<object>)>>();
        public static List<(string title, Sprite icon, Action<object> callback)> GetInteractableOptions<T>(T interactable) where T : InteractableObject {

            Type type = interactable.GetType();

            if (InteractableTypes.ContainsKey(type)) {

                // Debug.Log(type.Name);
                return InteractableTypes[type];

            } else {

                var list = new List<(string title, Sprite icon, Action<object> callback)>();
                type.GetMethods().ForEach(x => {
                    if (x.DeclaringType == type) {
                        // Debug.Log(x.Name);
                        var attrObj = x.GetCustomAttributes(typeof(ContextMenuOptionAttribute), false);
                        if (attrObj.Count() > 0 && attrObj[0] is ContextMenuOptionAttribute) {
                            ContextMenuOptionAttribute attr = (ContextMenuOptionAttribute)attrObj[0];
                            string title;
                            Action<object> callback;

                            if (attr.Title == string.Empty)
                                title = x.Name;
                            else
                                title = attr.Title;

                            if (attr.Callback == null)
                                callback = a => x.Invoke(a, null);
                            else
                                callback = a => attr.Callback();

                            list.Add((title, attr.Icon, callback));
                        }
                    }
                });

                InteractableTypes.Add(type, list);
                return list;

            }
        }

        #endregion
    }
}
