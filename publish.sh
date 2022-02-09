#!/bin/bash

SCRIPT=$(readlink -f "$0")
SCRIPTPATH=$(dirname "$SCRIPT")
cd "$SCRIPTPATH"

dotnet publish -c Release
PLUGIN_VERSION=$(python -c "import xml.etree.ElementTree as ET;print(ET.parse('Version/Version.csproj').getroot()[0][0].text)")
echo "::set-output name=plugin_version::${PLUGIN_VERSION}"