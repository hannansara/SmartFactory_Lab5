// =============================================================================
// SETUP-NOTIZEN (nur Kommentare, kein Code)
// Quelle: "Installing C# and Avalonia extensions under Visual Studio Code"
// Diese Datei kann das Projekt nicht stoeren: Sie enthaelt nur Kommentare.
// =============================================================================
//
//
// =============================================================================
// Installing C# and Avalonia extensions under Visual Studio Code
// =============================================================================
//
//
// -----------------------------------------------------------------------------
// 1. Install the .NET SDK
// -----------------------------------------------------------------------------
//
// Open Windows Terminal or PowerShell as Administrator and run:
//
//     winget install Microsoft.DotNet.SDK.10
//
// .NET 10 is the current long-term-support release. After installation, close
// and reopen both your terminal and VS Code.
//
// Verify it:
//
//     dotnet --version
//     dotnet --list-sdks
//
// The first command should report a version beginning with 10.
// The SDK contains the compiler, runtime, project templates, NuGet tooling,
// and dotnet command-line interface.
// (Microsoft's Windows installation guide)
//
//
// -----------------------------------------------------------------------------
// Install the C# extensions
// -----------------------------------------------------------------------------
//
// In VS Code:
//   1. Press Ctrl+Shift+X.
//   2. Search for C# Dev Kit.
//   3. Install the extension published by Microsoft.
//
// C# Dev Kit automatically installs the underlying Microsoft C# extension.
// It adds IntelliSense, debugging, testing, project management, and
// Solution Explorer. You may be prompted to sign in with a Microsoft account.
// (C# in VS Code documentation)
//
// Alternatively, install it from PowerShell:
//
//     code --install-extension ms-dotnettools.csdevkit
//
// If code is not recognized, install the extension through the VS Code
// interface instead.
//
//
// -----------------------------------------------------------------------------
// Test the basic C# installation
// -----------------------------------------------------------------------------
//
// Create a working folder:
//
//     mkdir C:\Projects
//     cd C:\Projects
//     dotnet new console -n HelloCSharp
//     cd HelloCSharp
//     code .
//
// Open Program.cs, then run:
//
//     dotnet run
//
// You should see: Hello, World!
//
// You can also press F5 in VS Code to start debugging.
// If prompted, select C# or C# Dev Kit.
//
//
// -----------------------------------------------------------------------------
// 2. Install Avalonia - recommended for VS Code
// -----------------------------------------------------------------------------
//
// Install the official extension:
//
//     code --install-extension AvaloniaTeam.vscode-avalonia
//
// Install the project templates and create an application:
//
//     cd C:\Projects
//     dotnet new install Avalonia.Templates
//     dotnet new avalonia.mvvm -o MyAvaloniaApp
//     cd MyAvaloniaApp
//     code .
//     dotnet run
//
// The UI files use the .axaml extension, which is Avalonia's XAML dialect.
// With the official extension installed, you can open an .axaml file and
// launch the XAML previewer.
// (Avalonia getting started and XAML preview documentation)
//
//
// -----------------------------------------------------------------------------
// 3. Install mcdg to generate UML Diagrams
// -----------------------------------------------------------------------------
//
// Open the project in VS Code
//
// Open the folder containing your .csproj or .sln file:
//
//     cd C:\Projects\MyAvaloniaApp
//     code .
//
// Then open VS Code's terminal with Ctrl+` (Backtick).
//
// 2. Install the generator locally
//
// A local tool is preferable because its version is recorded with the project:
//
//     dotnet new tool-manifest
//     dotnet tool install mcdg
//
//
// -----------------------------------------------------------------------------
// 4. Install previewer in VS Code
// -----------------------------------------------------------------------------
//
//     code --install-extension bierner.markdown-mermaid
//
//
// =============================================================================
// ZUSATZ (nicht aus dem PDF): Hinweise fuer macOS
// =============================================================================
//
// - Das PDF ist fuer Windows geschrieben (winget, PowerShell, C:\Projects).
//   Auf dem Mac gilt stattdessen:
//
//     .NET SDK:      brew install --cask dotnet-sdk
//                    (oder Download von dotnet.microsoft.com)
//     Ordner:        mkdir ~/Projects ; cd ~/Projects
//     "code" Befehl: In VS Code Cmd+Shift+P, dann
//                    "Shell Command: Install 'code' command in PATH"
//     Terminal:      Ctrl+` (Backtick)
//
// - Die dotnet-Befehle (dotnet --version, dotnet build, dotnet run, dotnet new,
//   dotnet tool install) sind auf Windows und Mac gleich.
//
// - Pruefungsmodus ohne Internet: Installieren und NuGet-Pakete holen geht dann
//   nicht. Zu Hause mit ausgeschaltetem WLAN "dotnet build" testen.
//
// =============================================================================