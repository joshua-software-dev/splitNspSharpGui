using Eto.Forms;
using splitNspSharpLib;

namespace splitNspSharpGui
{
    public static class RegisterSplitNspGuiEvents
    {
        public static readonly SplitNspSharpLib Library = new SplitNspSharpLib(false);

        static RegisterSplitNspGuiEvents()
        {
            Library.InputFileTooSmall += (sender, args) =>
                Application.Instance.Invoke(
                    () => MessageBox.Show
                    (
                        "Input NSP is under 4GiB and does not need to be split.", 
                        MessageBoxType.Error
                    )
                );

            Library.NeedFreeSpace += (sender, args) =>
                Application.Instance.Invoke(
                    () => MessageBox.Show
                    (
                        "Not enough free space to run! At least " + args + " bytes are needed.", 
                        MessageBoxType.Error
                    )
                );

            Library.Need4GbFreeSpace += (sender, args) =>
                Application.Instance.Invoke(
                    () => MessageBox.Show
                    (
                        "Not enough free space to run! At least " + args + " bytes are needed.", 
                        MessageBoxType.Error
                    )
                );
        }
    }
}
