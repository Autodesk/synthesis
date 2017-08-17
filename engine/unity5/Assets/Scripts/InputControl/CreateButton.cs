﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.ObjectModel;
using System.Collections.Generic;

public class CreateButton : MonoBehaviour
{
    GameObject tankDriveSwitch;
    public GameObject keyNamePrefab;
    public GameObject keyButtonsPrefab;
    public List<GameObject> keyButtonList;
    private Transform namesTransform;
    private Transform keysTransform;

    // Use this for initialization
    void Start()
    {
        DestroyList();

        namesTransform = transform.Find("Names");
        keysTransform = transform.Find("Keys");

        float maxNameWidth = 0;
        float contentHeight = 4;

        ReadOnlyCollection<KeyMapping> keys = InputControl.getPlayerKeys(0);

        foreach (KeyMapping key in keys)
        {
            //******************************Key Text vs Key Buttons***********************************
            //Key Text: The labels/text in the first column of the InputManager menu (see Options tab)
            //Key Buttons: The buttons in the second and third column of the Input Manager menu
            #region Key text
            GameObject keyNameText = Instantiate(keyNamePrefab) as GameObject;
            keyNameText.name = key.name;

            RectTransform keyNameTextRectTransform = keyNameText.GetComponent<RectTransform>();

            keyNameTextRectTransform.transform.SetParent(namesTransform);
            keyNameTextRectTransform.anchoredPosition3D = new Vector3(0, 0, 0);
            keyNameTextRectTransform.localScale = new Vector3(1, 1, 1);

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
            keyButtonsRectTransform.localScale = new Vector3(1, 1, 1);

            for (int i = 0; i < 2; ++i)
            {
                KeyButton buttonScript = keyButtons.transform.GetChild(i).GetComponent<KeyButton>();

                buttonScript.keyMapping = key;
                buttonScript.keyIndex = i;

                buttonScript.UpdateText();
            }
            #endregion

            contentHeight += 28;
        }

        RectTransform namesRectTransform = namesTransform.GetComponent<RectTransform>();
        RectTransform keysRectTransform = keysTransform.GetComponent<RectTransform>();
        RectTransform rectTransform = GetComponent<RectTransform>();

        namesRectTransform.offsetMax = new Vector2(maxNameWidth, 0);
        keysRectTransform.offsetMin = new Vector2(maxNameWidth, 0);
        rectTransform.offsetMin = new Vector2(0, -contentHeight);
    }

    #region UpdateButtons
    public void UpdateActiveButtons()
    {
        DestroyList();

        float maxNameWidth = 0;
        float contentHeight = 4;

        ReadOnlyCollection<KeyMapping> keys = InputControl.getActivePlayerKeys();

        foreach (KeyMapping key in keys)
        {
            //******************************Key Text vs Key Buttons***********************************
            //Key Text: The labels/text in the first column of the InputManager menu (see Options tab)
            //Key Buttons: The buttons in the second and third column of the Input Manager menu
            #region Key text
            GameObject keyNameText = Instantiate(keyNamePrefab) as GameObject;
            keyNameText.name = key.name;

            RectTransform keyNameTextRectTransform = keyNameText.GetComponent<RectTransform>();

            keyNameTextRectTransform.transform.SetParent(namesTransform);
            keyNameTextRectTransform.anchoredPosition3D = new Vector3(0, 0, 0);
            keyNameTextRectTransform.localScale = new Vector3(1, 1, 1);

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
            keyButtonsRectTransform.localScale = new Vector3(1, 1, 1);

            for (int i = 0; i < 2; ++i)
            {
                KeyButton buttonScript = keyButtons.transform.GetChild(i).GetComponent<KeyButton>();

                buttonScript.keyMapping = key;
                buttonScript.keyIndex = i;

                buttonScript.UpdateText();
            }
            #endregion

            contentHeight += 28;
        }

        RectTransform namesRectTransform = namesTransform.GetComponent<RectTransform>();
        RectTransform keysRectTransform = keysTransform.GetComponent<RectTransform>();
        RectTransform rectTransform = GetComponent<RectTransform>();

        namesRectTransform.offsetMax = new Vector2(maxNameWidth, 0);
        keysRectTransform.offsetMin = new Vector2(maxNameWidth, 0);
        rectTransform.offsetMin = new Vector2(0, -contentHeight);
    }
    #endregion

    #region Update Player One Keys
    public void UpdatePlayerOneButtons()
    {
        DestroyList();

        float maxNameWidth = 0;
        float contentHeight = 4;

        //Reads the main keys list: getKeysList()
        ReadOnlyCollection<KeyMapping> keys = InputControl.getPlayerKeys(0);

        foreach (KeyMapping key in keys)
        {
            //******************************Key Text vs Key Buttons***********************************
            //Key Text: The labels/text in the first column of the InputManager menu (see Options tab)
            //Key Buttons: The buttons in the second and third column of the Input Manager menu
            #region Key text
            GameObject keyNameText = Instantiate(keyNamePrefab) as GameObject;
            keyNameText.name = key.name;

            RectTransform keyNameTextRectTransform = keyNameText.GetComponent<RectTransform>();

            keyNameTextRectTransform.transform.SetParent(namesTransform);
            keyNameTextRectTransform.anchoredPosition3D = new Vector3(0, 0, 0);
            keyNameTextRectTransform.localScale = new Vector3(1, 1, 1);

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
            keyButtonsRectTransform.localScale = new Vector3(1, 1, 1);

            for (int i = 0; i < 2; ++i)
            {
                KeyButton buttonScript = keyButtons.transform.GetChild(i).GetComponent<KeyButton>();

                buttonScript.keyMapping = key;
                buttonScript.keyIndex = i;

                buttonScript.UpdateText();
            }
            #endregion

            contentHeight += 28;
        }

        RectTransform namesRectTransform = namesTransform.GetComponent<RectTransform>();
        RectTransform keysRectTransform = keysTransform.GetComponent<RectTransform>();
        RectTransform rectTransform = GetComponent<RectTransform>();

        namesRectTransform.offsetMax = new Vector2(maxNameWidth, 0);
        keysRectTransform.offsetMin = new Vector2(maxNameWidth, 0);
        rectTransform.offsetMin = new Vector2(0, -contentHeight);
    }
    #endregion

    #region Update Player Two Keys
    public void UpdatePlayerTwoButtons()
    {
        DestroyList();

        float maxNameWidth = 0;
        float contentHeight = 4;

        //Reads the main keys list: getKeysList()
        ReadOnlyCollection<KeyMapping> keys = InputControl.getPlayerKeys(1);

        foreach (KeyMapping key in keys)
        {
            //******************************Key Text vs Key Buttons***********************************
            //Key Text: The labels/text in the first column of the InputManager menu (see Options tab)
            //Key Buttons: The buttons in the second and third column of the Input Manager menu
            #region Key text
            GameObject keyNameText = Instantiate(keyNamePrefab) as GameObject;
            keyNameText.name = key.name;

            RectTransform keyNameTextRectTransform = keyNameText.GetComponent<RectTransform>();

            keyNameTextRectTransform.transform.SetParent(namesTransform);
            keyNameTextRectTransform.anchoredPosition3D = new Vector3(0, 0, 0);
            keyNameTextRectTransform.localScale = new Vector3(1, 1, 1);

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
            keyButtonsRectTransform.localScale = new Vector3(1, 1, 1);

            for (int i = 0; i < 2; ++i)
            {
                KeyButton buttonScript = keyButtons.transform.GetChild(i).GetComponent<KeyButton>();

                buttonScript.keyMapping = key;
                buttonScript.keyIndex = i;

                buttonScript.UpdateText();
            }
            #endregion

            contentHeight += 28;
        }

        RectTransform namesRectTransform = namesTransform.GetComponent<RectTransform>();
        RectTransform keysRectTransform = keysTransform.GetComponent<RectTransform>();
        RectTransform rectTransform = GetComponent<RectTransform>();

        namesRectTransform.offsetMax = new Vector2(maxNameWidth, 0);
        keysRectTransform.offsetMin = new Vector2(maxNameWidth, 0);
        rectTransform.offsetMin = new Vector2(0, -contentHeight);
    }
    #endregion

    #region Update Player Three Keys
    public void UpdatePlayerThreeButtons()
    {
        DestroyList();

        float maxNameWidth = 0;
        float contentHeight = 4;

        //Reads the main keys list: getKeysList()
        ReadOnlyCollection<KeyMapping> keys = InputControl.getPlayerKeys(2);

        foreach (KeyMapping key in keys)
        {
            //******************************Key Text vs Key Buttons***********************************
            //Key Text: The labels/text in the first column of the InputManager menu (see Options tab)
            //Key Buttons: The buttons in the second and third column of the Input Manager menu
            #region Key text
            GameObject keyNameText = Instantiate(keyNamePrefab) as GameObject;
            keyNameText.name = key.name;

            RectTransform keyNameTextRectTransform = keyNameText.GetComponent<RectTransform>();

            keyNameTextRectTransform.transform.SetParent(namesTransform);
            keyNameTextRectTransform.anchoredPosition3D = new Vector3(0, 0, 0);
            keyNameTextRectTransform.localScale = new Vector3(1, 1, 1);

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
            keyButtonsRectTransform.localScale = new Vector3(1, 1, 1);

            for (int i = 0; i < 2; ++i)
            {
                KeyButton buttonScript = keyButtons.transform.GetChild(i).GetComponent<KeyButton>();

                buttonScript.keyMapping = key;
                buttonScript.keyIndex = i;

                buttonScript.UpdateText();
            }
            #endregion

            contentHeight += 28;
        }

        RectTransform namesRectTransform = namesTransform.GetComponent<RectTransform>();
        RectTransform keysRectTransform = keysTransform.GetComponent<RectTransform>();
        RectTransform rectTransform = GetComponent<RectTransform>();

        namesRectTransform.offsetMax = new Vector2(maxNameWidth, 0);
        keysRectTransform.offsetMin = new Vector2(maxNameWidth, 0);
        rectTransform.offsetMin = new Vector2(0, -contentHeight);
    }
    #endregion

    #region Update Player Four Keys
    public void UpdatePlayerFourButtons()
    {
        DestroyList();

        float maxNameWidth = 0;
        float contentHeight = 4;

        //Reads the main keys list: getKeysList()
        ReadOnlyCollection<KeyMapping> keys = InputControl.getPlayerKeys(3);

        foreach (KeyMapping key in keys)
        {
            //******************************Key Text vs Key Buttons***********************************
            //Key Text: The labels/text in the first column of the InputManager menu (see Options tab)
            //Key Buttons: The buttons in the second and third column of the Input Manager menu
            #region Key text
            GameObject keyNameText = Instantiate(keyNamePrefab) as GameObject;
            keyNameText.name = key.name;

            RectTransform keyNameTextRectTransform = keyNameText.GetComponent<RectTransform>();

            keyNameTextRectTransform.transform.SetParent(namesTransform);
            keyNameTextRectTransform.anchoredPosition3D = new Vector3(0, 0, 0);
            keyNameTextRectTransform.localScale = new Vector3(1, 1, 1);

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
            keyButtonsRectTransform.localScale = new Vector3(1, 1, 1);

            for (int i = 0; i < 2; ++i)
            {
                KeyButton buttonScript = keyButtons.transform.GetChild(i).GetComponent<KeyButton>();

                buttonScript.keyMapping = key;
                buttonScript.keyIndex = i;

                buttonScript.UpdateText();
            }
            #endregion

            contentHeight += 28;
        }

        RectTransform namesRectTransform = namesTransform.GetComponent<RectTransform>();
        RectTransform keysRectTransform = keysTransform.GetComponent<RectTransform>();
        RectTransform rectTransform = GetComponent<RectTransform>();

        namesRectTransform.offsetMax = new Vector2(maxNameWidth, 0);
        keysRectTransform.offsetMin = new Vector2(maxNameWidth, 0);
        rectTransform.offsetMin = new Vector2(0, -contentHeight);
    }
    #endregion

    #region Update Player Five Keys
    public void UpdatePlayerFiveButtons()
    {
        DestroyList();

        float maxNameWidth = 0;
        float contentHeight = 4;

        //Reads the main keys list: getKeysList()
        ReadOnlyCollection<KeyMapping> keys = InputControl.getPlayerKeys(4);

        foreach (KeyMapping key in keys)
        {
            //******************************Key Text vs Key Buttons***********************************
            //Key Text: The labels/text in the first column of the InputManager menu (see Options tab)
            //Key Buttons: The buttons in the second and third column of the Input Manager menu
            #region Key text
            GameObject keyNameText = Instantiate(keyNamePrefab) as GameObject;
            keyNameText.name = key.name;

            RectTransform keyNameTextRectTransform = keyNameText.GetComponent<RectTransform>();

            keyNameTextRectTransform.transform.SetParent(namesTransform);
            keyNameTextRectTransform.anchoredPosition3D = new Vector3(0, 0, 0);
            keyNameTextRectTransform.localScale = new Vector3(1, 1, 1);

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
            keyButtonsRectTransform.localScale = new Vector3(1, 1, 1);

            for (int i = 0; i < 2; ++i)
            {
                KeyButton buttonScript = keyButtons.transform.GetChild(i).GetComponent<KeyButton>();

                buttonScript.keyMapping = key;
                buttonScript.keyIndex = i;

                buttonScript.UpdateText();
            }
            #endregion

            contentHeight += 28;
        }

        RectTransform namesRectTransform = namesTransform.GetComponent<RectTransform>();
        RectTransform keysRectTransform = keysTransform.GetComponent<RectTransform>();
        RectTransform rectTransform = GetComponent<RectTransform>();

        namesRectTransform.offsetMax = new Vector2(maxNameWidth, 0);
        keysRectTransform.offsetMin = new Vector2(maxNameWidth, 0);
        rectTransform.offsetMin = new Vector2(0, -contentHeight);
    }
    #endregion

    #region Update Player Six Keys
    public void UpdatePlayerSixButtons()
    {
        DestroyList();

        float maxNameWidth = 0;
        float contentHeight = 4;

        //Reads the main keys list: getKeysList()
        ReadOnlyCollection<KeyMapping> keys = InputControl.getPlayerKeys(5);

        foreach (KeyMapping key in keys)
        {
            //******************************Key Text vs Key Buttons***********************************
            //Key Text: The labels/text in the first column of the InputManager menu (see Options tab)
            //Key Buttons: The buttons in the second and third column of the Input Manager menu
            #region Key text
            GameObject keyNameText = Instantiate(keyNamePrefab) as GameObject;
            keyNameText.name = key.name;

            RectTransform keyNameTextRectTransform = keyNameText.GetComponent<RectTransform>();

            keyNameTextRectTransform.transform.SetParent(namesTransform);
            keyNameTextRectTransform.anchoredPosition3D = new Vector3(0, 0, 0);
            keyNameTextRectTransform.localScale = new Vector3(1, 1, 1);

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
            keyButtonsRectTransform.localScale = new Vector3(1, 1, 1);

            for (int i = 0; i < 2; ++i)
            {
                KeyButton buttonScript = keyButtons.transform.GetChild(i).GetComponent<KeyButton>();

                buttonScript.keyMapping = key;
                buttonScript.keyIndex = i;

                buttonScript.UpdateText();
            }
            #endregion

            contentHeight += 28;
        }

        RectTransform namesRectTransform = namesTransform.GetComponent<RectTransform>();
        RectTransform keysRectTransform = keysTransform.GetComponent<RectTransform>();
        RectTransform rectTransform = GetComponent<RectTransform>();

        namesRectTransform.offsetMax = new Vector2(maxNameWidth, 0);
        keysRectTransform.offsetMin = new Vector2(maxNameWidth, 0);
        rectTransform.offsetMin = new Vector2(0, -contentHeight);
    }
    #endregion

    /// <summary>
    /// Destroys old lists before regenerating a new list.
    /// </summary>
    public void DestroyList()
    {
        namesTransform = transform.Find("Names");
        keysTransform = transform.Find("Keys");

        foreach (Transform child in namesTransform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in keysTransform)
        {
            Destroy(child.gameObject);
        }
    }

    public void TankToggle()
    {
        tankDriveSwitch = AuxFunctions.FindObject("TankDriveSwitch");
        int i = (int)tankDriveSwitch.GetComponent<Slider>().value;

        switch (i)
        {
            case 0:
                InputControl.mPlayerList[InputControl.activePlayerIndex].SetArcadeDrive();
                UpdateActiveButtons();
                break;
            case 1:
                InputControl.mPlayerList[InputControl.activePlayerIndex].SetTankDrive();
                UpdateActiveButtons();
                Controls.TankDriveEnabled = true;
                break;
            default:
                InputControl.mPlayerList[InputControl.activePlayerIndex].SetArcadeDrive();
                UpdateActiveButtons();
                break;
        }
    }

    public void OnEnable()
    {
        tankDriveSwitch = AuxFunctions.FindObject("TankDriveSwitch");
        tankDriveSwitch.GetComponent<Slider>().value = InputControl.mPlayerList[InputControl.activePlayerIndex].isTankDrive ? 1 : 0;
    }

    public void UpdateSlider()
    {
        tankDriveSwitch = AuxFunctions.FindObject("TankDriveSwitch");
        tankDriveSwitch.GetComponent<Slider>().value = InputControl.mPlayerList[InputControl.activePlayerIndex].isTankDrive ? 1 : 0;
    }
}