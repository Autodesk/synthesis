// using System.Collections;
// using System.Collections.Generic;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;

// namespace Synthesis.Simulator.UI
// {
//     public class ScriptableMenu
//     {
//         private RectTransform parentPanel;
//         private RectTransform menuPanel;

//         private float latestEntryHeight = 0.0f;

//         public ScriptableMenu(RectTransform panel, string title)
//         {
//             this.parentPanel = panel;

//             GameObject me = new GameObject(title);
//             me.layer = 5;
//             me.transform.parent = parentPanel.transform;
//             menuPanel = me.AddComponent<RectTransform>();
//             menuPanel.anchorMin = new Vector2(0, 0);
//             menuPanel.anchorMax = new Vector2(1, 1);
//             menuPanel.localScale = new Vector3(1, 1, 1);
//             menuPanel.offsetMin = new Vector2(0, 0);
//             menuPanel.offsetMax = new Vector2(0, 0);
//         }

//         public void AddTitle(string msg, float fontSize)
//         {
//             RectTransform r = CreateEntry(40);
//             //Text text = r.gameObject.AddComponent<Text>();
//             //text.text = msg;
//             //text.font = (Font)Resources.FindObjectsOfTypeAll(typeof(Font))[0];
//             //text.alignment = TextAnchor.MiddleCenter;
//             TextMeshProUGUI txt = r.gameObject.AddComponent<TextMeshProUGUI>();
//             txt.text = msg;
//             txt.fontSize = fontSize;
//             txt.alignment = TextAlignmentOptions.Center;
//         }

//         private RectTransform CreateEntry(float height)
//         {
//             GameObject g = new GameObject("entry_" + ((int)latestEntryHeight).ToString());
//             g.layer = 5;
//             g.transform.parent = menuPanel.transform;
//             RectTransform r = g.AddComponent<RectTransform>();
//             r.localScale = new Vector3(1, 1, 1);
//             r.anchorMin = new Vector2(0.5f, 1);
//             r.anchorMax = new Vector2(0.5f, 1);
//             r.anchoredPosition = new Vector2(0, -((height / 2) + latestEntryHeight));
//             r.pivot = new Vector2(0.5f, 0.5f);
//             r.sizeDelta = new Vector2(menuPanel.rect.width, height);
//             //r.offsetMin = new Vector2(0, 30);
//             //r.offsetMax = new Vector2(150, 0);
//             latestEntryHeight += height;
//             return r;
//         }
//     }
// }