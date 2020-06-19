using System;
using McMaster.Extensions.CommandLineUtils;
using splitNspSharpLib;

namespace splitNspSharp
{
    internal static class SplitNspSharp
    {

        private static void SplitNsp(string filePath, string outputDir, bool inPlace)
        {
            var library = new SplitNspSharpLib();
            
            outputDir = library.ValidateOutputDirectory(filePath, outputDir);
            var splitNum = library.GetNspSplitCount(filePath, outputDir, inPlace);

            if (splitNum > 0)
            {
                library.Split(filePath, outputDir, inPlace, splitNum);
            }
        }

        public static int Main(string[] args)
        {
            Console.WriteLine("\n========== NSP Splitter ==========\n");
            
            var app = new CommandLineApplication();
            app.HelpOption();

            var argFilePath = app.Argument
            (
                "filepath", 
                "Path to NSP file."
            ).IsRequired().Accepts(v => v.ExistingFile());

            var optOutputDir = app.Option
            (
                "-o|--output-dir <DIR>", 
                "Set alternative output directory.", 
                CommandOptionType.SingleValue
            ).Accepts(v => v.ExistingDirectory());

            var optQuick = app.Option
            (
                "-q|--quick", 
                "Splits file in-place without creating a copy. Only requires 4GiB free space to run.", 
                CommandOptionType.NoValue
            );
            
            app.OnExecute
            (
                () =>
                {
                    string filePath = argFilePath.Value ?? "";
                    var outputDir = optOutputDir.Value();
                    var quick = optQuick.HasValue();
                    
                    SplitNsp(filePath, outputDir, quick);
                }
            );

            try
            {
                return app.Execute(args);
            }
            catch (UnrecognizedCommandParsingException)
            {
                app.ShowHelp();
                return -1;
            }
            catch (Exception)
            {
                Console.WriteLine("An error occurred! NSP splitting may have been unsuccessful.");
                return -1;
            }
        }
    }
}
