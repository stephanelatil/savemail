#!/bin/bash

export ASPNETCORE_URLS="http://+:${PORT}"
export ASPNETCORE_HTTP_PORTS="${PORT}"

exec dotnet Backend.dll