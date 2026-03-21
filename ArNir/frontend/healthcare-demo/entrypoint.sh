#!/bin/sh
set -eu

API_URL="${API_BASE_URL:-https://genaiapi.empiricaledge.site/api}"
ESCAPED_API_URL=$(printf '%s\n' "$API_URL" | sed 's/[&|]/\\&/g')

sed -i "s|__API_URL__|$ESCAPED_API_URL|g" /usr/share/nginx/html/env-config.js

exec nginx -g "daemon off;"
