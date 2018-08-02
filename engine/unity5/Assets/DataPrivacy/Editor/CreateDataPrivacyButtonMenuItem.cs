#if !UNITY_2018_3_OR_NEWER
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace UnityEngine.Analytics
{
    internal static class CreateObjectMenu
    {
        [MenuItem("GameObject/UI/Data Privacy Button", false, 2030)]
        static void CreateDataPrivacyButtonMenuItem(MenuCommand menuCommand)
        {
            GameObject go = CreateButton();
            PlaceUIElementRoot(go, menuCommand);

            var button = go.GetComponent<DataPrivacyButton>();
            var text = button.GetComponentInChildren<Text>();
            text.text = "Open Data Privacy Page";
        }

        // All the code below is more-or-less copy-pasted from MenuOptions and DefaultControls, sorry.
        private static Color s_TextColor = new Color(0.1960784f, 0.1960784f, 0.1960784f, 1f);

        private static GameObject CreateButton()
        {
            //GameObject uiElementRoot = DefaultControls.CreateUIElementRoot("Button", DefaultControls.s_ThickElementSize);
            GameObject uiElementRoot = new GameObject("DataPrivacyButton");
            uiElementRoot.AddComponent<RectTransform>().sizeDelta = new Vector2(180f, 30f);

            GameObject child = new GameObject("Text");
            child.AddComponent<RectTransform>();
            //DefaultControls.SetParentAndAlign(child, uiElementRoot);
            child.transform.SetParent(uiElementRoot.transform, false);
            //DefaultControls.SetLayerRecursively(child, uiElementRoot.layer);
            child.layer = uiElementRoot.layer;

            Image image = uiElementRoot.AddComponent<Image>();
            //image.sprite = resources.standard; // DefaultControls.Resources resources = GetStandardResources()
            image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            image.type = Image.Type.Sliced;
            image.color = new Color(1f, 1f, 1f, 1f);

            var dataPrivacyButton = uiElementRoot.AddComponent<DataPrivacyButton>();
            //DefaultControls.SetDefaultColorTransitionValues((Selectable) dataPrivacyButton);
            ColorBlock colors = dataPrivacyButton.colors;
            colors.highlightedColor = new Color(0.882f, 0.882f, 0.882f);
            colors.pressedColor = new Color(0.698f, 0.698f, 0.698f);
            colors.disabledColor = new Color(0.521f, 0.521f, 0.521f);

            Text lbl = child.AddComponent<Text>();
            lbl.text = "Button";
            lbl.alignment = TextAnchor.MiddleCenter;
            //DefaultControls.SetDefaultTextValues(lbl);
            lbl.color = s_TextColor;
            //lbl.AssignDefaultFont();
            lbl.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            RectTransform component = child.GetComponent<RectTransform>();
            component.anchorMin = Vector2.zero;
            component.anchorMax = Vector2.one;
            component.sizeDelta = Vector2.zero;
            return uiElementRoot;
        }

        private static void PlaceUIElementRoot(GameObject element, MenuCommand menuCommand)
        {
            GameObject parent = menuCommand.context as GameObject;
            if (parent == null || parent.GetComponentInParent<Canvas>() == null)
            {
                parent = GetOrCreateCanvasGameObject();
            }

#if UNITY_4_7
            string uniqueName = element.name;
#else
            string uniqueName = GameObjectUtility.GetUniqueNameForSibling(parent.transform, element.name);
#endif
            element.name = uniqueName;
            Undo.RegisterCreatedObjectUndo(element, "Create " + element.name);
            Undo.SetTransformParent(element.transform, parent.transform, "Parent " + element.name);
            GameObjectUtility.SetParentAndAlign(element, parent);
            if (parent != menuCommand.context)  // not a context click, so center in sceneview
                SetPositionVisibleinSceneView(parent.GetComponent<RectTransform>(), element.GetComponent<RectTransform>());

            Selection.activeGameObject = element;
        }

        // Helper function that returns a Canvas GameObject; preferably a parent of the selection, or other existing Canvas.
        static private GameObject GetOrCreateCanvasGameObject()
        {
            GameObject selectedGo = Selection.activeGameObject;

            // Try to find a gameobject that is the selected GO or one if its parents.
            Canvas canvas = (selectedGo != null) ? selectedGo.GetComponentInParent<Canvas>() : null;
            if (canvas != null && canvas.gameObject.activeInHierarchy)
                return canvas.gameObject;

            // No canvas in selection or its parents? Then use just any canvas..
            canvas = Object.FindObjectOfType(typeof(Canvas)) as Canvas;
            if (canvas != null && canvas.gameObject.activeInHierarchy)
                return canvas.gameObject;

            // No canvas in the scene at all? Then create a new one.
            return CreateNewUI();
        }

        private static void SetPositionVisibleinSceneView(RectTransform canvasRTransform, RectTransform itemTransform)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if ((Object)sceneView == (Object)null && SceneView.sceneViews.Count > 0)
                sceneView = SceneView.sceneViews[0] as SceneView;
            if ((Object)sceneView == (Object)null || (Object)sceneView.camera == (Object)null)
                return;
            Camera camera = sceneView.camera;
            Vector3 zero = Vector3.zero;
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRTransform, new Vector2((float)(camera.pixelWidth / 2), (float)(camera.pixelHeight / 2)), camera, out localPoint))
            {
                localPoint.x += canvasRTransform.sizeDelta.x * canvasRTransform.pivot.x;
                localPoint.y += canvasRTransform.sizeDelta.y * canvasRTransform.pivot.y;
                localPoint.x = Mathf.Clamp(localPoint.x, 0.0f, canvasRTransform.sizeDelta.x);
                localPoint.y = Mathf.Clamp(localPoint.y, 0.0f, canvasRTransform.sizeDelta.y);
                zero.x = localPoint.x - canvasRTransform.sizeDelta.x * itemTransform.anchorMin.x;
                zero.y = localPoint.y - canvasRTransform.sizeDelta.y * itemTransform.anchorMin.y;
                Vector3 vector3_1;
                vector3_1.x = (float)((double)canvasRTransform.sizeDelta.x * (0.0 - (double)canvasRTransform.pivot.x) + (double)itemTransform.sizeDelta.x * (double)itemTransform.pivot.x);
                vector3_1.y = (float)((double)canvasRTransform.sizeDelta.y * (0.0 - (double)canvasRTransform.pivot.y) + (double)itemTransform.sizeDelta.y * (double)itemTransform.pivot.y);
                Vector3 vector3_2;
                vector3_2.x = (float)((double)canvasRTransform.sizeDelta.x * (1.0 - (double)canvasRTransform.pivot.x) - (double)itemTransform.sizeDelta.x * (double)itemTransform.pivot.x);
                vector3_2.y = (float)((double)canvasRTransform.sizeDelta.y * (1.0 - (double)canvasRTransform.pivot.y) - (double)itemTransform.sizeDelta.y * (double)itemTransform.pivot.y);
                zero.x = Mathf.Clamp(zero.x, vector3_1.x, vector3_2.x);
                zero.y = Mathf.Clamp(zero.y, vector3_1.y, vector3_2.y);
            }

            itemTransform.anchoredPosition = (Vector2)zero;
            itemTransform.localRotation = Quaternion.identity;
            itemTransform.localScale = Vector3.one;
        }

        private static GameObject CreateNewUI()
        {
            GameObject gameObject = new GameObject("Canvas");
            gameObject.layer = LayerMask.NameToLayer("UI");
            gameObject.AddComponent<Canvas>().renderMode = UnityEngine.RenderMode.ScreenSpaceOverlay;
            gameObject.AddComponent<CanvasScaler>();
            gameObject.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo((Object)gameObject, "Create " + gameObject.name);
            CreateEventSystem(false, null);
            return gameObject;
        }

        private static void CreateEventSystem(bool select, GameObject parent)
        {
            EventSystem eventSystem = Object.FindObjectOfType<EventSystem>();
            if ((Object)eventSystem == (Object)null)
            {
                GameObject child = new GameObject("EventSystem");
                GameObjectUtility.SetParentAndAlign(child, parent);
                eventSystem = child.AddComponent<EventSystem>();
                child.AddComponent<StandaloneInputModule>();
#if !UNITY_5_3 && !UNITY_5_4_OR_NEWER
                child.AddComponent<TouchInputModule>();
#endif
                Undo.RegisterCreatedObjectUndo((Object)child, "Create " + child.name);
            }
            if (!select || !((Object)eventSystem != (Object)null))
                return;
            Selection.activeGameObject = eventSystem.gameObject;
        }
    }
}
#endif //!UNITY_2018_3_OR_NEWER
