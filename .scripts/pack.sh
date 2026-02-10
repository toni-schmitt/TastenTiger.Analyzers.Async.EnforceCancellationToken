#!/bin/bash

set +ex

dotnet tool install GitVersion.Tool

VERSION=$(dotnet gitversion -showvariable FullSemVer)

echo "Creating nuget package with version ${VERSION}"

dotnet clean

dotnet restore

dotnet build --configuration=Release --no-restore /p:Version="${VERSION}"

dotnet pack --configuration=Release \
  --no-build \
  --output ./.nupkg \
  /p:PackageVersion="${VERSION}" \
  /p:Version="${VERSION}"

set -ex