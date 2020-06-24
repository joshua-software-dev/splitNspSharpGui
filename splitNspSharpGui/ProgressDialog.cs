using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;

namespace splitNspSharpGui
{
    public class ProgressDialog : Dialog
    {
        private readonly ProgressBar _progressBar;
        private readonly Label _progressLabel;
        
        public ProgressDialog()
        {
            Title = "Progress";
            ClientSize = new Size(200, 60);
            
            _progressBar = new ProgressBar { Height = 15 };
            _progressLabel = new Label { Height = 40, TextAlignment = TextAlignment.Center };
            Content = new DynamicLayout(_progressLabel, _progressBar) { Padding = 3 };
            
            RegisterHooks();
        }

        private static void DisableUserDialogClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
        }

        private void SetProgressMinMax(int min, int max)
        {
            _progressBar.MinValue = min;
            _progressBar.MaxValue = max;
            _progressBar.Value = min;
        }

        private void TriggerShowDialog(object sender, int splitNum)
        {
            Application.Instance.Invoke(
                async () =>
                {
                    SetProgressMinMax(0, splitNum);
                    await ShowModalAsync();
                }
            );
        }

        private void TriggerUpdateProgressLabel(object sender, string currentWorkFileName)
        {
            Application.Instance.Invoke(
                () =>
                {
                    _progressLabel.Text = "Finished: " + _progressBar.Value + "/" + _progressBar.MaxValue + "\nWriting File: " + currentWorkFileName;
                }
            );
        }
        
        private void TriggerIncrementProgress(object sender, string currentWorkFileName)
        {
            Application.Instance.Invoke(
                () =>
                {
                    _progressBar.Value += 1;
                }
            );
        }

        private async void TriggerHideDialog(object sender, EventArgs e)
        {
            await Task.Delay(100);
            await Application.Instance.InvokeAsync(
                () =>
                {
                    Closing -= DisableUserDialogClosing;
                    Close();
                    Closing += DisableUserDialogClosing;
                }
            );
        }

        private void RegisterHooks()
        {
            Closing += DisableUserDialogClosing;
            RegisterSplitNspGuiEvents.Library.BeginningWork += TriggerShowDialog;
            RegisterSplitNspGuiEvents.Library.StartWritingFile += TriggerUpdateProgressLabel;
            RegisterSplitNspGuiEvents.Library.FinishWritingFile += TriggerIncrementProgress;
            RegisterSplitNspGuiEvents.Library.FinishWork += TriggerHideDialog;
        }
    }
}
