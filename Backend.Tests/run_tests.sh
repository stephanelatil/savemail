#!/bin/bash
set -e

# Configuration variables
CONTAINER_NAME=greenmail-test

# Ensure clean state
docker rm -f $CONTAINER_NAME 2>/dev/null || true

# Start Greenmail container with test configuration with only IMAP configured, the rest is not needed
docker run -d \
  --name $CONTAINER_NAME --rm \
  -p 3143:3143 \
  -e GREENMAIL_OPTS="-Dgreenmail.setup.test.imap -Dgreenmail.setup.test.smtp -Dgreenmail.users=test:password@localhost,test2:password2@localhost -Dgreenmail.users.login=email -Dgreenmail.preload.dir=/tmp/mails -Dgreenmail.hostname=0.0.0.0" \
  -v "$(pwd)/test_mail_dir:/tmp/mails:ro" \
  greenmail/standalone

# Wait for Greenmail to be ready
echo "Waiting for Greenmail to start..."
sleep 5

# Run the tests
dotnet test --nologo || true

# Cleanup
docker stop $CONTAINER_NAME