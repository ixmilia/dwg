IxMilia.Dwg
===========

A portable .NET library for reading and writing DWG files.

### Building Locally

This repo requires the following to build:

1. [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download).
2. [PowerShell 7](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell?view=powershell-7.3).

Once dependencies are met, run the following in a PowerShell 7 terminal:

```
.\build-and-test.ps1
```

Once that script has been run, you can use build from within Visual Studio.

### DWG reference

Reference for the closed DWG format come from the [Open Design Alliance](https://www.opendesign.com/).

[R13-R14](http://www.idea2ic.com/File_Formats/AutoCAD%20R13:R14%20DWG%20File%20Specification.pdf)

[Full specification (R13-R2018)](http://www.opendesign.com/files/guestdownloads/OpenDesign_Specification_for_.dwg_files.pdf).
