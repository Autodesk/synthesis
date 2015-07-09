using Inventor;

class ComponentHighlighter
{
    public static HighlightSet PARENT_HIGHLIGHT_SET = null;
    public static HighlightSet CHILD_HIGHLIGHT_SET = null;

    public static void PrepareHighlight()
    {
        if (CHILD_HIGHLIGHT_SET == null)
        {
            CHILD_HIGHLIGHT_SET = Exporter.INVENTOR_APPLICATION.ActiveDocument.CreateHighlightSet();
            CHILD_HIGHLIGHT_SET.Color = Exporter.INVENTOR_APPLICATION.TransientObjects.CreateColor(0, 0, 255);
        }
        if (PARENT_HIGHLIGHT_SET == null)
        {
            PARENT_HIGHLIGHT_SET = Exporter.INVENTOR_APPLICATION.ActiveDocument.CreateHighlightSet();
            PARENT_HIGHLIGHT_SET.Color = Exporter.INVENTOR_APPLICATION.TransientObjects.CreateColor(255, 0, 0);
        }
    }

    public static void ClearHighlight()
    {
        if (CHILD_HIGHLIGHT_SET != null)
        {
            CHILD_HIGHLIGHT_SET.Clear();
        }
        if (PARENT_HIGHLIGHT_SET != null)
        {
            PARENT_HIGHLIGHT_SET.Clear();
        }
    }

    public static void CleanupHighlighter()
    {
        if (!(CHILD_HIGHLIGHT_SET == null))
            CHILD_HIGHLIGHT_SET.Delete();
        if (!(PARENT_HIGHLIGHT_SET == null))
            PARENT_HIGHLIGHT_SET.Delete();
    }
}
