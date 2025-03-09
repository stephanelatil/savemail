#!/bin/bash

export ASPNETCORE_URLS=http://${HOST}:${PORT}

dotnet Backend.dll