using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalizationHelperr : MonoBehaviour
{
    [Tooltip("The drop down box to populate with camera view icons")]
    public Dropdown dropDown;

    [Tooltip("The icons of the different camera views")]
    public Sprite[] icons;

    void Start()
    {
        dropDown.ClearOptions();

        List<Dropdown.OptionData> iconItems = new List<Dropdown.OptionData>();

        foreach (var icon in icons)
        {

            string iconName = icon.name;
            int dot = icon.name.IndexOf('.');
            if (dot>=0)
            {
                iconName = iconName.Substring(dot + 1);
            }

            var iconOption = new Dropdown.OptionData(icon.name, icon);
            iconItems.Add(iconOption);
        }

        dropDown.AddOptions(iconItems);
    }

}
