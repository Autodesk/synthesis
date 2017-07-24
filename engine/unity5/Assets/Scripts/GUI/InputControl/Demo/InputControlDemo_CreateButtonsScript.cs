using UnityEngine;
using UnityEngine.UI;
using System.Collections.ObjectModel;



public class InputControlDemo_CreateButtonsScript : MonoBehaviour
{
    public GameObject keyNamePrefab;
    public GameObject keyButtonsPrefab;

    private Transform namesTransform;
    private Transform keysTransform;



    // Use this for initialization
    void Start()
    {
        namesTransform = transform.Find("Names");
        keysTransform  = transform.Find("Keys");

        float maxNameWidth  = 0;
        float contentHeight = 4;

        ReadOnlyCollection<KeyMapping> keys = InputControl.getKeysList();

        foreach(KeyMapping key in keys)
        {
            #region Key text
            GameObject keyNameText = Instantiate(keyNamePrefab) as GameObject;
            keyNameText.name = key.name;

            RectTransform keyNameTextRectTransform = keyNameText.GetComponent<RectTransform>();

            keyNameTextRectTransform.transform.SetParent(namesTransform);
            keyNameTextRectTransform.anchoredPosition3D = new Vector3(0, 0, 0);
            keyNameTextRectTransform.localScale         = new Vector3(1, 1, 1);

            Text keyText = keyNameText.GetComponentInChildren<Text>();
            keyText.text = key.name;

            float keyNameWidth = keyText.preferredWidth + 8;

            if (keyNameWidth > maxNameWidth)
            {
                maxNameWidth = keyNameWidth;
            }
            #endregion

            #region Key buttons
            GameObject keyButtons = Instantiate(keyButtonsPrefab) as GameObject;
            keyButtons.name = key.name;

            RectTransform keyButtonsRectTransform = keyButtons.GetComponent<RectTransform>();

            keyButtonsRectTransform.transform.SetParent(keysTransform);
            keyButtonsRectTransform.anchoredPosition3D = new Vector3(0, 0, 0);
            keyButtonsRectTransform.localScale         = new Vector3(1, 1, 1);

            for (int i = 0; i < 3; ++i)
            {
                InputControlDemo_KeyButtonScript buttonScript = keyButtons.transform.GetChild(i).GetComponent<InputControlDemo_KeyButtonScript>();

                buttonScript.keyMapping = key;
                buttonScript.keyIndex   = i;

                buttonScript.updateText();
            }
            #endregion

            contentHeight += 28;
        }

        RectTransform namesRectTransform = namesTransform.GetComponent<RectTransform>();
        RectTransform keysRectTransform  = keysTransform.GetComponent<RectTransform>();
        RectTransform rectTransform      = GetComponent<RectTransform>();

        namesRectTransform.offsetMax = new Vector2(maxNameWidth, 0);
        keysRectTransform.offsetMin  = new Vector2(maxNameWidth, 0);
        rectTransform.offsetMin      = new Vector2(0, -contentHeight);
    }
}
