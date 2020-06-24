using System;
using Eto.Forms;

namespace splitNspSharpGui.Wpf
{
    internal static class MainClass
    {
        [STAThread]
        public static void Main(string[] args)
        {
            new Application(Eto.Platforms.Wpf).Run(new MainForm());
        }
    }
}