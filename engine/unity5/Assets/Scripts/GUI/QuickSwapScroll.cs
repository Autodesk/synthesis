using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

    class QuickSwapScroll : MonoBehaviour
    {
        GameObject GObject;
        Vector2 TargetPosition;

        public QuickSwapScroll(GameObject gObject, Vector2 targetPosition)
        {
            GObject = gObject;
            TargetPosition = targetPosition;
        }

        void Update()
        {
            if (GObject.activeSelf == true && Math.Abs(GObject.GetComponent<RectTransform>().anchoredPosition.x - TargetPosition.x) > 3)
            {
                GObject.GetComponent<RectTransform>().anchoredPosition = (GObject.GetComponent<RectTransform>().anchoredPosition.x - TargetPosition.x > 0) ? (Vector3)GObject.GetComponent<RectTransform>().anchoredPosition + new Vector3(-3f, 0f, 0f) : (Vector3)GObject.GetComponent<RectTransform>().anchoredPosition + new Vector3(3f, 0f, 0f);
            }
        }

    }

