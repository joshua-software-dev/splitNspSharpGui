using System;
using System.IO;
using System.Text.RegularExpressions;

namespace splitNspSharpLib
{
    public class SplitNspSharpLib
    {
        private const long SplitSize = 0xFFFF0000;  // 4,294,901,760 bytes
        private const int ChunkSize = 0x1000;  // 4,096 bytes

        public event EventHandler InputFileTooSmall;
        public event EventHandler<long> NeedFreeSpace;
        public event EventHandler<long> Need4GbFreeSpace;
        public event EventHandler<int> BeginningWork;
        public event EventHandler<string> StartWritingFile;
        public event EventHandler<string> FinishWritingFile;
        public event EventHandler FinishWork;

        public SplitNspSharpLib(bool registerConsoleEventHandlers = true)
        {
            if (!registerConsoleEventHandlers) return;
            InputFileTooSmall += (sender, args) => Console.WriteLine("Input NSP is under 4GiB and does not need to be split.");
            NeedFreeSpace += (sender, args) => Console.WriteLine("Not enough free space to run! At least " + args + " bytes are needed.");
            Need4GbFreeSpace += (sender, args) => Console.WriteLine("Not enough free space to run! At least " + args + " bytes are needed.");
            BeginningWork += (sender, args) => Console.WriteLine("Splitting NSP into " + args + " parts...\n");
            StartWritingFile += (sender, args) => Console.WriteLine("Writing file: " + args);
            FinishWritingFile += (sender, args) => Console.WriteLine("Finished writing file: " + args);
            FinishWork += (sender, args) => Console.WriteLine("\nNSP successfully split!\n");
        }
        
        private static long GetFileSize(string file)
        {
            return new FileInfo(file).Length;
        }

        private static void CreateDirectoryIfNotExist(string outputDir)
        // Directory.CreateDirectory will not error if a directory of the same
        // name already exists, but will error if a file of the same name
        // exists, which will prevent the folder from being created.
        {
            try
            {
                if (!File.GetAttributes(outputDir).HasFlag(FileAttributes.Directory))
                {
                    File.Delete(outputDir);
                }
            }
            catch (FileNotFoundException) {}

            Directory.CreateDirectory(outputDir);

            File.SetAttributes(outputDir, FileAttributes.Archive);
        }

        private static long GetFreeSpace(string location)
        {
            return new DriveInfo(location).AvailableFreeSpace;
        }

        private bool CheckFreeSpace(bool inPlace, long inputFileSize, string outputDir)
        {
            if (inPlace)
            {
                if (GetFreeSpace(outputDir) >= SplitSize) return true;
                
                Need4GbFreeSpace?.Invoke(this, SplitSize);

                return false;
            }
            else
            {
                if (GetFreeSpace(outputDir) >= (inputFileSize * 2)) return true;
                
                NeedFreeSpace?.Invoke(this, inputFileSize);

                return false;
            }
        }

        private void InPlaceSplit(string inputFile, string outputDir, int splitNum)
        {
            var splitFileLocation = Path.Combine(outputDir, "00");
            File.Move(inputFile, splitFileLocation);

            using (Stream input = File.Open(splitFileLocation, FileMode.Open))
            {
                for (var i = splitNum; i >= 0; i--)
                {
                    StartWritingFile?.Invoke(this, $"{i:D2}");
                    if (i != 0)
                    {
                        using (Stream output = File.Create(Path.Combine(outputDir, $"{i:D2}")))
                        {
                            input.Seek(i * SplitSize, SeekOrigin.Begin);
                            input.CopyTo(output);
                            input.SetLength(i * SplitSize);
                        }
                    }
                    FinishWritingFile?.Invoke(this, $"{i:D2}");
                }
            }
        }

        private void CopySplit(string inputFile, string outputDir, int splitNum)
        {
            var byteBuffer = new byte[ChunkSize];

            using (Stream input = File.OpenRead(inputFile))
            {
                for (var i = 0; i < splitNum; i++)
                {
                    StartWritingFile?.Invoke(this, $"{i:D2}");
                    long currentPosition = 0;

                    using (Stream output = File.Create(Path.Combine(outputDir, $"{i:D2}")))
                    {
                        while (currentPosition < SplitSize && input.Position != input.Length)
                        {
                            currentPosition += input.Read(byteBuffer, 0, ChunkSize);
                            output.Write(byteBuffer, 0, ChunkSize);
                        }

                        FinishWritingFile?.Invoke(this, $"{i:D2}");
                    }
                }

                if (input.Position >= input.Length) return;
                
                StartWritingFile?.Invoke(this, $"{splitNum:D2}");
                using (Stream output = File.Create(Path.Combine(outputDir, $"{splitNum:D2}")))
                {
                    input.CopyTo(output);
                }
                FinishWritingFile?.Invoke(this, $"{splitNum:D2}");
            }
        }

        public string ValidateOutputDirectory(string inputFile, string outputDir)
        {
            if (string.IsNullOrEmpty(outputDir))
            {
                outputDir = Regex.Replace(inputFile, "\\.nsp$", "_split.nsp");

                if (!outputDir.EndsWith(".nsp"))
                {
                    outputDir += ".nsp";
                }
            }

            return outputDir;
        }

        public int GetNspSplitCount(string inputFile, string outputDir, bool inPlace)
        {
            var inputFileSize = GetFileSize(inputFile);

            if (inputFileSize < SplitSize)
            {
                InputFileTooSmall?.Invoke(this, EventArgs.Empty);

                return -1;
            }

            CreateDirectoryIfNotExist(outputDir);

            if (!CheckFreeSpace(inPlace, inputFileSize, outputDir))
            {
                return -2;
            }

            return (int)(inputFileSize / SplitSize);
        }
        public void Split(string inputFile, string outputDir, bool inPlace, int splitNum)
        {
            BeginningWork?.Invoke(this, splitNum + 1);

            if (inPlace)
            {
                InPlaceSplit(inputFile, outputDir, splitNum);
            }
            else
            {
                CopySplit(inputFile, outputDir, splitNum);
            }
            
            FinishWork?.Invoke(this, EventArgs.Empty);
        }
    }
}
