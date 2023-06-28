using Synthesis.UI.Dynamic;
using UnityEngine;

public class ModeManager {
    
    private static IMode _currentMode;
    public static IMode CurrentMode
    {
        get => _currentMode;
        set
        {
            if (_currentMode != null)
                _currentMode.End();
            _currentMode = value;
            _currentMode.Start();
        }
    }

    public static void Start()
    {
        if (CurrentMode == null)
        {
            DynamicUIManager.CreateModal<ChooseModeModal>();
        }
    }
    
    public static void Update()
    {
        if (CurrentMode != null)
            CurrentMode.Update();
    }

    public static void ModalClosed()
    {
        if (CurrentMode != null) 
            CurrentMode.CloseMenu();
    }
}