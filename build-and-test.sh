#!/bin/sh -e

_SCRIPT_DIR="$( cd -P -- "$(dirname -- "$(command -v -- "$0")")" && pwd -P )"

# run code generator
GENERATOR_DIR=$_SCRIPT_DIR/src/IxMilia.Dwg.Generator
LIBRARY_DIR=$_SCRIPT_DIR/src/IxMilia.Dwg
cd "$GENERATOR_DIR"
dotnet restore
dotnet build
dotnet run "$LIBRARY_DIR"
cd -

# build and run tests
TEST_PROJECT=$_SCRIPT_DIR/src/IxMilia.Dwg.Test/IxMilia.Dwg.Test.csproj
dotnet restore "$TEST_PROJECT"
dotnet build "$TEST_PROJECT"
dotnet test "$TEST_PROJECT"
