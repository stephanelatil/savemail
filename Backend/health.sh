#!/bin/bash

/usr/bin/curl http://${HOST}:${PORT}/healthz || exit 1