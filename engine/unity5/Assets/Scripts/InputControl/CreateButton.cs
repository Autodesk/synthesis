using UnityEngine;
using UnityEngine.UI;
using System.Collections.ObjectModel;
using System.Collections.Generic;

public class CreateButton : MonoBehaviour
{
    public GameObject keyNamePrefab;
    public GameObject keyButtonsPrefab;
    public List<GameObject> keyButtonList;
    private Transform namesTransform;
    private Transform keysTransform;

    // Use this for initialization
    void Start()
    {
        namesTransform = transform.Find("Names");
        keysTransform = transform.Find("Keys");

        float maxNameWidth = 0;
        float contentHeight = 4;

        ReadOnlyCollection<KeyMapping> keys = InputControl.getPlayerOneKeys();

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

    // Main List: The main list is the list that contains all of the keys in both tank drive and arcade drive.
    // Each player has its own list of set keys, which is then fed into the main list when setKey() is called.
    // Purpose: This allows us to call each player's key lists individually in addition to the main list.

    #region Update Main List
    public void UpdateMainButtons()
    {
        DestroyList();

        float maxNameWidth = 0;
        float contentHeight = 4;

        //Reads the main keys list: getKeysList()
        ReadOnlyCollection<KeyMapping> keys = InputControl.getKeysList();

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
    public void UpdatePlayerOne()
    {
        //Destroys all previous keys
        DestroyList();

        float maxNameWidth = 0;
        float contentHeight = 4;

        //Calls the getPlayerOneKeys list to generate ONLY Player One's keys
        ReadOnlyCollection<KeyMapping> playerOneKeys = InputControl.getPlayerOneKeys();

        foreach (KeyMapping key in playerOneKeys)
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
    public void UpdatePlayerTwo()
    {
        DestroyList();

        float maxNameWidth = 0;
        float contentHeight = 4;

        //Calls the getPlayerTwoKeys list to generate ONLY Player Two's keys
        ReadOnlyCollection<KeyMapping> playerTwoKeys = InputControl.getPlayerTwoKeys();

        foreach (KeyMapping key in playerTwoKeys)
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
    public void UpdatePlayerThree()
    {
        DestroyList();

        float maxNameWidth = 0;
        float contentHeight = 4;

        //Calls the getPlayerThreeKeys list to generate ONLY Player Three's keys
        ReadOnlyCollection<KeyMapping> playerThreeKeys = InputControl.getPlayerThreeKeys();

        foreach (KeyMapping key in playerThreeKeys)
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
    public void UpdatePlayerFour()
    {
        DestroyList();

        float maxNameWidth = 0;
        float contentHeight = 4;

        //Calls the getPlayerFourKeys list to generate ONLY Player Four's keys
        ReadOnlyCollection<KeyMapping> playerFourKeys = InputControl.getPlayerFourKeys();

        foreach (KeyMapping key in playerFourKeys)
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
    public void UpdatePlayerFive()
    {
        DestroyList();

        float maxNameWidth = 0;
        float contentHeight = 4;

        //Calls the getPlayerFiveKeys list to generate ONLY Player Five's keys
        ReadOnlyCollection<KeyMapping> playerFiveKeys = InputControl.getPlayerFiveKeys();

        foreach (KeyMapping key in playerFiveKeys)
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
    public void UpdatePlayerSix()
    {
        DestroyList();

        float maxNameWidth = 0;
        float contentHeight = 4;

        //Calls the getPlayerSixKeys list to generate ONLY Player Six's keys
        ReadOnlyCollection<KeyMapping> playerSixKeys = InputControl.getPlayerSixKeys();

        foreach (KeyMapping key in playerSixKeys)
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

    //=============================================================================
    //                          Key Transitions: Tank Drive
    //=============================================================================
    #region Update Tank Drive Key Buttons
    public void UpdateTankDriveList()
    {
        DestroyList();

        float maxNameWidth = 0;
        float contentHeight = 4;

        ReadOnlyCollection<KeyMapping> keys = InputControl.getTankDriveKeys();

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

    #region Update Tank Drive Player One Key Buttons
    public void UpdateTankDriveP1()
    {
        DestroyList();

        float maxNameWidth = 0;
        float contentHeight = 4;

        ReadOnlyCollection<KeyMapping> keys = InputControl.getTankDriveP1Keys();

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

    #region Update Tank Drive Player Two Key Buttons
    public void UpdateTankDriveP2()
    {
        DestroyList();

        float maxNameWidth = 0;
        float contentHeight = 4;

        ReadOnlyCollection<KeyMapping> keys = InputControl.getTankDriveP2Keys();

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

    #region Update Tank Drive Player Three Key Buttons
    public void UpdateTankDriveP3()
    {
        DestroyList();

        float maxNameWidth = 0;
        float contentHeight = 4;

        ReadOnlyCollection<KeyMapping> keys = InputControl.getTankDriveP3Keys();

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

    #region Update Tank Drive Player Four Key Buttons
    public void UpdateTankDriveP4()
    {
        DestroyList();

        float maxNameWidth = 0;
        float contentHeight = 4;

        ReadOnlyCollection<KeyMapping> keys = InputControl.getTankDriveP4Keys();

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

    #region Update Tank Drive Player Five Key Buttons
    public void UpdateTankDriveP5()
    {
        DestroyList();

        float maxNameWidth = 0;
        float contentHeight = 4;

        ReadOnlyCollection<KeyMapping> keys = InputControl.getTankDriveP5Keys();

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

    #region Update Tank Drive Player Six Key Buttons
    public void UpdateTankDriveP6()
    {
        DestroyList();

        float maxNameWidth = 0;
        float contentHeight = 4;

        ReadOnlyCollection<KeyMapping> keys = InputControl.getTankDriveP6Keys();

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


    //==================================Arcade Drive==========================================
    #region Update Arcade Drive Key Buttons
    public void UpdateArcadeDriveList()
    {
        DestroyList();

        float maxNameWidth = 0;
        float contentHeight = 4;

        ReadOnlyCollection<KeyMapping> keys = InputControl.getArcadeDriveKeys();

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

    #region Update Arcade Drive Player One Key Buttons
    public void UpdateArcadeDriveP1()
    {
        DestroyList();

        float maxNameWidth = 0;
        float contentHeight = 4;

        ReadOnlyCollection<KeyMapping> keys = InputControl.getArcadeDriveP1Keys();

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

    #region Update Arcade Drive Player Two Key Buttons
    public void UpdateArcadeDriveP2()
    {
        DestroyList();

        float maxNameWidth = 0;
        float contentHeight = 4;

        ReadOnlyCollection<KeyMapping> keys = InputControl.getArcadeDriveP2Keys();

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

    #region Update Arcade Drive Player Three Key Buttons
    public void UpdateArcadeDriveP3()
    {
        DestroyList();

        float maxNameWidth = 0;
        float contentHeight = 4;

        ReadOnlyCollection<KeyMapping> keys = InputControl.getArcadeDriveP3Keys();

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

    #region Update Arcade Drive Player Four Key Buttons
    public void UpdateArcadeDriveP4()
    {
        DestroyList();

        float maxNameWidth = 0;
        float contentHeight = 4;

        //Reads the main keys list: getKeysList()
        ReadOnlyCollection<KeyMapping> keys = InputControl.getArcadeDriveP4Keys();

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

    #region Update Arcade Drive Player Five Key Buttons
    public void UpdateArcadeDriveP5()
    {
        DestroyList();

        float maxNameWidth = 0;
        float contentHeight = 4;

        //Reads the main keys list: getKeysList()
        ReadOnlyCollection<KeyMapping> keys = InputControl.getArcadeDriveP5Keys();

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

    #region Update Arcade Drive Player Six Key Buttons
    public void UpdateArcadeDriveP6()
    {
        DestroyList();

        float maxNameWidth = 0;
        float contentHeight = 4;

        ReadOnlyCollection<KeyMapping> keys = InputControl.getArcadeDriveP6Keys();

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
}