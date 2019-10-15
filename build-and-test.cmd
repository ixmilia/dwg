@echo off
setlocal

set thisdir=%~dp0
set configuration=Debug
set runtests=true

:parseargs
if "%1" == "" goto argsdone
if /i "%1" == "-c" (
    set configuration=%2
    shift
    shift
    goto parseargs
)
if /i "%1" == "-notest" (
    set runtests=false
    shift
    goto parseargs
)

echo Unsupported argument: %1
goto error

:argsdone

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

:: build
set SOLUTION=%~dp0src\IxMilia.Dwg.sln
dotnet restore %SOLUTION%
if errorlevel 1 exit /b 1
dotnet build %SOLUTION% -c %configuration%
if errorlevel 1 exit /b 1

:: test
if /i "%runtests%" == "true" (
    dotnet test "%SOLUTION%" -c %configuration% --no-restore --no-build
    if errorlevel 1 goto error
)
