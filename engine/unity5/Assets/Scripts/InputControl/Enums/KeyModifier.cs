/// <summary>
/// Key modifier. Enumeration for key modifiers.
/// Source: https://github.com/Gris87/InputControl
/// </summary>
public enum KeyModifier
{
      NoModifier   = 0x00
    , Ctrl         = 0x01
    , Alt          = 0x02
    , Shift        = 0x04
    , CtrlAlt      = Ctrl | Alt
    , CtrlShift    = Ctrl | Shift
    , AltShift     = Alt  | Shift
    , CtrlAltShift = Ctrl | Alt | Shift
}
