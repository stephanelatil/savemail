#!/bin/bash

export ASPNETCORE_URLS=http://${HOSTNAME}:${PORT}

dotnet Backend.dll