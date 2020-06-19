#!/usr/bin/env bash

CYAN="\x1b[1m\x1B[36m"
CLEAR="\x1B[0m"

current_pwd="$PWD"
current_version=$(cat version.txt)

mkdir release_binaries
mkdir release_binaries/splitNspSharp
rm -rf release_binaries/splitNspSharp/*

printf "${CYAN}dotnet restore splitNspSharpLib${CLEAR}\n"
dotnet restore splitNspSharpLib

printf "${CYAN}dotnet clean splitNspSharpLib${CLEAR}\n"
dotnet clean splitNspSharpLib

printf "${CYAN}dotnet restore splitNspSharp${CLEAR}\n"
dotnet restore splitNspSharp

printf "${CYAN}dotnet clean splitNspSharp${CLEAR}\n"
dotnet clean splitNspSharp

printf "${CYAN}dotnet.exe publish splitNspSharp -c contained -r win-x64 /p:Version=${current_version}${CLEAR}\n"
dotnet.exe publish splitNspSharp -c contained -r win-x64 /p:Version=${current_version}

printf "${CYAN}dotnet.exe publish splitNspSharp -c dependent -r win-x64 /p:Version=${current_version}${CLEAR}\n"
dotnet.exe publish splitNspSharp -c dependent -r win-x64 /p:Version=${current_version}

printf "${CYAN}dotnet.exe publish splitNspSharp -c framework -r win-x64 /p:Version=${current_version}${CLEAR}\n"
dotnet.exe publish splitNspSharp -c framework -r win-x64 /p:Version=${current_version}

printf "${CYAN}dotnet publish splitNspSharp -c contained -r osx.10.10-x64 /p:Version=${current_version}${CLEAR}\n"
dotnet publish splitNspSharp -c contained -r osx.10.10-x64 /p:Version=${current_version}

printf "${CYAN}dotnet publish splitNspSharp -c dependent -r osx.10.10-x64 /p:Version=${current_version}${CLEAR}\n"
dotnet publish splitNspSharp -c dependent -r osx.10.10-x64 /p:Version=${current_version}

printf "${CYAN}dotnet publish splitNspSharp -c contained -r linux-x64 /p:Version=${current_version}${CLEAR}\n"
dotnet publish splitNspSharp -c contained -r linux-x64 /p:Version=${current_version}

printf "${CYAN}dotnet publish splitNspSharp -c dependent -r linux-x64 /p:Version=${current_version}${CLEAR}\n"
dotnet publish splitNspSharp -c dependent -r linux-x64 /p:Version=${current_version}

cd splitNspSharp/bin/contained/netcoreapp3.1/linux-x64/publish/
printf "${CYAN}zip -9 ../../../../../../release_binaries/splitNspSharp/splitNspSharp-Linux-core-contained-${current_version}.zip splitNspSharp${CLEAR}\n"
zip -9 ../../../../../../release_binaries/splitNspSharp/splitNspSharp-Linux-core-contained-${current_version}.zip splitNspSharp
cd ${current_pwd}

cd splitNspSharp/bin/dependent/netcoreapp3.1/linux-x64/publish/
printf "${CYAN}zip -9 ../../../../../../release_binaries/splitNspSharp/splitNspSharp-Linux-core-dependent-${current_version}.zip splitNspSharp${CLEAR}\n"
zip -9 ../../../../../../release_binaries/splitNspSharp/splitNspSharp-Linux-core-dependent-${current_version}.zip splitNspSharp
cd ${current_pwd}

cd splitNspSharp/bin/contained/netcoreapp3.1/osx.10.10-x64/publish/
printf "${CYAN}zip -9 -r ../../../../../../release_binaries/splitNspSharp/splitNspSharp-OSx64-core-contained-${current_version}.zip splitNspSharp${CLEAR}\n"
zip -9 -r ../../../../../../release_binaries/splitNspSharp/splitNspSharp-OSx64-core-contained-${current_version}.zip splitNspSharp
cd ${current_pwd}

cd splitNspSharp/bin/dependent/netcoreapp3.1/osx.10.10-x64/publish/
printf "${CYAN}zip -9 -r ../../../../../../release_binaries/splitNspSharp/splitNspSharp-OSx64-core-dependent-${current_version}.zip splitNspSharp${CLEAR}\n"
zip -9 -r ../../../../../../release_binaries/splitNspSharp/splitNspSharp-OSx64-core-dependent-${current_version}.zip splitNspSharp
cd ${current_pwd}

cd splitNspSharp/bin/contained/netcoreapp3.1/win-x64/publish/
printf "${CYAN}zip -9 ../../../../../../release_binaries/splitNspSharp/splitNspSharp-Win64-core-contained-${current_version}.zip splitNspSharp.exe${CLEAR}\n"
zip -9 ../../../../../../release_binaries/splitNspSharp/splitNspSharp-Win64-core-contained-${current_version}.zip splitNspSharp.exe
cd ${current_pwd}

cd splitNspSharp/bin/dependent/netcoreapp3.1/win-x64/publish/
printf "${CYAN}zip -9 ../../../../../../release_binaries/splitNspSharp/splitNspSharp-Win64-core-dependent-${current_version}.zip splitNspSharp.exe${CLEAR}\n"
zip -9 ../../../../../../release_binaries/splitNspSharp/splitNspSharp-Win64-core-dependent-${current_version}.zip splitNspSharp.exe
cd ${current_pwd}

printf "${CYAN}warp-packer --arch windows-x64 --input_dir splitNspSharp/bin/framework/net48/win-x64/publish --exec splitNspSharp.exe --output release_binaries/splitNspSharp/splitNspSharp.exe${CLEAR}\n"
warp-packer --arch windows-x64 --input_dir splitNspSharp/bin/framework/net48/win-x64/publish --exec splitNspSharp.exe --output release_binaries/splitNspSharp/splitNspSharp.exe

printf "${CYAN}zip -9 -j release_binaries/splitNspSharp/splitNspSharp-Win64-framework-dependent-${current_version}.zip release_binaries/splitNspSharp/splitNspSharp.exe${CLEAR}\n"
zip -9 -j release_binaries/splitNspSharp/splitNspSharp-Win64-framework-dependent-${current_version}.zip release_binaries/splitNspSharp/splitNspSharp.exe
rm release_binaries/splitNspSharp/splitNspSharp.exe
