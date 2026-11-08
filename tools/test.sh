#!/usr/bin/env bash
# Native tests. One thread: the tests share globals and tracking allocators.
set -euo pipefail
cd "$(dirname "$0")/.."
"${ODIN:-/home/yermom/ODIN/odin}" test odin -define:ODIN_TEST_THREADS=1 -out:build/odin-test
