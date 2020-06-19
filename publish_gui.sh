#!/usr/bin/env bash

CYAN="\x1b[1m\x1B[36m"
CLEAR="\x1B[0m"

current_pwd="$PWD"
current_version=$(cat version.txt)

mkdir release_binaries
mkdir release_binaries/splitNspSharpGui
rm -rf release_binaries/splitNspSharpGui/*

printf "${CYAN}dotnet restore splitNspSharpLib${CLEAR}\n"
dotnet restore splitNspSharpLib

printf "${CYAN}dotnet clean splitNspSharpLib${CLEAR}\n"
dotnet clean splitNspSharpLib

printf "${CYAN}dotnet restore splitNspSharpGui${CLEAR}\n"
dotnet restore splitNspSharpGui

printf "${CYAN}dotnet clean splitNspSharpGui${CLEAR}\n"
dotnet clean splitNspSharpGui

printf "${CYAN}dotnet restore splitNspSharpGui.Wpf${CLEAR}\n"
dotnet restore splitNspSharpGui.Wpf

printf "${CYAN}dotnet clean splitNspSharpGui.Wpf${CLEAR}\n"
dotnet clean splitNspSharpGui.Wpf

printf "${CYAN}dotnet.exe publish splitNspSharpGui.Wpf -c contained -r win-x64 /p:Version=${current_version}${CLEAR}\n"
dotnet.exe publish splitNspSharpGui.Wpf -c contained -r win-x64 /p:Version=${current_version}

printf "${CYAN}dotnet.exe publish splitNspSharpGui.Wpf -c dependent -r win-x64 /p:Version=${current_version}${CLEAR}\n"
dotnet.exe publish splitNspSharpGui.Wpf -c dependent -r win-x64 /p:Version=${current_version}

printf "${CYAN}dotnet.exe publish splitNspSharpGui.Wpf -c framework -r win-x64 /p:Version=${current_version}${CLEAR}\n"
dotnet.exe publish splitNspSharpGui.Wpf -c framework -r win-x64 /p:Version=${current_version}

printf "${CYAN}dotnet restore splitNspSharpGui.Gtk${CLEAR}\n"
dotnet restore splitNspSharpGui.Gtk

printf "${CYAN}dotnet clean splitNspSharpGui.Gtk${CLEAR}\n"
dotnet clean splitNspSharpGui.Gtk

printf "${CYAN}dotnet publish splitNspSharpGui.Gtk -c contained /p:Version=${current_version}${CLEAR}\n"
dotnet publish splitNspSharpGui.Gtk -c contained /p:Version=${current_version}

printf "${CYAN}dotnet publish splitNspSharpGui.Gtk -c dependent /p:Version=${current_version}${CLEAR}\n"
dotnet publish splitNspSharpGui.Gtk -c dependent /p:Version=${current_version}

printf "${CYAN}dotnet restore splitNspSharpGui.Mac${CLEAR}\n"
dotnet restore splitNspSharpGui.Mac

printf "${CYAN}dotnet clean splitNspSharpGui.Mac${CLEAR}\n"
dotnet clean splitNspSharpGui.Mac

printf "${CYAN}dotnet publish splitNspSharpGui.Mac -c contained /p:Version=${current_version}${CLEAR}\n"
dotnet publish splitNspSharpGui.Mac -c contained /p:Version=${current_version}

printf "${CYAN}dotnet publish splitNspSharpGui.Mac -c dependent /p:Version=${current_version}${CLEAR}\n"
dotnet publish splitNspSharpGui.Mac -c dependent /p:Version=${current_version}

cd splitNspSharpGui.Gtk/bin/contained/netcoreapp3.1/linux-x64/publish
mv splitNspSharpGui.Gtk splitNspSharpGui
printf "${CYAN}zip -9 ../../../../../../release_binaries/splitNspSharpGui/splitNspSharpGui-Linux-core-contained-${current_version}.zip splitNspSharpGui${CLEAR}\n"
zip -9 ../../../../../../release_binaries/splitNspSharpGui/splitNspSharpGui-Linux-core-contained-${current_version}.zip splitNspSharpGui
cd ${current_pwd}

cd splitNspSharpGui.Gtk/bin/dependent/netcoreapp3.1/linux-x64/publish
mv splitNspSharpGui.Gtk splitNspSharpGui
printf "${CYAN}zip -9 ../../../../../../release_binaries/splitNspSharpGui/splitNspSharpGui-Linux-core-dependent-${current_version}.zip splitNspSharpGui${CLEAR}\n"
zip -9 ../../../../../../release_binaries/splitNspSharpGui/splitNspSharpGui-Linux-core-dependent-${current_version}.zip splitNspSharpGui
cd ${current_pwd}

cd splitNspSharpGui.Mac/bin/contained/netcoreapp3.1/osx.10.10-x64/
printf "${CYAN}zip -9 -r ../../../../../release_binaries/splitNspSharpGui/splitNspSharpGui-OSx64-core-contained-${current_version}.zip splitNspSharpGui.Mac.app${CLEAR}\n"
zip -9 -r ../../../../../release_binaries/splitNspSharpGui/splitNspSharpGui-OSx64-core-contained-${current_version}.zip splitNspSharpGui.Mac.app
cd ${current_pwd}

cd splitNspSharpGui.Mac/bin/dependent/netcoreapp3.1/osx.10.10-x64/
printf "${CYAN}zip -9 -r ../../../../../release_binaries/splitNspSharpGui/splitNspSharpGui-OSx64-core-dependent-${current_version}.zip splitNspSharpGui.Mac.app${CLEAR}\n"
zip -9 -r ../../../../../release_binaries/splitNspSharpGui/splitNspSharpGui-OSx64-core-dependent-${current_version}.zip splitNspSharpGui.Mac.app
cd ${current_pwd}

cd splitNspSharpGui.Wpf/bin/contained/netcoreapp3.1/win-x64/publish
mv splitNspSharpGui.Wpf.exe splitNspSharpGui.exe
printf "${CYAN}zip -9 ../../../../../../release_binaries/splitNspSharpGui/splitNspSharpGui-Win64-core-contained-${current_version}.zip splitNspSharpGui.exe${CLEAR}\n"
zip -9 ../../../../../../release_binaries/splitNspSharpGui/splitNspSharpGui-Win64-core-contained-${current_version}.zip splitNspSharpGui.exe
cd ${current_pwd}

cd splitNspSharpGui.Wpf/bin/dependent/netcoreapp3.1/win-x64/publish
mv splitNspSharpGui.Wpf.exe splitNspSharpGui.exe
printf "${CYAN}zip -9 ../../../../../../release_binaries/splitNspSharpGui/splitNspSharpGui-Win64-core-dependent-${current_version}.zip splitNspSharpGui.exe${CLEAR}\n"
zip -9 ../../../../../../release_binaries/splitNspSharpGui/splitNspSharpGui-Win64-core-dependent-${current_version}.zip splitNspSharpGui.exe
cd ${current_pwd}

printf "${CYAN}warp-packer --arch windows-x64 --input_dir splitNspSharpGui.Wpf/bin/framework/net48/win-x64/publish --exec splitNspSharpGui.Wpf.exe --output release_binaries/splitNspSharpGui/splitNspSharpGui.exe${CLEAR}\n"
warp-packer --arch windows-x64 --input_dir splitNspSharpGui.Wpf/bin/framework/net48/win-x64/publish --exec splitNspSharpGui.Wpf.exe --output release_binaries/splitNspSharpGui/splitNspSharpGui.exe

printf "${CYAN}editbin /subsystem:windows release_binaries/splitNspSharpGui/splitNspSharpGui.exe${CLEAR}\n"
editbin /subsystem:windows release_binaries/splitNspSharpGui/splitNspSharpGui.exe

printf "${CYAN}zip -9 -j release_binaries/splitNspSharpGui/splitNspSharpGui-Win64-framework-dependent-${current_version}.zip release_binaries/splitNspSharpGui/splitNspSharpGui.exe${CLEAR}\n"
zip -9 -j release_binaries/splitNspSharpGui/splitNspSharpGui-Win64-framework-dependent-${current_version}.zip release_binaries/splitNspSharpGui/splitNspSharpGui.exe
rm release_binaries/splitNspSharpGui/splitNspSharpGui.exe
