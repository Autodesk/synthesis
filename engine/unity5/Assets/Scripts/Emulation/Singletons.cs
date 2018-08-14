using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

public static class OutputManager
{

    static OutputManager() { }
    public static EmuData Instance { get { return OutputInternal.instance; } set { OutputInternal.instance = value; } }

    private class OutputInternal
    {
        static OutputInternal()
        {

        }
        internal static EmuData instance = new EmuData();
    }
}

public static class InputManager
{
    public static EngineData Instance { get { return InputInternal.instance; } set { InputInternal.instance = value; } }

    private class InputInternal
    {
        static InputInternal()
        {

        }
        internal static EngineData instance = new EngineData();
    }
}
