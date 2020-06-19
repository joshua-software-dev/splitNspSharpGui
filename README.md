# splitNspSharpGui
A cross platform command line/GUI application for splitting NSP files. There are two releases of this program, one is "splitNspSharp" and the other is "splitNspSharpGui". The former provides a similar interface to the command line python script by AnalogMan151, and the latter is a mouse driven program that does not require the use of a command line.

# FAQ

## What is the difference between the different builds? Contained? Dependent? Framework?

In addition to cross platform releases, there are multiple variants of each release. Each platform has two major releases, one is "contained" and the other is "dependent".

"Contained" releases include most everything needed to run the release inside the executable itself, which increases the size of the executable considerably.

Alternative "Dependent" releases require that a .NET runtime is already installed and available system-wide on a machine. This allows them to only include components directly related to the program, which lowers the size of the executable significantly. In exchange, some prior setup is required of the user, and the runtime + the release take up more space combined than the "contained" releases do.

"Framework" releases are themselves a dependent release, although instead of depending upon the ".NET Core" runtime they depend on the ".NET Framework" runtime. Windows 10 comes preinstalled with the .NET Framework in most standard configurations, which allows for a release with a small executable, without added expectations placed on the user. However, the release of the .Net Framework that comes preinstalled on Windows 10 is quite old, and if no other programs required the user to update it for any reason, splitNspSharp could encounter unexpected issues that would not be present on the .NET Core powered releases. If you're experiencing any bugs while running the .NET Framework dependent release, you may wish to use a .NET Core powered release.

I'll do my best to try and fix bugs/issues for all releases, but I can't possibly test every configuration, and issues may arise in unusual envorinments.

## The program immediately exits when I try to run it! Why is it broken?

You may have accidentally downloaded the command line version of the program. There are two versions "splitNspSharp" and "splitNspSharpGui". Only the "Gui" version contained a mouse driven menu, so ensure you download the right version.

# Download

Downloads can be found here: https://github.com/joshua-software-dev/splitNspSharpGui/releases

If you're running a .NET Core dependent release, make sure you download the .NET Core runtime first: https://dotnet.microsoft.com/download

If you're using Windows 10, the recommended release is the .NET Framework dependent releases, for their small size and lack of seperate dependencies.
