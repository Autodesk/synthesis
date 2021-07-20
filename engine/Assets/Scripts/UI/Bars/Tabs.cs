using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tabs : MonoBehaviour
{
    public List<GameObject> tabs = new List<GameObject>();
    private Color darkGray = new Color(0.7f, 0.7f, 0.7f);
    private Color lightGray = new Color(0.9f, 0.9f, 0.9f);

    // Start is called before the first frame update
    void Start()
    {
        DarkenTab(tabs[0].GetComponent<Image>());//darken the home tab as the default start setup
    }
    public void DarkenTab(Image img)//darkens the color of the tab when the button is clicked
    {
        LightenAllTabs();
        img.color = darkGray;
    }
    private void LightenAllTabs() //lightens the color of all tabs
    {
        foreach (GameObject tab in tabs) tab.GetComponent<Image>().color = lightGray;
    }

}
