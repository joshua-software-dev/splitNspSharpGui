using System;
using Eto.Forms;

namespace splitNspSharpGui.Gtk
{
    internal static class MainClass
    {
        private static void ResizableFixer(ResizeableForm sender, bool value)
        // Reflection work around for Resizable = false not doing anything for GTK
        {
            var refControl = sender.Handler.GetType().GetProperty("Control").GetValue(sender.Handler, null);
            var resizeProp = refControl.GetType().GetProperty("Resizable");
            resizeProp.SetValue(refControl, value, null);
        }
        
        [STAThread]
        public static void Main(string[] args)
        {
            ResizeableForm.IsResizableChangeEvent += ResizableFixer;
            new Application(Eto.Platforms.Gtk).Run(new MainForm());
        }
    }
}
