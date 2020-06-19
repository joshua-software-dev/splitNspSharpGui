using System;
using Eto.Forms;

namespace splitNspSharpGui.Gtk
{
    class MainClass
    {
        private static void ResizableFixer(object sender, (Form, bool) values)
        // Reflection work around for Resizable = false not doing anything for GTK
        {
            var (form, value) = values;

            var refControl = form.Handler.GetType().GetProperty("Control").GetValue(form.Handler, null);
            var resizeProp = refControl.GetType().GetProperty("Resizable");
            resizeProp.SetValue(refControl, value, null);
        }
        
        [STAThread]
        public static void Main(string[] args)
        {
            var app = new Application(Eto.Platforms.Gtk);
            var formInit = new FormInitializer(app);
            formInit.mainForm.FixResizeable += ResizableFixer;
            formInit.progressForm.FixResizeable += ResizableFixer;
            formInit.FixResizable();
            app.Run(formInit.mainForm);
        }
    }
}
