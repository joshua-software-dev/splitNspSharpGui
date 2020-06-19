using Eto.Forms;
using Eto.Drawing;

namespace splitNspSharpGui
{
    public partial class ProgressForm : ResizeableForm
    {
        private readonly ProgressBar _progressBar;
        private readonly Label _progressLabel;
        public ProgressForm()
        {
            Title = "Progress";
            ClientSize = new Size(200, 60);
            MinimumSize = ClientSize;
            Resizable = false;
            Minimizable = false;
            Maximizable = false;

            _progressLabel = new Label() {Height = 10, Text = "0/0", TextAlignment = TextAlignment.Center};
            _progressBar = new ProgressBar();

            Content = new DynamicLayout(_progressLabel, _progressBar) { Padding = 3 };

            Closing += (sender, e) => { e.Cancel = true; };
        }
        
        public void IncrementProgressBar()
        {
            _progressBar.Value += 1;
            _progressLabel.Text = _progressBar.Value + "/" + _progressBar.MaxValue;
        }

        public void SetProgressMinMax(int min, int max)
        {
            _progressBar.MinValue = min;
            _progressBar.MaxValue = max;
            _progressBar.Value = min;
        }
    }
}