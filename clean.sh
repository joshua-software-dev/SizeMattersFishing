#!/bin/bash

SCRIPT=$(readlink -f "$0")
SCRIPTPATH=$(dirname "$SCRIPT")
cd "$SCRIPTPATH"

for i in **/*.csproj; do
    if [[ "$i" == "Version/Version.csproj" ]]; then
        continue
    fi

    rm -rf "$(dirname "$i")"/bin/
    rm -rf "$(dirname "$i")"/obj/
done

rm -rf deps/lib/*
