#!/usr/bin/env bash
# Builds the browser game into build/web. Pass --size for the shipping build.
set -euo pipefail
cd "$(dirname "$0")/.."
ODIN="${ODIN:-/home/yermom/ODIN/odin}"
opt=speed
[[ "${1:-}" == "--size" ]] && opt=size
rm -rf build/web
mkdir -p build/web
"$ODIN" build odin -target:js_wasm32 -out:build/web/game.wasm -o:$opt
cp "$("$ODIN" root)/core/sys/wasm/js/odin.js" build/web/
cp odin/web/index.html odin/web/lootlocker.js build/web/
cp assets/dinobyte/dbyte_1x.png build/web/
cp assets/dinobyte/dbyte-LICENSE.txt build/web/NOTICE-dinobyte.txt
echo "built build/web ($opt)"
