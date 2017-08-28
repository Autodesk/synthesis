using UnityEngine;
using UnityEngine.UI;
using System.Collections.ObjectModel;
using System.Collections.Generic;

//=========================================================================================
//                                      CreateButton.cs
// Description: Generates buttons and button text for the each player in the control panel.
// Main Content:
//     UpdateActiveButtons(): Creates and updates the active player's buttons/controls.
//     UpdatePlayerOne() - UpdatePlayerSix(): Creates and updates the specified player's controls.
//     Toggles - Utilized by players to toggle their player preferances.
//     Functions - Various functions supporting these features.
// Adapted from: https://github.com/Gris87/InputControl
//=========================================================================================

public class CreateButton : MonoBehaviour
{
    //Toggle Switches
    public GameObject tankDriveSwitch; 
    public GameObject unitConversionSwitch;

    public GameObject keyNamePrefab;
    public GameObject keyButtonsPrefab;

    private Transform namesTransform; //The string name of the control (the first column of the control panel; non-button)
    private Transform keysTransform; //The buttons of the controls (column 2 and column 3 of the control panel)

    // Use this for initialization
    void Start()
    {
        DestroyList();

        tankDriveSwitch = AuxFunctions.FindObject("TankDriveSwitch");
        unitConversionSwitch = AuxFunctions.FindObject("UnitConversionSwitch");

        //Can change the default measurement HERE and also change the default value in the slider game object in main menu
        PlayerPrefs.SetString("Measure", "Metric");
        GameObject.Find("SettingsMode").GetComponent<SettingsMode>().UpdateAllText();

        namesTransform = transform.Find("Names");
        keysTransform = transform.Find("Keys");

        float maxNameWidth = 0;
        float contentHeight = 4;

        //Defaults to player one's keys at Start
        ReadOnlyCollection<KeyMapping> keys = InputControl.getPlayerKeys(0);

        foreach (KeyMapping key in keys)
        {
            //========================================================================================
            //                                   Key Text vs Key Buttons
            //Key Text: The labels/text in the first column of the InputManager menu (see Options tab)
            //Key Buttons: The buttons in the second and third column of the Input Manager menu
            //========================================================================================

            //Source: https://github.com/Gris87/InputControl
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
            //==============================================

            contentHeight += 28;
        }

        RectTransform namesRectTransform = namesTransform.GetComponent<RectTransform>();
        RectTransform keysRectTransform = keysTransform.GetComponent<RectTransform>();
        RectTransform rectTransform = GetComponent<RectTransform>();

        namesRectTransform.offsetMax = new Vector2(maxNameWidth, 0);
        keysRectTransform.offsetMin = new Vector2(maxNameWidth, 0);
        rectTransform.sizeDelta = new Vector2(0, contentHeight); //Controls the height on the control panel

        //Updates the current active player's button (at start, we default to player one.)
        GameObject.Find("SettingsMode").GetComponent<SettingsMode>().UpdateButtonStyle();

        //Loads controls (if changed in another scene) and updates their button text.
        Controls.Load();
        GameObject.Find("Content").GetComponent<CreateButton>().UpdatePlayerOneButtons();
        GameObject.Find("SettingsMode").GetComponent<SettingsMode>().UpdateAllText();
    }

    //==============================================================================================
    //                                       Update Functions
    // The following functions are almost identical EXCEPT for the ReadOnlyCollection line.
    // Each function will retrieve control information for the specified player list and create control 
    // input buttons for that player. Each player is specified by a playerIndex and the specific player's
    // list can be called with this index 0 (player one) - index 5 (player six).
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
            //========================================================================================
            //                                   Key Text vs Key Buttons
            //Key Text: The labels/text in the first column of the InputManager menu (see Options tab)
            //Key Buttons: The buttons in the second and third column of the Input Manager menu
            //========================================================================================

            //Source: https://github.com/Gris87/InputControl
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
            //=============================================

            contentHeight += 28;
        }

        RectTransform namesRectTransform = namesTransform.GetComponent<RectTransform>();
        RectTransform keysRectTransform = keysTransform.GetComponent<RectTransform>();
        RectTransform rectTransform = GetComponent<RectTransform>();

        namesRectTransform.offsetMax = new Vector2(maxNameWidth, 0);
        keysRectTransform.offsetMin = new Vector2(maxNameWidth, 0);
        rectTransform.sizeDelta = new Vector2(0, contentHeight);

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
            //========================================================================================
            //                                   Key Text vs Key Buttons
            //Key Text: The labels/text in the first column of the InputManager menu (see Options tab)
            //Key Buttons: The buttons in the second and third column of the Input Manager menu
            //========================================================================================

            //Source: https://github.com/Gris87/InputControl
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
            //==============================================

            contentHeight += 28;
        }

        RectTransform namesRectTransform = namesTransform.GetComponent<RectTransform>();
        RectTransform keysRectTransform = keysTransform.GetComponent<RectTransform>();
        RectTransform rectTransform = GetComponent<RectTransform>();

        namesRectTransform.offsetMax = new Vector2(maxNameWidth, 0);
        keysRectTransform.offsetMin = new Vector2(maxNameWidth, 0);
        rectTransform.sizeDelta = new Vector2(0, contentHeight);

        GameObject.Find("SettingsMode").GetComponent<SettingsMode>().UpdateButtonStyle();
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
            //========================================================================================
            //                                   Key Text vs Key Buttons
            //Key Text: The labels/text in the first column of the InputManager menu (see Options tab)
            //Key Buttons: The buttons in the second and third column of the Input Manager menu
            //========================================================================================

            //Source: https://github.com/Gris87/InputControl
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
            //==============================================

            contentHeight += 28;
        }

        RectTransform namesRectTransform = namesTransform.GetComponent<RectTransform>();
        RectTransform keysRectTransform = keysTransform.GetComponent<RectTransform>();
        RectTransform rectTransform = GetComponent<RectTransform>();

        namesRectTransform.offsetMax = new Vector2(maxNameWidth, 0);
        keysRectTransform.offsetMin = new Vector2(maxNameWidth, 0);
        rectTransform.sizeDelta = new Vector2(0, contentHeight);

        GameObject.Find("SettingsMode").GetComponent<SettingsMode>().UpdateButtonStyle();
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
            //========================================================================================
            //                                   Key Text vs Key Buttons
            //Key Text: The labels/text in the first column of the InputManager menu (see Options tab)
            //Key Buttons: The buttons in the second and third column of the Input Manager menu
            //========================================================================================

            //Source: https://github.com/Gris87/InputControl
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
            //===============================================

            contentHeight += 28;
        }

        RectTransform namesRectTransform = namesTransform.GetComponent<RectTransform>();
        RectTransform keysRectTransform = keysTransform.GetComponent<RectTransform>();
        RectTransform rectTransform = GetComponent<RectTransform>();

        namesRectTransform.offsetMax = new Vector2(maxNameWidth, 0);
        keysRectTransform.offsetMin = new Vector2(maxNameWidth, 0);
        rectTransform.sizeDelta = new Vector2(0, contentHeight);

        GameObject.Find("SettingsMode").GetComponent<SettingsMode>().UpdateButtonStyle();
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
            //========================================================================================
            //                                   Key Text vs Key Buttons
            //Key Text: The labels/text in the first column of the InputManager menu (see Options tab)
            //Key Buttons: The buttons in the second and third column of the Input Manager menu
            //========================================================================================

            //Source: https://github.com/Gris87/InputControl
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
            //=============================================

            contentHeight += 28;
        }

        RectTransform namesRectTransform = namesTransform.GetComponent<RectTransform>();
        RectTransform keysRectTransform = keysTransform.GetComponent<RectTransform>();
        RectTransform rectTransform = GetComponent<RectTransform>();

        namesRectTransform.offsetMax = new Vector2(maxNameWidth, 0);
        keysRectTransform.offsetMin = new Vector2(maxNameWidth, 0);
        rectTransform.sizeDelta = new Vector2(0, contentHeight);

        GameObject.Find("SettingsMode").GetComponent<SettingsMode>().UpdateButtonStyle();
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
            //========================================================================================
            //                                   Key Text vs Key Buttons
            //Key Text: The labels/text in the first column of the InputManager menu (see Options tab)
            //Key Buttons: The buttons in the second and third column of the Input Manager menu
            //========================================================================================

            //Source: https://github.com/Gris87/InputControl
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
            //===============================================

            contentHeight += 28;
        }

        RectTransform namesRectTransform = namesTransform.GetComponent<RectTransform>();
        RectTransform keysRectTransform = keysTransform.GetComponent<RectTransform>();
        RectTransform rectTransform = GetComponent<RectTransform>();

        namesRectTransform.offsetMax = new Vector2(maxNameWidth, 0);
        keysRectTransform.offsetMin = new Vector2(maxNameWidth, 0);
        rectTransform.sizeDelta = new Vector2(0, contentHeight);

        GameObject.Find("SettingsMode").GetComponent<SettingsMode>().UpdateButtonStyle();
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
            //========================================================================================
            //                                   Key Text vs Key Buttons
            //Key Text: The labels/text in the first column of the InputManager menu (see Options tab)
            //Key Buttons: The buttons in the second and third column of the Input Manager menu
            //========================================================================================

            //Source: https://github.com/Gris87/InputControl
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
            //==============================================

            contentHeight += 28;
        }

        RectTransform namesRectTransform = namesTransform.GetComponent<RectTransform>();
        RectTransform keysRectTransform = keysTransform.GetComponent<RectTransform>();
        RectTransform rectTransform = GetComponent<RectTransform>();

        namesRectTransform.offsetMax = new Vector2(maxNameWidth, 0);
        keysRectTransform.offsetMin = new Vector2(maxNameWidth, 0);
        rectTransform.sizeDelta = new Vector2(0, contentHeight);

        GameObject.Find("SettingsMode").GetComponent<SettingsMode>().UpdateButtonStyle();
    }
    #endregion

    /// <summary>
    /// Destroys control lists.
    /// Reccommended: Call before generating/creating a new player control list.
    /// </summary>
    public void DestroyList()
    {
        namesTransform = transform.Find("Names");
        keysTransform = transform.Find("Keys");

        //Loops through each string name for controls and destroys the object(s)
        foreach (Transform child in namesTransform)
        {
            Destroy(child.gameObject);
        }

        //Loops through each control input button and destroys the object(s)
        foreach (Transform child in keysTransform)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Resets controls to tank drive defaults for the active player and updates 
    /// corresponding control labels/buttons.
    /// </summary>
    public void ResetTankDrive()
    {
        InputControl.mPlayerList[InputControl.activePlayerIndex].ResetTank();
        UpdateActiveButtons();
    }

    /// <summary>
    /// Resets controls to arcade drive defaults for the active player and updates 
    /// corresponding control labels/buttons.
    /// </summary>
    public void ResetArcadeDrive()
    {
        InputControl.mPlayerList[InputControl.activePlayerIndex].ResetArcade();
        UpdateActiveButtons();
    }

    /// <summary>
    /// Toggles the tankDriveSwitch/slider between arcade/tank drive for each player.
    /// </summary>
    public void TankSlider()
    {
        int i = (int)tankDriveSwitch.GetComponent<Slider>().value;

        switch (i)
        {
            case 0:  //tank drive slider is OFF
                InputControl.mPlayerList[InputControl.activePlayerIndex].SetArcadeDrive();
                UpdateActiveButtons();
                Controls.TankDriveEnabled = false;
                break;
            case 1:  //tank drive slider is ON
                InputControl.mPlayerList[InputControl.activePlayerIndex].SetTankDrive();
                UpdateActiveButtons();
                Controls.TankDriveEnabled = true;
                break;
            default: //defaults to arcade drive
                InputControl.mPlayerList[InputControl.activePlayerIndex].SetArcadeDrive();
                UpdateActiveButtons();
                Controls.TankDriveEnabled = false;
                break;
        }
    }

    /// <summary>
    /// Sets the player preference for measurement units
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
    /// Updates the toggles/sliders when changing scenes.
    /// </summary>
    public void OnEnable()
    {
        //Tank drive slider
        tankDriveSwitch = AuxFunctions.FindObject("TankDriveSwitch");
        tankDriveSwitch.GetComponent<Slider>().value = InputControl.mPlayerList[InputControl.activePlayerIndex].isTankDrive ? 1 : 0;

        //Measurement slider
        unitConversionSwitch = AuxFunctions.FindObject("UnitConversionSwitch");
        unitConversionSwitch.GetComponent<Slider>().value = PlayerPrefs.GetString("Measure").Equals("Metric") ? 1 : 0;
    }

    /// <summary>
    /// Updates the tank slider. Called on the active player to check for each player's individual preferances.
    /// </summary>
    public void UpdateTankSlider()
    {
        //tankDriveSwitch = AuxFunctions.FindObject("TankDriveSwitch");
        tankDriveSwitch.GetComponent<Slider>().value = InputControl.mPlayerList[InputControl.activePlayerIndex].isTankDrive ? 1 : 0;
    }
}