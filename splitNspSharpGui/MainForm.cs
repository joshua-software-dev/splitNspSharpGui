using System;
using System.Threading.Tasks;
using Eto.Forms;
using Eto.Drawing;
using splitNspSharpLib;

namespace splitNspSharpGui
{
    public class ResizeableForm : Form
    {
        public event EventHandler<(Form, bool)> FixResizeable; 
        
        public void SetResizeableState(bool value)
        {
            this.Resizable = value;
            FixResizeable?.Invoke(this, (this, value));
        }
    }
    
    public class MainForm : ResizeableForm
    {
        private readonly Application _app;
        private readonly ProgressForm _progressForm;
        private SplitNspSharpLib _library;

        private string _inputFile;
        private string _outputFolder;
        private bool _inPlace;

        public MainForm(Application app, ProgressForm progressForm)
        {
            Title = "splitNspSharpGui " + (System.Reflection.Assembly.GetEntryAssembly()?.GetName()?.Version?.ToString() ?? "");
            ClientSize = new Size(400, 170);
            MinimumSize = ClientSize;
            Resizable = false;
            Maximizable = false; 

            _app = app;
            _progressForm = progressForm;
            
            InitControls();
        }
        
        private async void ProgressAsync(object sender, EventArgs e)
        {
            Content.Enabled = false;

            _outputFolder = _library.ValidateOutputDirectory(_inputFile, _outputFolder);
            var splitNum = _library.GetNspSplitCount(_inputFile, _outputFolder, _inPlace);

            if (splitNum > 0)
            {
                _progressForm.SetProgressMinMax(0, splitNum + 1);
                await Task.Run(() => _library.Split(_inputFile, _outputFolder, _inPlace, splitNum));
            }

            Content.Enabled = true;
        }

        private void InitControls()
        {
            var fileLabel = new Label() { Text = "Input File: Choose an input file.", Wrap = WrapMode.None };
            var openFileButton = new Button() { Height = 40, Text = "Open NSP File" };
            var openFileDialog = new OpenFileDialog();
            var folderLabel = new Label() { Text = "Output Folder: Same as input file.", Wrap = WrapMode.None };
            var openFolderButton = new Button() { Height = 40, Width = 100, Text = "Choose output folder." };
            var openFolderDialog = new SelectFolderDialog();
            var checkBox = new CheckBox() { Height = 50, Text = "Enable splitting of input file\ninstead of creating a split copy." };
            var startSplittingButton = new Button { Enabled = false, Height = 10, Text = "Split NSP" };
            
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
                fileLabel.Text = "Input File: " + (_inputFile ?? "");
            };
            
            openFolderButton.Click += (sender, e) =>
            {
                openFolderDialog.ShowDialog(openFolderButton);
                _outputFolder = openFolderDialog.Directory;
                folderLabel.Text = "Output Folder: " + (_outputFolder ?? "Same as input file");
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
            
            InitEventHooks();
        }

        private void InitEventHooks()
        {
            _library = new SplitNspSharpLib(false);

            _library.InputFileTooSmall += (sender, args) => _app.Invoke(
                () => MessageBox.Show
                (
                    "Input NSP is under 4GiB and does not need to be split.", 
                    MessageBoxType.Error
                )
            );

            _library.NeedFreeSpace += (sender, args) => _app.Invoke(
                () => MessageBox.Show
                (
                    $"Not enough free space to run! At least " + args + " bytes are needed.", 
                    MessageBoxType.Error
                )
            );

            _library.Need4GbFreeSpace += (sender, args) => _app.Invoke(
                () => MessageBox.Show
                (
                    $"Not enough free space to run! At least " + args + " bytes are needed.", 
                    MessageBoxType.Error
                )
            );

            _library.BeginningWork += (sender, args) => _app.Invoke
            (
                async () =>
                {
                    _progressForm.Topmost = true;
                    _progressForm.Show();
                    await Task.Delay(100);
                    _progressForm.Topmost = false;
                }
            );

            _library.StartWritingFile += (sender, args) => _app.Invoke(() => _progressForm.IncrementProgressBar());
            _library.FinishWork += (sender, args) => _app.Invoke(() => _progressForm.Visible = false);
        }
    }
    
    public class FormInitializer
    {
        public readonly MainForm mainForm;
        public readonly ProgressForm progressForm;
        
        public FormInitializer(Application app)
        {
            progressForm = new ProgressForm();
            mainForm = new MainForm(app, progressForm);
        }

        public void FixResizable()
        {
            mainForm.SetResizeableState(mainForm.Resizable);
            progressForm.SetResizeableState(progressForm.Resizable);
        }
    }
}
