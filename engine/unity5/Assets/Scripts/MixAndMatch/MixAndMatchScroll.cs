using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

    class MixAndMatchScroll : MonoBehaviour
    {
        Vector2 TargetPosition;

    private void Start()
    {
    }


    public void SetTargetPostion(Vector2 targetPosition)
    {
        TargetPosition = targetPosition;
        if (gameObject.name == "PneumaticWheel")
        {
            Debug.Log("Pneumatic wheel target position: " + targetPosition);
        }

    }
    void Update()
        {
            float distance = gameObject.GetComponent<RectTransform>().anchoredPosition.x - TargetPosition.x;

            if (gameObject.activeSelf == true && Math.Abs(distance) > 6)
            {
                gameObject.GetComponent<RectTransform>().anchoredPosition = (gameObject.GetComponent<RectTransform>().anchoredPosition.x - TargetPosition.x > 0) ? (Vector3)gameObject.GetComponent<RectTransform>().anchoredPosition + new Vector3(-6f, 0f, 0f) : (Vector3)gameObject.GetComponent<RectTransform>().anchoredPosition + new Vector3(6f, 0f, 0f);
            } else
            {
            Destroy(this);
            }
        }

    }

