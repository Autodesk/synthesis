using System;
using EditorsLibrary;

static class Program
{

    [STAThread]
    public static void Main(String[] args)
    {
        SynthesisGUI GUI = new SynthesisGUI();
        GUI.ShowDialog();
    }

}