#!/bin/sh

export LISTEN=${LISTEN:-0.0.0.0}

export NEXT_PUBLIC_FRONTEND_URL="http://${HOST}:${PORT}"
export NEXT_PUBLIC_BACKEND_URL="http://${HOST}:${BACKEND_PORT}"

npx next start -p ${PORT} -H ${LISTEN}