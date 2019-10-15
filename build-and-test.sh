#!/bin/sh -e

_SCRIPT_DIR="$( cd -P -- "$(dirname -- "$(command -v -- "$0")")" && pwd -P )"

CONFIGURATION=Debug
RUNTESTS=true

while [ $# -gt 0 ]; do
  case "$1" in
    --configuration|-c)
      CONFIGURATION=$2
      shift
      ;;
    --notest)
      RUNTESTS=false
      ;;
    *)
      echo "Invalid argument: $1"
      exit 1
      ;;
  esac
  shift
done

# run code generator
GENERATOR_DIR=$_SCRIPT_DIR/src/IxMilia.Dwg.Generator
LIBRARY_DIR=$_SCRIPT_DIR/src/IxMilia.Dwg
cd "$GENERATOR_DIR"
dotnet restore
dotnet build
dotnet run "$LIBRARY_DIR"
cd -

# build
SOLUTION=$_SCRIPT_DIR/src/IxMilia.Dwg.sln
dotnet restore $SOLUTION
dotnet build $SOLUTION -c $CONFIGURATION

# test
if [ "$RUNTESTS" = "true" ]; then
    dotnet test $SOLUTION -c $CONFIGURATION --no-restore --no-build
fi
