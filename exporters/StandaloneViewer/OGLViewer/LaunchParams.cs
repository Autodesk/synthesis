using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class LaunchParams
{
    public static string Path { get; private set; }
    public static Size WindowSize { get; private set; }

    public static void Load(string[] args)
    {
        int width = -1, height = -1;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].ToLower() == "-w")
                width = int.Parse(args[i + 1]);
            else if (args[i].ToLower() == "-h")
                height = int.Parse(args[i + 1]);
            else if (args[i].ToLower() == "-path")
                Path = args[i + 1];
        }
        WindowSize = new Size((width != -1) ? width : 800, (height != -1) ? height : 600);
        if (Path == null)
        {
            Path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Documents\Synthesis\Robots\Dozer";
        }
    }
    public static void Defaults()
    {
        Path = null;
        WindowSize = new Size(800, 600);
    }
}