using UnityEngine;

public class Popup
{
    static int popupListHash = "PopupList".GetHashCode();
    static bool popupActive = false;

    public static bool List(Rect position, ref bool showList, ref int listEntry, GUIContent buttonContent, GUIContent[] listContent,
                             GUIStyle listStyle)
    {
        return List(position, ref showList, ref listEntry, buttonContent, listContent, "button", "box", listStyle);
    }

    public static bool List(Rect position, ref bool showList, ref int listEntry, GUIContent buttonContent, GUIContent[] listContent,
                             GUIStyle buttonStyle, GUIStyle boxStyle, GUIStyle listStyle)
    {
        int controlID = GUIUtility.GetControlID(popupListHash, FocusType.Passive);
        bool done = false;
        if (Event.current.GetTypeForControl(controlID) == EventType.mouseDown)
        {
            if (!showList && position.Contains(Event.current.mousePosition) && !popupActive)
            {
                GUIUtility.hotControl = controlID;
                showList = true;
                popupActive = true;
            }
            else if (showList)
            {
                done = true;
                popupActive = false;
            }


        }

        if (showList)
        {
            Rect listRect = new Rect(position.x, position.y, position.width, listStyle.CalcHeight(listContent[0], 1.0f) * listContent.Length);
            GUI.Box(listRect, "", boxStyle);
            listEntry = GUI.SelectionGrid(listRect, listEntry, listContent, 1, listStyle);
        }
        else GUI.Label(position, buttonContent, buttonStyle);
        if (done)
        {
            showList = false;
        }
        return done;
    }
}