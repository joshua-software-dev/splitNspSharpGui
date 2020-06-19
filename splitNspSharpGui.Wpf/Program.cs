using System;
using Eto.Forms;

namespace splitNspSharpGui.Wpf
{
    class MainClass
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var app = new Application(Eto.Platforms.Wpf);
            var formInit = new FormInitializer(app);
            app.Run(formInit.mainForm);
        }
    }
}