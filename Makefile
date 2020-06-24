CP=cp -r
MKDIR=mkdir -p
MV=mv
RM=rm -rf
UNZIP=unzip
ZIP=zip

CLI_VERSION := $(shell sed -n -e 's/.*<Version>\(.*\)<\/Version>.*/\1/p' Version/CliVersion.csproj)
GUI_VERSION := $(shell sed -n -e 's/.*<Version>\(.*\)<\/Version>.*/\1/p' Version/GuiVersion.csproj)
THIS_FILE := $(lastword $(MAKEFILE_LIST))

ifeq ($(shell uname),Darwin)
	BINARY_EXTENSION :=
	OS_PLATFORM := darwin
endif

ifeq ($(shell uname),Linux)
	BINARY_EXTENSION :=
	OS_PLATFORM := linux
endif

ifeq ($(OS),Windows_NT)
	BINARY_EXTENSION := exe
	OS_PLATFORM := windows
	WINDOWS_NATIVE := _native
else
	WINDOWS_NATIVE :=
endif

.PHONY: all publish_cli publish_gui

all: publish_cli publish_gui acquire_build_tools zip_output_cli_files zip_output_gui_files delete_build_tools

delete_cache:
	@echo Deleting cache
	$(RM) splitNspSharp/bin
	$(RM) splitNspSharp/obj
	$(RM) splitNspSharpGui/bin
	$(RM) splitNspSharpGui/obj
	$(RM) splitNspSharpGui.Gtk/bin
	$(RM) splitNspSharpGui.Gtk/obj
	$(RM) splitNspSharpGui.Mac/bin
	$(RM) splitNspSharpGui.Mac/obj
	$(RM) splitNspSharpGui.Wpf/bin
	$(RM) splitNspSharpGui.Wpf/obj
	$(RM) splitNspSharpLib/bin
	$(RM) splitNspSharpLib/obj
	@echo

clean: delete_cache
	@echo "Deleting older final export binaries"
	$(RM) release_binaries/bin/
	$(RM) release_binaries/splitNspSharp/*
	$(RM) release_binaries/splitNspSharpGui/*
	@echo

restore:
	dotnet restore
	dotnet clean

make_release_dirs:
	@echo Making prebuilt binary folders for output
	$(MKDIR) release_binaries
	$(MKDIR) release_binaries/splitNspSharp
	$(MKDIR) release_binaries/splitNspSharpGui
	@echo
	
publish_darwin:
	dotnet publish splitNspSharp -c contained -r osx.10.10-x64
	dotnet publish splitNspSharp -c dependent -r osx.10.10-x64

publish_darwin_gui:
	dotnet publish splitNspSharpGui.Mac -c contained
	dotnet publish splitNspSharpGui.Mac -c dependent

publish_linux:
	docker context use default
	docker build -t split -f Dockerfile-splitNspSharpLinux .
	docker run --name splitc --rm -dit split
	docker cp splitc:"/temp/app/splitNspSharp/bin/" splitNspSharp/
	docker rm -f splitc
	docker image rm split

publish_linux_gui:
	docker context use default
	docker build -t split -f Dockerfile-splitNspSharpGuiGtk .
	docker run --name splitc --rm -dit split
	docker cp splitc:"/temp/app/splitNspSharpGui.Gtk/bin/" splitNspSharpGui.Gtk/
	docker rm -f splitc
	docker image rm split

publish_windows:
	docker context use 2019-box
	docker build -t split -f Dockerfile-splitNspSharpWindows .
	docker run --name splitc --rm -dit split
	docker cp splitc:"c:/temp/splitNspSharp/bin/" splitNspSharp/
	docker rm -f splitc
	docker image rm split

publish_windows_native:
	dotnet publish splitNspSharp -c contained -r win-x64
	dotnet publish splitNspSharp -c dependent -r win-x64
	dotnet publish splitNspSharp -c framework -r win-x64

publish_windows_gui:
	docker context use 2019-box
	docker build -t split -f Dockerfile-splitNspSharpGuiWpf .
	docker run --name splitc --rm -dit split
	docker cp splitc:"c:/temp/splitNspSharpGui.Wpf/bin/" splitNspSharpGui.Wpf/
	docker rm -f splitc
	docker image rm split

publish_windows_gui_native:
	dotnet publish splitNspSharpGui.Wpf -c contained
	dotnet publish splitNspSharpGui.Wpf -c dependent
	dotnet publish splitNspSharpGui.Wpf -c framework

acquire_build_tools_darwin:
	curl -L -O https://github.com/joshua-software-dev/SharpGuiConverter/releases/download/v1.0.0.0/gui_converter_macos10_10_64.zip
	curl -L -O https://github.com/dgiagio/warp/releases/download/v0.3.0/macos-x64.warp-packer
	
	$(UNZIP) gui_converter_macos10_10_64.zip
	$(RM) gui_converter_macos10_10_64.zip
	$(MV) macos-x64.warp-packer warp-packer
	chmod +x gui_converter
	chmod +x warp-packer

acquire_build_tools_linux:
	curl -L -O https://github.com/joshua-software-dev/SharpGuiConverter/releases/download/v1.0.0.0/gui_converter_linux_64.zip
	curl -L -O https://github.com/dgiagio/warp/releases/download/v0.3.0/linux-x64.warp-packer
	
	$(UNZIP) gui_converter_linux_64.zip
	$(RM) gui_converter_linux_64.zip
	$(MV) linux-x64.warp-packer warp-packer
	chmod +x gui_converter
	chmod +x warp-packer

acquire_build_tools_windows:
	curl -L -O https://github.com/joshua-software-dev/SharpGuiConverter/releases/download/v1.0.0.0/gui_converter_win_64.zip
	curl -L -O https://github.com/dgiagio/warp/releases/download/v0.3.0/windows-x64.warp-packer.exe
	
	$(UNZIP) gui_converter_win_64.zip
	$(RM) gui_converter_win_64.zip
	$(MV) windows-x64.warp-packer.exe warp-packer.exe

acquire_build_tools:
	@$(MAKE) -f $(THIS_FILE) acquire_build_tools_$(OS_PLATFORM)

publish_cli:
	@$(MAKE) -f $(THIS_FILE) publish_windows$(WINDOWS_NATIVE)
	@$(MAKE) -f $(THIS_FILE) publish_linux
	@$(MAKE) -f $(THIS_FILE) publish_darwin

publish_gui: make_release_dirs
	@$(MAKE) -f $(THIS_FILE) publish_windows_gui$(WINDOWS_NATIVE)
	@$(MAKE) -f $(THIS_FILE) publish_linux_gui
	@$(MAKE) -f $(THIS_FILE) publish_darwin_gui

zip_output_cli_files:
	$(ZIP) -9 -j release_binaries/splitNspSharp/splitNspSharp-Linux-core-contained-$(CLI_VERSION).zip splitNspSharp/bin/contained/netcoreapp3.1/linux-x64/publish/splitNspSharp
	$(ZIP) -9 -j release_binaries/splitNspSharp/splitNspSharp-Linux-core-dependent-$(CLI_VERSION).zip splitNspSharp/bin/dependent/netcoreapp3.1/linux-x64/publish/splitNspSharp
	
	$(ZIP) -9 -j release_binaries/splitNspSharp/splitNspSharp-OSx64-core-contained-$(CLI_VERSION).zip splitNspSharp/bin/contained/netcoreapp3.1/osx.10.10-x64/publish/splitNspSharp
	$(ZIP) -9 -j release_binaries/splitNspSharp/splitNspSharp-OSx64-core-dependent-$(CLI_VERSION).zip splitNspSharp/bin/dependent/netcoreapp3.1/osx.10.10-x64/publish/splitNspSharp
	
	$(ZIP) -9 -j release_binaries/splitNspSharp/splitNspSharp-Win64-core-contained-$(CLI_VERSION).zip splitNspSharp/bin/contained/netcoreapp3.1/win-x64/publish/splitNspSharp.exe
	$(ZIP) -9 -j release_binaries/splitNspSharp/splitNspSharp-Win64-core-dependent-$(CLI_VERSION).zip splitNspSharp/bin/dependent/netcoreapp3.1/win-x64/publish/splitNspSharp.exe
	
	./warp-packer$(BINARY_EXTENSION) --arch windows-x64 --input_dir splitNspSharp/bin/framework/net48/win-x64/publish/ --exec splitNspSharp.exe --output release_binaries/splitNspSharp.exe
	$(ZIP) -9 -j release_binaries/splitNspSharp/splitNspSharp-Win64-framework-dependent-$(CLI_VERSION).zip release_binaries/splitNspSharp.exe
	$(RM) release_binaries/splitNspSharp.exe

zip_output_gui_files:
	$(CP) splitNspSharpGui.Gtk/bin/contained/netcoreapp3.1/linux-x64/publish/splitNspSharpGui.Gtk splitNspSharpGui.Gtk/bin/contained/netcoreapp3.1/linux-x64/publish/splitNspSharpGui
	$(ZIP) -9 -j release_binaries/splitNspSharpGui/splitNspSharpGui-Linux-core-contained-$(GUI_VERSION).zip splitNspSharpGui.Gtk/bin/contained/netcoreapp3.1/linux-x64/publish/splitNspSharpGui
	
	$(CP) splitNspSharpGui.Gtk/bin/dependent/netcoreapp3.1/linux-x64/publish/splitNspSharpGui.Gtk splitNspSharpGui.Gtk/bin/dependent/netcoreapp3.1/linux-x64/publish/splitNspSharpGui
	$(ZIP) -9 -j release_binaries/splitNspSharpGui/splitNspSharpGui-Linux-core-dependent-$(GUI_VERSION).zip splitNspSharpGui.Gtk/bin/dependent/netcoreapp3.1/linux-x64/publish/splitNspSharpGui
	
	$(CP) splitNspSharpGui.Mac/bin/contained/netcoreapp3.1/osx.10.10-x64/splitNspSharpGui.Mac.app splitNspSharpGui.app
	$(ZIP) -9 -r release_binaries/splitNspSharpGui/splitNspSharpGui-OSx64-core-contained-$(GUI_VERSION).zip splitNspSharpGui.app
	$(RM) splitNspSharpGui.app
	
	$(CP) splitNspSharpGui.Mac/bin/dependent/netcoreapp3.1/osx.10.10-x64/splitNspSharpGui.Mac.app splitNspSharpGui.app
	$(ZIP) -9 -r release_binaries/splitNspSharpGui/splitNspSharpGui-OSx64-core-dependent-$(GUI_VERSION).zip splitNspSharpGui.app
	$(RM) splitNspSharpGui.app
	
	./warp-packer$(BINARY_EXTENSION) --arch windows-x64 --input_dir splitNspSharpGui.Wpf/bin/framework/net48/win-x64/publish/ --exec splitNspSharpGui.Wpf.exe --output splitNspSharpGui.Wpf/bin/framework/net48/win-x64/publish/splitNspSharpGui.exe
	./gui_converter$(BINARY_EXTENSION) splitNspSharpGui.Wpf/bin/contained/netcoreapp3.1/win-x64/publish/splitNspSharpGui.Wpf.exe
	./gui_converter$(BINARY_EXTENSION) splitNspSharpGui.Wpf/bin/dependent/netcoreapp3.1/win-x64/publish/splitNspSharpGui.Wpf.exe
	./gui_converter$(BINARY_EXTENSION) splitNspSharpGui.Wpf/bin/framework/net48/win-x64/publish/splitNspSharpGui.exe
	
	$(ZIP) -9 -j release_binaries/splitNspSharpGui/splitNspSharpGui-Win64-core-contained-$(GUI_VERSION).zip splitNspSharpGui.Wpf/bin/contained/netcoreapp3.1/win-x64/publish/splitNspSharpGui.Wpf.exe
	$(ZIP) -9 -j release_binaries/splitNspSharpGui/splitNspSharpGui-Win64-core-dependent-$(GUI_VERSION).zip splitNspSharpGui.Wpf/bin/dependent/netcoreapp3.1/win-x64/publish/splitNspSharpGui.Wpf.exe
	$(ZIP) -9 -j release_binaries/splitNspSharpGui/splitNspSharpGui-Win64-framework-dependent-$(GUI_VERSION).zip splitNspSharpGui.Wpf/bin/framework/net48/win-x64/publish/splitNspSharpGui.exe

delete_build_tools:
	$(RM) gui_converter$(BINARY_EXTENSION)
	$(RM) warp-packer$(BINARY_EXTENSION)
