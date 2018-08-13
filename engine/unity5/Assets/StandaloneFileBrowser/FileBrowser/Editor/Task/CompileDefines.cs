using UnityEditor;

namespace Crosstales.FB.EditorTask
{
    /// <summary>Adds the given define symbols to PlayerSettings define symbols.</summary>
    [InitializeOnLoad]
    public class CompileDefines : Common.EditorTask.BaseCompileDefines
    {

        private static readonly string[] symbols = new string[] {
            "CT_FB",
        };

        static CompileDefines()
        {
            setCompileDefines(symbols);
        }
    }
}
// © 2017-2018 crosstales LLC (https://www.crosstales.com)