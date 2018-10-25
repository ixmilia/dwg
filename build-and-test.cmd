@echo off

:: run code generator
set GENERATOR_DIR=%~dp0src\IxMilia.Dwg.generator
set LIBRARY_DIR=%~dp0src\IxMilia.Dwg
pushd "%GENERATOR_DIR%"
dotnet restore
if errorlevel 1 goto error
dotnet build
if errorlevel 1 goto error
dotnet run "%LIBRARY_DIR%"
if errorlevel 1 goto error
popd

:: build and run tests
set TEST_PROJECT=.\src\IxMilia.Dwg.Test\IxMilia.Dwg.Test.csproj
dotnet restore %TEST_PROJECT%
if errorlevel 1 exit /b 1
dotnet test %TEST_PROJECT%
if errorlevel 1 exit /b 1
goto :eof

error:
echo Error building project.
exit /b 1
