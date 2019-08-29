using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;

public class AnalyticsChecker : MonoBehaviour
{
    public GameObject analyticsManagerPrefab;

    public void Start()
    {
        if (GameObject.FindGameObjectWithTag("AM") == null)
        {
            Instantiate(analyticsManagerPrefab);
        }

        Destroy(gameObject);
    }
}
