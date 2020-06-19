using System;
using Eto.Forms;

namespace splitNspSharpGui.Mac
{
    class MainClass
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var app = new Application(Eto.Platforms.Mac64);
            var formInit = new FormInitializer(app);
            app.Run(formInit.mainForm);
        }
    }
}