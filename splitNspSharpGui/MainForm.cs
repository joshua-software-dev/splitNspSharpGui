using Eto.Forms;
using Eto.Drawing;
using System;
using System.Threading.Tasks;

namespace splitNspSharpGui
{
    public class MainForm : ResizeableForm
    {
        private readonly ProgressDialog _progressDialog;

        private string _inputFile;
        private string _outputFolder;
        private bool _inPlace;

        private const string MainWindowTitle = "splitNspSharpGui";
        private const string DefaultInputFileLabel = "Input File: ";
        private const string MissingFileNameLabel = "Choose an input file";
        private const string OpenFileButtonLabel = "Open NSP File";
        private const string DefaultInputFolderLabel = "Output Folder: ";
        private const string MissingFolderNameLabel = "Same as input file";
        private const string OpenFolderButtonLabel = "Choose Output Folder";
        private const string EnableInPlaceSplittingDesc = "Enable splitting of input file\ninstead of creating a split copy.";
        private const string StartWorkButtonLabel = "Split NSP";

        public MainForm()
        {
            Title = MainWindowTitle + " " + Version.GetVersion();
            ClientSize = new Size(400, 170);
            IsResizable = false;
            Maximizable = false; 
            
            _progressDialog = new ProgressDialog();

            InitContent();
            InitEventHooks();
        }

        private async void ProgressAsync(object sender, EventArgs e)
        {
            _outputFolder = RegisterSplitNspGuiEvents.Library.ValidateOutputDirectory(_inputFile, _outputFolder);
            var splitNum = RegisterSplitNspGuiEvents.Library.GetNspSplitCount(_inputFile, _outputFolder, _inPlace);
            
            if (splitNum > 0)
            {
                await Task.Run(
                    () => RegisterSplitNspGuiEvents.Library.Split(
                        _inputFile,
                        _outputFolder,
                        _inPlace,
                        splitNum
                    )
                );
            }
        }

        private void InitContent()
        {
            var fileLabel = new Label { Text = DefaultInputFileLabel + MissingFileNameLabel, Width = Size.Width, Wrap = WrapMode.None };
            var openFileButton = new Button { Height = 40, Text = OpenFileButtonLabel };
            var openFileDialog = new OpenFileDialog();
            var folderLabel = new Label { Text = DefaultInputFolderLabel + MissingFolderNameLabel, Width = Size.Width, Wrap = WrapMode.None };
            var openFolderButton = new Button { Height = 40, Width = 100, Text = OpenFolderButtonLabel };
            var openFolderDialog = new SelectFolderDialog();
            var checkBox = new CheckBox { Height = 50, Text = EnableInPlaceSplittingDesc };
            var startSplittingButton = new Button { Enabled = false, Height = 10, Text = StartWorkButtonLabel };
            
            openFileButton.Click += (sender, e) =>
            {
                try
                {
                    openFileDialog.ShowDialog(openFileButton);
                    _inputFile = openFileDialog.FileName;
                }
                catch (NullReferenceException)
                {
                    _inputFile = null;
                }

                startSplittingButton.Enabled = _inputFile != null;
                fileLabel.Text = DefaultInputFileLabel + (_inputFile ?? MissingFileNameLabel);
            };

            openFolderButton.Click += (sender, e) =>
            {
                try
                {
                    openFolderDialog.ShowDialog(openFolderButton);
                    _outputFolder = openFolderDialog.Directory;
                }
                catch (NullReferenceException)
                {
                    _outputFolder = null;
                }
            
                folderLabel.Text = DefaultInputFolderLabel + (_outputFolder ?? MissingFolderNameLabel);
            };

            checkBox.CheckedChanged += (sender, e) =>
            {
                _inPlace = checkBox.Checked ?? false;
            };
            
            startSplittingButton.Click += ProgressAsync;
            
            var layout = new DynamicLayout
            (
                new TableLayout
                ( 
                    fileLabel, 
                    openFileButton, 
                    folderLabel, 
                    openFolderButton
                )
                {
                    Padding = 3
                }
            );
            
            layout.AddSeparateRow
            (
                new TableLayout
                (
                    new TableRow(checkBox, startSplittingButton)
                )
                {
                    Padding = 3
                }
            );

            Content = layout;
        }
        
        private static void KillProgramOnMainWindowExit(object sender, EventArgs e)
        {
            Application.Instance.Quit();
        }

        private void InitEventHooks()
        {
            Closed += KillProgramOnMainWindowExit;
            Application.Instance.Terminating += (s, e) => Closed -= KillProgramOnMainWindowExit;
        }
    }
}
