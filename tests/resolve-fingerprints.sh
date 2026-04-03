#!/bin/bash
# Resolve .NET 10 fingerprinted static asset references in index.html.
#
# .NET 10 publishes HTML with two patterns that require MapStaticAssets()
# middleware to work at runtime:
#   1. src="path#[.{fingerprint}].ext" — browser treats # as fragment → 404
#   2. <script type="importmap"></script> — empty, but JS modules need mappings
#
# Since Blazor WASM hosting uses UseStaticFiles(), we fix both at build time.

set -euo pipefail

PUBLISH_DIR="${1:?Usage: resolve-fingerprints.sh <publish-dir>}"
WWWROOT="$PUBLISH_DIR/wwwroot"
INDEX="$WWWROOT/index.html"

if [ ! -f "$INDEX" ]; then
    echo "No index.html found at $INDEX, skipping"
    exit 0
fi

echo "Resolving fingerprinted assets in $INDEX"

# --- Step 1: Rewrite #[.{fingerprint}] references ---

grep -oP '[^"'\'']+#\[\.{fingerprint}\]\.[a-z]+' "$INDEX" | while read -r pattern; do
    base="${pattern%%#*}"
    ext="${pattern##*.}"

    actual=$(find "$WWWROOT" -path "*/${base}.*${ext}" \
        -not -name "*.gz" -not -name "*.br" 2>/dev/null | head -1)

    if [ -n "$actual" ]; then
        relative="${actual#$WWWROOT/}"
        echo "  [rewrite] $pattern -> $relative"
        perl -pi -e "s|\Q${pattern}\E|${relative}|g" "$INDEX"
    else
        echo "  [warn] no match for $pattern"
    fi
done

# --- Step 2: Populate the import map ---

# Find fingerprinted JS modules in _framework/ that Blazor imports by bare name.
# Pattern: dotnet.{hash}.js, dotnet.native.{hash}.js, dotnet.runtime.{hash}.js
IMPORTS=""
for f in "$WWWROOT"/_framework/dotnet.*.js; do
    [ -f "$f" ] || continue
    # Skip compressed variants
    case "$f" in *.gz|*.br) continue ;; esac

    filename=$(basename "$f")

    # Derive the unfingered name by removing the hash segment
    # dotnet.h4ixstq87t.js -> dotnet.js
    # dotnet.native.ykrnppwhq2.js -> dotnet.native.js
    # dotnet.runtime.peu2mfb29t.js -> dotnet.runtime.js
    unfingerprinted=$(echo "$filename" | perl -pe 's/^(dotnet(?:\.native|\.runtime)?)\.[a-z0-9]+\.js$/$1.js/')

    if [ "$unfingerprinted" != "$filename" ]; then
        [ -n "$IMPORTS" ] && IMPORTS="$IMPORTS,"
        IMPORTS="$IMPORTS\"./_framework/$unfingerprinted\":\"./_framework/$filename\""
        echo "  [importmap] $unfingerprinted -> $filename"
    fi
done

if [ -n "$IMPORTS" ]; then
    IMPORTMAP="{\"imports\":{${IMPORTS}}}"
    perl -pi -e "s|<script type=\"importmap\"></script>|<script type=\"importmap\">${IMPORTMAP}</script>|" "$INDEX"
    echo "  [importmap] populated with $(echo "$IMPORTS" | tr ',' '\n' | wc -l) entries"
fi

echo "Done"
