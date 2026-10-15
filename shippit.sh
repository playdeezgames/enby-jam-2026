#!/usr/bin/env bash
# Tests, builds the browser game and packages it for itch.io.
# Does NOT upload unless asked: ./shippit.sh --push
set -euo pipefail
cd "$(dirname "$0")"

./odin/test.sh
./odin/build.sh

rm -rf ./pub-html ./pub-html.zip
cp -r ./odin/build/web ./pub-html
(cd pub-html && zip -qr ../pub-html.zip .)
echo "packaged pub-html and pub-html.zip ($(du -sh pub-html | cut -f1))"

if [[ "${1:-}" == "--push" ]]; then
	butler push pub-html thegrumpygamedev/gwens-peregrination-ii:html
else
	echo "not pushed; run ./shippit.sh --push to upload"
fi
