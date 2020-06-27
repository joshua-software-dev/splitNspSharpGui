using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;
using System.Xml;
using Mono.Unix;
using static Bullseye.Targets;
using static SimpleExec.Command;

namespace _build
{
    internal static class Program
    {
        private static bool? _dockerHasDefaultContext;

        private static bool DockerHasDefaultContext
        {
            get
            {
                _dockerHasDefaultContext ??= CheckDockerContext("default");
                return _dockerHasDefaultContext.GetValueOrDefault();
            }
        }

        private static bool? _dockerHasWindowsContext;

        private static bool DockerHasWindowsContext
        {
            get
            {
                _dockerHasWindowsContext ??= CheckDockerContext("2019-box");
                return _dockerHasWindowsContext.GetValueOrDefault();
            }
        }

        private static bool CheckDockerVersion()
        {
            try
            {
                var dockerVersionChecker = ProcessAsyncHelper.RunAsync(
                    new ProcessStartInfo("docker", "version") { RedirectStandardError = true, RedirectStandardOutput = true}, 
                    3000
                ).GetAwaiter().GetResult();
                
                return (dockerVersionChecker.ExitCode ?? 1) == 0;
            }
            catch (System.ComponentModel.Win32Exception)
            {
                return false;
            }
        }

        private static bool CheckDockerContext(string context)
        {
            try
            {
                var dockerVersionChecker = ProcessAsyncHelper.RunAsync(
                    new ProcessStartInfo("docker", "context use " + context) { RedirectStandardError = true, RedirectStandardOutput = true}, 
                    3000
                ).GetAwaiter().GetResult();
                
                return (dockerVersionChecker.ExitCode ?? 1) == 0 && CheckDockerVersion();
            }
            catch (System.ComponentModel.Win32Exception)
            {
                return false;
            }
        }
        
        private static OSPlatform Platform
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return OSPlatform.Linux;
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return OSPlatform.OSX;
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return OSPlatform.Windows;
                }

                throw new ArgumentException("Unsupported platform for compiling.");
            }
        }
        
        private static string _guiConverterExecutable;
        
        private static string GuiConverterExecutable
        {
            get
            {
                _guiConverterExecutable ??= "gui_converter" + (Platform == OSPlatform.Windows ? ".exe" : "");
                return _guiConverterExecutable;
            }
        }
        
        private static string _warpPackerExecutable;
        
        private static string WarpPackerExecutable
        {
            get
            {
                _warpPackerExecutable ??= "warp-packer" + (Platform == OSPlatform.Windows ? ".exe" : "");
                return _warpPackerExecutable;
            }
        }

        private static string _cliVersion;
        
        private static string CliVersion
        {
            get
            {
                if (_cliVersion != null)
                {
                    return _cliVersion;
                }
                
                var xmlDoc = new XmlDocument();
                xmlDoc.Load("Version/CliVersion.csproj");
                _cliVersion = xmlDoc.SelectSingleNode("//Project/PropertyGroup/Version")?.InnerText;

                return _cliVersion;
            }
        }
        
        private static string _guiVersion;
        
        private static string GuiVersion
        {
            get
            {
                if (_guiVersion != null)
                {
                    return _guiVersion;
                }
                
                var xmlDoc = new XmlDocument();
                xmlDoc.Load("Version/GuiVersion.csproj");
                _guiVersion = xmlDoc.SelectSingleNode("//Project/PropertyGroup/Version")?.InnerText;

                return _guiVersion;
            }
        }

        private static void DeleteDirectoryIfExist(string directory)
        {
            try
            {
                Directory.Delete(directory, true);
            }
            catch (DirectoryNotFoundException) {}
        }
        
        private static void CleanBuildTools()
        {
            if (File.Exists(GuiConverterExecutable))
            {
                File.Delete(GuiConverterExecutable);
            }
            
            if (File.Exists(WarpPackerExecutable))
            {
                File.Delete(WarpPackerExecutable);
            }
        }
        
        private static void CleanOutputDirectories(IEnumerable<string> outputDirs)
        {
            foreach (var outputDir in outputDirs)
            {
                try
                {
                    if (!File.GetAttributes(outputDir).HasFlag(FileAttributes.Directory))
                    {
                        File.Delete(outputDir);
                    }
                }
                catch (FileNotFoundException) {}
                catch (DirectoryNotFoundException) {}
                
                DeleteDirectoryIfExist(outputDir);
            }
        }

        private static void RestoreSolution()
        {
            ProcessAsyncHelper
                .RunAsync(new ProcessStartInfo("dotnet", "restore"))
                .GetAwaiter()
                .GetResult();
        }

        private static void MakeOutputDirectories(IEnumerable<string> outputDirs)
        {
            foreach (var outputDir in outputDirs)
            {
                Directory.CreateDirectory(outputDir);
            }
        }

        private static void PublishDarwinCli()
        {
            Run("dotnet", "publish splitNspSharp -c contained -r osx.10.10-x64");
            Run("dotnet", "publish splitNspSharp -c dependent -r osx.10.10-x64");
        }

        private static void PublishDarwinGui()
        {
            Run("dotnet", "publish splitNspSharpGui.Mac -c contained");
            Run("dotnet", "publish splitNspSharpGui.Mac -c dependent");
        }

        private static void PublishLinuxCli()
        {
            Run("dotnet", "publish splitNspSharp -c contained -r linux-x64");
            Run("dotnet", "publish splitNspSharp -c dependent -r linux-x64");
        }

        private static void PublishLinuxCliDocker()
        {
            Run("docker", "context use default");
            Run("docker", "build -t split -f Dockerfile-splitNspSharpLinux .");
            Run("docker", "run --name splitc --rm -dit split");
            Run("docker", "cp splitc:\"/temp/app/splitNspSharp/bin/\" splitNspSharp/");
            Run("docker", "rm -f splitc");
            Run("docker", "image rm split");
        }

        private static void PublishLinuxGui()
        {
            Run("dotnet", "publish splitNspSharpGui.Gtk -c contained");
            Run("dotnet", "publish splitNspSharpGui.Gtk -c dependent");
        }
        
        private static void PublishLinuxGuiDocker()
        {
            Run("docker", "context use default");
            Run("docker", "build -t split -f Dockerfile-splitNspSharpGuiGtk .");
            Run("docker", "run --name splitc --rm -dit split");
            Run("docker", "cp splitc:\"/temp/app/splitNspSharpGui.Gtk/bin/\" splitNspSharpGui.Gtk/");
            Run("docker", "rm -f splitc");
            Run("docker", "image rm split");
        }

        private static void PublishWindowsCli()
        {
            Run("dotnet", "publish splitNspSharp -c contained -r win-x64");
            Run("dotnet", "publish splitNspSharp -c dependent -r win-x64");
        }

        private static void PublishWindowsCliFramework()
        {
            Run("dotnet", "publish splitNspSharp -c framework -r win-x64");
        }

        private static void PublishWindowsCliDocker()
        {
            Run("docker", "context use 2019-box");
            Run("docker", "build -t split -f Dockerfile-splitNspSharpWindows .");
            Run("docker", "run --name splitc --rm -dit split");
            Run("docker", "cp splitc:\"c:/temp/splitNspSharp/bin/\" splitNspSharp/");
            Run("docker", "rm -f splitc");
            Run("docker", "image rm split");
        }
        
        private static void PublishWindowsGui()
        {
            Run("dotnet", "publish splitNspSharpGui.Wpf -c contained");
            Run("dotnet", "publish splitNspSharpGui.Wpf -c dependent");
        }

        private static void PublishWindowsGuiDocker()
        {
            Run("docker", "context use 2019-box");
            Run("docker", "build -t split -f Dockerfile-splitNspSharpGuiWpf .");
            Run("docker", "run --name splitc --rm -dit split");
            Run("docker", "cp splitc:\"c:/temp/splitNspSharpGui.Wpf/bin/\" splitNspSharpGui.Wpf/");
            Run("docker", "rm -f splitc");
            Run("docker", "image rm split");
        }
        
        private static void PublishWindowsGuiFramework()
        {
            Run("dotnet", "publish splitNspSharpGui.Wpf -c framework");
        }

        private static void PublishCli()
        {
            PublishDarwinCli();

            if (Platform != OSPlatform.Linux && DockerHasDefaultContext)
            {
                PublishLinuxCliDocker();
            }
            else
            {
                PublishLinuxCli();
            }

            if (Platform != OSPlatform.Windows && DockerHasWindowsContext)
            {
                PublishWindowsCliDocker();
            }
            else 
            {
                PublishWindowsCli();
            }
            
            PublishWindowsCliFramework();
        }
        
        private static void PublishGui()
        {
            PublishDarwinGui();

            if (Platform != OSPlatform.Linux && DockerHasDefaultContext)
            {
                PublishLinuxGuiDocker();
            }
            else
            {
                PublishLinuxGui();
            }

            if (Platform == OSPlatform.Windows)
            {
                PublishWindowsGui();
            }
            else if (DockerHasWindowsContext)
            {
                PublishWindowsGuiDocker();
            }
            
            PublishWindowsGuiFramework();
        }

        private static void AcquireGuiConverter()
        {
            const string tempZip = "gui_converter.zip";
            
            var guiConverterUrl = new Dictionary<OSPlatform, string>
            {
                {OSPlatform.OSX, "https://github.com/joshua-software-dev/SharpGuiConverter/releases/download/v1.0.0.0/gui_converter_macos10_10_64.zip"},
                {OSPlatform.Linux, "https://github.com/joshua-software-dev/SharpGuiConverter/releases/download/v1.0.0.0/gui_converter_linux_64.zip"},
                {OSPlatform.Windows, "https://github.com/joshua-software-dev/SharpGuiConverter/releases/download/v1.0.0.0/gui_converter_win_64.zip"},
            }[Platform];

            using (var client = new WebClient())
            {
                client.DownloadFile(guiConverterUrl, tempZip);
            }
            
            ZipFile.ExtractToDirectory(tempZip, Environment.CurrentDirectory, true);
            File.Delete(tempZip);

            if (Platform != OSPlatform.Windows)
            {
                var unixFileInfo = new UnixFileInfo(GuiConverterExecutable);
                unixFileInfo.FileAccessPermissions |= FileAccessPermissions.UserExecute;
                unixFileInfo.Refresh();
            }
        }
        
        private static void AcquireWarpPacker()
        {
            var warpPackerUrl = new Dictionary<OSPlatform, string>
            {
                {OSPlatform.OSX, "https://github.com/dgiagio/warp/releases/download/v0.3.0/macos-x64.warp-packer"},
                {OSPlatform.Linux, "https://github.com/dgiagio/warp/releases/download/v0.3.0/linux-x64.warp-packer"},
                {OSPlatform.Windows, "https://github.com/dgiagio/warp/releases/download/v0.3.0/windows-x64.warp-packer.exe"},
            }[Platform];

            using (var client = new WebClient())
            {
                client.DownloadFile(warpPackerUrl, WarpPackerExecutable);
            }
            
            if (Platform != OSPlatform.Windows)
            {
                var unixFileInfo = new UnixFileInfo(WarpPackerExecutable);
                unixFileInfo.FileAccessPermissions |= FileAccessPermissions.UserExecute;
                unixFileInfo.Refresh();
            }
        }

        private static void CreateArchiveFromFile(string inputFile, string outputZipPath)
        {
            using (var zip = ZipFile.Open(outputZipPath, ZipArchiveMode.Create))
            {
                zip.CreateEntryFromFile(inputFile, Path.GetFileName(inputFile), CompressionLevel.Optimal);
            }
        }
        
        private static void ZipOutputCli()
        {
            CreateArchiveFromFile(
                "splitNspSharp/bin/contained/netcoreapp3.1/linux-x64/publish/splitNspSharp", 
                $"release_binaries/splitNspSharp/splitNspSharp-Linux-core-contained-{CliVersion}.zip"
            );

            CreateArchiveFromFile(
                "splitNspSharp/bin/dependent/netcoreapp3.1/linux-x64/publish/splitNspSharp", 
                $"release_binaries/splitNspSharp/splitNspSharp-Linux-core-dependent-{CliVersion}.zip"
            );

            CreateArchiveFromFile(
                "splitNspSharp/bin/contained/netcoreapp3.1/osx.10.10-x64/publish/splitNspSharp", 
                $"release_binaries/splitNspSharp/splitNspSharp-OSx64-core-contained-{CliVersion}.zip"
            );

            CreateArchiveFromFile(
                "splitNspSharp/bin/dependent/netcoreapp3.1/osx.10.10-x64/publish/splitNspSharp", 
                $"release_binaries/splitNspSharp/splitNspSharp-OSx64-core-dependent-{CliVersion}.zip"
            );

            CreateArchiveFromFile(
                "splitNspSharp/bin/contained/netcoreapp3.1/win-x64/publish/splitNspSharp.exe", 
                $"release_binaries/splitNspSharp/splitNspSharp-Win64-core-contained-{CliVersion}.zip"
            );

            CreateArchiveFromFile(
                "splitNspSharp/bin/dependent/netcoreapp3.1/win-x64/publish/splitNspSharp.exe", 
                $"release_binaries/splitNspSharp/splitNspSharp-Win64-core-dependent-{CliVersion}.zip"
            );
            
            Run(
                WarpPackerExecutable,
                string.Join(
                    ' ', 
                    "--arch", 
                    "windows-x64", 
                    "--input_dir",
                    "splitNspSharp/bin/framework/net48/win-x64/publish/",
                    "--exec",
                    "splitNspSharp.exe",
                    "--output",
                    "release_binaries/splitNspSharp.exe"
                )
            );
            
            CreateArchiveFromFile(
                "release_binaries/splitNspSharp.exe",
                $"release_binaries/splitNspSharp/splitNspSharp-Win64-framework-dependent-{CliVersion}.zip"
            );
            
            File.Delete("release_binaries/splitNspSharp.exe");
        }

        private static void ZipOutputGui()
        {
            CreateArchiveFromFile(
                "splitNspSharpGui.Gtk/bin/contained/netcoreapp3.1/linux-x64/publish/splitNspSharpGui.Gtk", 
                $"release_binaries/splitNspSharpGui/splitNspSharpGui-Linux-core-contained-{GuiVersion}.zip"
            );
            
            CreateArchiveFromFile(
                "splitNspSharpGui.Gtk/bin/dependent/netcoreapp3.1/linux-x64/publish/splitNspSharpGui.Gtk", 
                $"release_binaries/splitNspSharpGui/splitNspSharpGui-Linux-core-dependent-{GuiVersion}.zip"
            );
            
            ZipFile.CreateFromDirectory(
                "splitNspSharpGui.Mac/bin/contained/netcoreapp3.1/osx.10.10-x64/splitNspSharpGui.Mac.app", 
                $"release_binaries/splitNspSharpGui/splitNspSharpGui-OSx64-core-contained-{GuiVersion}.zip"
            );
            
            ZipFile.CreateFromDirectory(
                "splitNspSharpGui.Mac/bin/dependent/netcoreapp3.1/osx.10.10-x64/splitNspSharpGui.Mac.app",
                $"release_binaries/splitNspSharpGui/splitNspSharpGui-OSx64-core-dependent-{GuiVersion}.zip"
            );

            if (File.Exists("splitNspSharpGui.Wpf/bin/contained/netcoreapp3.1/win-x64/publish/splitNspSharpGui.Wpf.exe"))
            {
                File.Copy(
                    "splitNspSharpGui.Wpf/bin/contained/netcoreapp3.1/win-x64/publish/splitNspSharpGui.Wpf.exe", 
                    "splitNspSharpGui.Wpf/bin/contained/netcoreapp3.1/win-x64/publish/splitNspSharpGui.exe",
                    true
                );
                
                Run(GuiConverterExecutable, "splitNspSharpGui.Wpf/bin/contained/netcoreapp3.1/win-x64/publish/splitNspSharpGui.exe");

                CreateArchiveFromFile(
                    "splitNspSharpGui.Wpf/bin/contained/netcoreapp3.1/win-x64/publish/splitNspSharpGui.exe", 
                    $"release_binaries/splitNspSharpGui/splitNspSharpGui-Win64-core-contained-{GuiVersion}.zip"
                );
            }
            
            if (File.Exists("splitNspSharpGui.Wpf/bin/dependent/netcoreapp3.1/win-x64/publish/splitNspSharpGui.Wpf.exe"))
            {
                File.Copy(
                    "splitNspSharpGui.Wpf/bin/dependent/netcoreapp3.1/win-x64/publish/splitNspSharpGui.Wpf.exe", 
                    "splitNspSharpGui.Wpf/bin/dependent/netcoreapp3.1/win-x64/publish/splitNspSharpGui.exe",
                    true
                );
                
                Run(GuiConverterExecutable, "splitNspSharpGui.Wpf/bin/dependent/netcoreapp3.1/win-x64/publish/splitNspSharpGui.exe");

                CreateArchiveFromFile(
                    "splitNspSharpGui.Wpf/bin/dependent/netcoreapp3.1/win-x64/publish/splitNspSharpGui.exe", 
                    $"release_binaries/splitNspSharpGui/splitNspSharpGui-Win64-core-dependent-{GuiVersion}.zip"
                );
            }
            
            Run(
                WarpPackerExecutable,
                string.Join(
                    ' ', 
                    "--arch", 
                    "windows-x64", 
                    "--input_dir",
                    "splitNspSharpGui.Wpf/bin/framework/net48/win-x64/publish/",
                    "--exec",
                    "splitNspSharpGui.Wpf.exe",
                    "--output",
                    "splitNspSharpGui.Wpf/bin/framework/net48/win-x64/publish/splitNspSharpGui.exe"
                )
            );

            Run(GuiConverterExecutable, "splitNspSharpGui.Wpf/bin/framework/net48/win-x64/publish/splitNspSharpGui.exe");
            
            CreateArchiveFromFile(
                "splitNspSharpGui.Wpf/bin/framework/net48/win-x64/publish/splitNspSharpGui.exe",
                $"release_binaries/splitNspSharpGui/splitNspSharpGui-Win64-framework-dependent-{GuiVersion}.zip"
            );
        }
        
        private static void Main(string[] args)
        {
            string[] outputDirs = {
                "release_binaries", 
                "release_binaries/splitNspSharp", 
                "release_binaries/splitNspSharpGui"
            };

            Target("clean-build-tools", CleanBuildTools);

            Target(
                "clean-output-dirs",
                DependsOn("clean-build-tools"),
                () => CleanOutputDirectories(outputDirs)
            );
            
            Target(
                "restore",
                RestoreSolution
            );
            
            Target(
                "clean",
                DependsOn("clean-build-tools", "clean-output-dirs"),
                () =>
                {
                    DeleteDirectoryIfExist("_build/bin/");
                    DeleteDirectoryIfExist("_build/obj/");
                    DeleteDirectoryIfExist("splitNspSharp/bin/");
                    DeleteDirectoryIfExist("splitNspSharp/obj/");
                    DeleteDirectoryIfExist("splitNspSharpGui/bin/");
                    DeleteDirectoryIfExist("splitNspSharpGui/obj/");
                    DeleteDirectoryIfExist("splitNspSharpGui.Gtk/bin/");
                    DeleteDirectoryIfExist("splitNspSharpGui.Gtk/obj/");
                    DeleteDirectoryIfExist("splitNspSharpGui.Mac/bin/");
                    DeleteDirectoryIfExist("splitNspSharpGui.Mac/obj/");
                    DeleteDirectoryIfExist("splitNspSharpGui.Wpf/bin/");
                    DeleteDirectoryIfExist("splitNspSharpGui.Wpf/obj/");
                    DeleteDirectoryIfExist("splitNspSharpLib/bin/");
                    DeleteDirectoryIfExist("splitNspSharpLib/obj/");
                }
            );
            
            Target(
                "make-output-dirs", 
                DependsOn("clean-output-dirs"), 
                () => MakeOutputDirectories(outputDirs)
            );

            Target(
                "publish-cli",
                DependsOn("make-output-dirs", "restore"),
                PublishCli
            );
            
            Target(
                "publish-gui",
                DependsOn("make-output-dirs", "restore"),
                PublishGui
            );

            Target("acquire-gui-converter", AcquireGuiConverter);

            Target("acquire-warp-packer", AcquireWarpPacker);

            Target(
                "zip-output-cli",
                DependsOn("publish-cli", "acquire-warp-packer"),
                ZipOutputCli
            );
            
            Target(
                "zip-output-gui",
                DependsOn("publish-gui", "acquire-warp-packer", "acquire-gui-converter"),
                ZipOutputGui
            );
            
            Target(
                "default",
                DependsOn("zip-output-cli", "zip-output-gui"),
                CleanBuildTools
            );

            RunTargetsAndExit(args);
        }
    }
}