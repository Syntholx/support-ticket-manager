#!/bin/sh
set -eu
# Only the public CA certificate is installed. Private CA and SQL keys never enter this container.
test -s /run/certificates/demo-ca.crt
cp /run/certificates/demo-ca.crt /usr/local/share/ca-certificates/tsm-demo-ca.crt
update-ca-certificates >/dev/null
exec setpriv --reuid=1654 --regid=1654 --clear-groups dotnet /app/SupportTicketManager.Api.dll "$@"
