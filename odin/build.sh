#!/usr/bin/env bash
# Builds the browser game into odin/build/web (the folder that gets shipped).
set -euo pipefail
cd "$(dirname "$0")"
ODIN="${ODIN:-/home/yermom/ODIN/odin}"
OUT=build/web
rm -rf "$OUT"
mkdir -p "$OUT"
"$ODIN" build . -target:js_wasm32 -out:"$OUT/game.wasm" -o:speed
cp "$("$ODIN" root)/core/sys/wasm/js/odin.js" "$OUT/"
cp web/index.html "$OUT/"
cp ../assets/dinobyte/dbyte_1x.png "$OUT/"
cp ../assets/dinobyte/dbyte-LICENSE.txt "$OUT/NOTICE-dinobyte.txt"
echo "built $OUT"
