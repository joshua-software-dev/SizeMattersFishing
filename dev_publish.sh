#!/bin/bash

SCRIPT=$(readlink -f "$0")
SCRIPTPATH=$(dirname "$SCRIPT")
cd "$SCRIPTPATH"

./publish.sh && ssh windows "%APPDATA%\XIVLauncher\liveDevPlugins\size.bat"
