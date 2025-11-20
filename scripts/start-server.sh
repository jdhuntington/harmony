#!/bin/bash

# Start the Harmony.Api server
# This script builds and runs the API server on port 5199

set -e

cd "$(dirname "$0")/.."

echo "Building Harmony.Api..."
dotnet build Harmony.Api/Harmony.Api.csproj

echo "Starting Harmony.Api on http://localhost:5199..."
echo "Press Ctrl+C to stop the server"
echo ""

cd Harmony.Api
ASPNETCORE_URLS="http://localhost:5199" dotnet run --no-build
