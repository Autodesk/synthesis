using UnityEngine;
using UnityEditor;

public class OpenFilePanelExample : EditorWindow
{
    [MenuItem("Example/Overwrite Texture")]
    static void Apply()
    {
        Texture2D texture = Selection.activeObject as Texture2D;
        if (texture == null)
        {
            EditorUtility.DisplayDialog("Select Texture", "You must select a texture first!", "OK");
            return;
        }

        string path = EditorUtility.OpenFilePanel("Overwrite with png", "", "png");
        if (path.Length != 0)
        {
            WWW www = new WWW("file:///" + path);
            www.LoadImageIntoTexture(texture);
        }
    }
}