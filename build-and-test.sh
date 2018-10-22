#!/bin/sh -e

TEST_PROJECT=./src/IxMilia.Dwg.Test/IxMilia.Dwg.Test.csproj
dotnet restore $TEST_PROJECT
dotnet test $TEST_PROJECT
