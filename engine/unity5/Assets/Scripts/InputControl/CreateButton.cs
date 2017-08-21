using UnityEngine;
using UnityEngine.UI;
using System.Collections.ObjectModel;
using System.Collections.Generic;

//Adapted from: https://github.com/Gris87/InputControl

public class CreateButton : MonoBehaviour
{
    public GameObject tankDriveSwitch;
    public GameObject unitConversionSwitch;

    public GameObject keyNamePrefab;
    public GameObject keyButtonsPrefab;
    public List<GameObject> keyButtonList;

    private Transform namesTransform;
    private Transform keysTransform;

    // Use this for initialization
    void Start()
    {
        DestroyList();
        tankDriveSwitch = AuxFunctions.FindObject("TankDriveSwitch");
        unitConversionSwitch = AuxFunctions.FindObject("UnitConversionSwitch");
        //Can change the default measure HERE and also change the default value in the slider game object in main menu
        PlayerPrefs.SetString("Measure", "Metric");
        GameObject.Find("SettingsMode").GetComponent<SettingsMode>().UpdateAllText();
        namesTransform = transform.Find("Names");
        keysTransform = transform.Find("Keys");

        float maxNameWidth = 0;
        float contentHeight = 4;

        //Defaults to player one's keys
        ReadOnlyCollection<KeyMapping> keys = InputControl.getPlayerKeys(0);

        foreach (KeyMapping key in keys)
        {
            //===============================Key Text vs Key Buttons==================================
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

    //==============================================================================================
    //                                          Update Functions
    // The following functions are almost identical EXCEPT for the ReadOnlyCollection line.
    // Each function will retrieve the specified player list and generate controls for that player.
    // Each player is specified with a playerIndex and are retrieved by this index.
    //==============================================================================================

    #region Update Active Buttons
    public void UpdateActiveButtons()
    {
        DestroyList();

        float maxNameWidth = 0;
        float contentHeight = 4;

        //Retrieves and updates the active player's keys
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

        GameObject.Find("SettingsMode").GetComponent<SettingsMode>().UpdateButtonStyle();
    }
    #endregion

    #region Update Player One Keys
    public void UpdatePlayerOneButtons()
    {
        DestroyList();

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
    #endregion

    #region Update Player Two Keys
    public void UpdatePlayerTwoButtons()
    {
        DestroyList();

        float maxNameWidth = 0;
        float contentHeight = 4;

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
    /// Call before retrieving a new player.
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

    public void ResetTankDrive()
    {
        DestroyList();
        InputControl.mPlayerList[InputControl.activePlayerIndex].ResetTank();
        UpdateActiveButtons();
    }

    public void ResetArcadeDrive()
    {
        DestroyList();
        InputControl.mPlayerList[InputControl.activePlayerIndex].ResetArcade();
        UpdateActiveButtons();
    }

    /// <summary>
    /// Toggles the Tank slider between on/off for each player.
    /// </summary>
    public void TankSlider()
    {
        //tankDriveSwitch = AuxFunctions.FindObject("TankDriveSwitch");
        int i = (int)tankDriveSwitch.GetComponent<Slider>().value;

        switch (i)
        {
            case 0:  //Tank Drive slider is OFF
                InputControl.mPlayerList[InputControl.activePlayerIndex].SetArcadeDrive();
                UpdateActiveButtons();
                Controls.TankDriveEnabled = false;
                break;
            case 1:  //Tank Drive slider is ON
                InputControl.mPlayerList[InputControl.activePlayerIndex].SetTankDrive();
                UpdateActiveButtons();
                Controls.TankDriveEnabled = true;
                break;
            default: //Defaults to Arcade Drive
                InputControl.mPlayerList[InputControl.activePlayerIndex].SetArcadeDrive();
                UpdateActiveButtons();
                Controls.TankDriveEnabled = false;
                break;
        }
    }

    /// <summary>
    /// Set the player preference for measurement units
    /// </summary>
    public void UnitConversionSlider()
    {
        int i = (int)unitConversionSwitch.GetComponent<Slider>().value;

        switch (i)
        {
            case 0:  //unit conversion slider is OFF
                PlayerPrefs.SetString("Measure", "Imperial");
                break;
            case 1:  //unit conversion slider is ON
                PlayerPrefs.SetString("Measure", "Metric");
                break;
        }
    }
    /// <summary>
    /// Updates Tank Drive Slider from MainMenu to the active Scene.
    /// </summary>
    public void OnEnable()
    {
        tankDriveSwitch = AuxFunctions.FindObject("TankDriveSwitch");
        tankDriveSwitch.GetComponent<Slider>().value = InputControl.mPlayerList[InputControl.activePlayerIndex].isTankDrive ? 1 : 0;
        unitConversionSwitch = AuxFunctions.FindObject("UnitConversionSwitch");
        unitConversionSwitch.GetComponent<Slider>().value = PlayerPrefs.GetString("Measure").Equals("Metric") ? 1 : 0;
    }

    /// <summary>
    /// Updates slider. Called when players are switched to check for their drive settings.
    /// </summary>
    public void UpdateTankSlider()
    {
        //tankDriveSwitch = AuxFunctions.FindObject("TankDriveSwitch");
        tankDriveSwitch.GetComponent<Slider>().value = InputControl.mPlayerList[InputControl.activePlayerIndex].isTankDrive ? 1 : 0;
    }
}