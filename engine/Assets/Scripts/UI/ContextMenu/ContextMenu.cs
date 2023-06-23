using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Synthesis.Util;
using TMPro;

// Todo: Descriptions
namespace Synthesis.UI.ContextMenus {
    public class ContextMenu : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler {
        public static bool IsShowing { get; private set; } = false;

        public CanvasScaler canvasScaler;
        public static CanvasScaler CanvasScaler => contextMenu.canvasScaler;

        public GameObject ContextItem;
        public Transform ContentContainer;
        // public TMP_Text ContextTitle;
        private List<GameObject> SpawnedItems = new List<GameObject>();

        private static ContextMenu contextMenu;
        private static InteractableObject CurrentInteraction = null;

        // public delegate void OnContextItemSelect(string uuid, string item);
        // public static event OnContextItemSelect ContextItemSelected;

        private bool isMouseOverMe = false;

        private void Awake() {
            contextMenu = this;
            gameObject.SetActive(false);
        }

        private void Update() {
            // if (!isMouseOverMe && (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1)))
            //     ContextMenu.Hide();
        }

        public static void Show<T>(InteractableObject sender, Vector2 pos, string title, T interactable)
            where T : InteractableObject {
            Show(sender, pos, title, InteractableObject.GetInteractableOptions(interactable));
        }

        public static void Show(InteractableObject sender, Vector2 pos, string title,
            IEnumerable<(string title, Sprite icon, Action<object> callback)> description) {
            // This order??
            Hide();
            contextMenu.gameObject.SetActive(true);
            if (IsShowing)
                ResetItems();

            // contextMenu.ContextTitle.text = title == string.Empty ? "" : title; // A bit useless?

            // Spawn in items
            description.ForEach(x => {
                var item =
                    Instantiate(contextMenu.ContextItem, contextMenu.ContentContainer).GetComponent<ContextItem>();
                item.Text = x.title;
                if (x.icon == null) {
                    item.IconImage.color = new Color(0, 0, 0, 0);
                } else {
                    item.Icon = x.icon;
                }
                item.Callback = x.callback;
                item.Creator  = sender;
                contextMenu.SpawnedItems.Add(item.gameObject);
            });

            // Move menu
            contextMenu.GetComponent<RectTransform>().position = pos;

            IsShowing                    = true;
            sender.IsBeingInteractedWith = true;
            CurrentInteraction           = sender;
        }

        public static void Hide() {
            ResetItems();
            if (CurrentInteraction != null) {
                CurrentInteraction.IsBeingInteractedWith = false;
                CurrentInteraction                       = null;
            }

            contextMenu.gameObject.SetActive(false);

            IsShowing = false;
        }

        private static void ResetItems() {
            contextMenu.SpawnedItems.RemoveAll(x => {
                Destroy(x);
                return true;
            });
        }

        public void OnPointerEnter(PointerEventData eventData) => isMouseOverMe = true;

        public void OnPointerExit(PointerEventData eventData) => isMouseOverMe = false;
    }
}
