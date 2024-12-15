#!/bin/bash
set -e

# Configuration variables
GREENMAIL_PORT=3143
CONTAINER_NAME=greenmail-test

# Ensure clean state
docker rm -f $CONTAINER_NAME 2>/dev/null || true

# Start Greenmail container with test configuration with only IMAP configured, the rest is not needed
docker run -d \
  --name $CONTAINER_NAME --rm \
  -p $GREENMAIL_PORT:3143 \
  -e GREENMAIL_OPTS="-Dgreenmail.setup.test.imap -Dgreenmail.users=test:password@localhost,test2:password2@localhost -Dgreenmail.preload.dir=/tmp/mails" \
  -e GREENMAIL_AUTH_MODE=plain \
  -v "$(pwd)/test_mail_dir:/tmp/mails:ro" \
  greenmail/standalone

# Wait for Greenmail to be ready
echo "Waiting for Greenmail to start..."
sleep 5

# Run the tests
dotnet test --nologo
docker stop $CONTAINER_NAME

# Cleanup
docker rm -f $CONTAINER_NAME