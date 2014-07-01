using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inventor;

class ComponentHighlighter
{
    public static HighlightSet parentHS = null;
    public static HighlightSet childHS = null;

    public static void prepareHighlight()
    {
        if (childHS == null)
        {
            childHS = Program.invApplication.ActiveDocument.CreateHighlightSet();
            childHS.Color = Program.invApplication.TransientObjects.CreateColor(0, 0, 255);
        }
        if (parentHS == null)
        {
            parentHS = Program.invApplication.ActiveDocument.CreateHighlightSet();
            parentHS.Color = Program.invApplication.TransientObjects.CreateColor(255, 0, 0);
        }
    }

    public static void clearHighlight()
    {
        if (childHS != null) { childHS.Clear(); }
        if (parentHS != null) { parentHS.Clear(); }
    }

    public static void cleanupHS()
    {
        if (!(childHS == null))
            childHS.Delete();
        if (!(parentHS == null))
            parentHS.Delete();
    }
}
