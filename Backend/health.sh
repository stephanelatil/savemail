#!/bin/bash

/usr/bin/curl http://${HOSTNAME}:${PORT}/healthz || exit 1