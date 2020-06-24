using Eto.Forms;

namespace splitNspSharpGui
{
    public class ResizeableForm : Form
    {
        public delegate void IsResizableChanged(ResizeableForm sender, bool value);
        public static event IsResizableChanged IsResizableChangeEvent;
        
        protected bool IsResizable
        {
            get => Resizable;
            set
            {
                Resizable = value;
                IsResizableChangeEvent?.Invoke(this, value);
            }
        }
    }
}
