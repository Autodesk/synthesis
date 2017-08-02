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
        Debug.Log("scroll start");
    }


    public void SetTargetPostion(Vector2 targetPosition)
    {
        TargetPosition = targetPosition;

    }
    void Update()
        {
        Debug.Log("scroll");
            if (gameObject.activeSelf == true && Math.Abs(gameObject.GetComponent<RectTransform>().anchoredPosition.x - TargetPosition.x) > 3)
            {
                gameObject.GetComponent<RectTransform>().anchoredPosition = (gameObject.GetComponent<RectTransform>().anchoredPosition.x - TargetPosition.x > 0) ? (Vector3)gameObject.GetComponent<RectTransform>().anchoredPosition + new Vector3(-3f, 0f, 0f) : (Vector3)gameObject.GetComponent<RectTransform>().anchoredPosition + new Vector3(3f, 0f, 0f);
            } else
        {
            Destroy(this);
        }
        }

    }

