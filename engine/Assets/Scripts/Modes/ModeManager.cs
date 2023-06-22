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
        }
    }

    public static void Start()
    {
        // should this be here? or somewhere else, or not at all?
        if (CurrentMode == null)
            CurrentMode = new PracticeMode();
        CurrentMode.Start();
        // Shooting.Start();
    }
    
    public static void Update()
    {
        CurrentMode.Update();
        // Shooting.Update();
    }

    public static void ModalClosed()
    {
        // used to tell practice mode that the modal has closed due to a button
        // so that the user doesn't have to press escape twice to open it again
        CurrentMode.CloseMenu();
    }
}