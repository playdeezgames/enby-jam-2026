# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

"Gwen's Peregrination II" by TheGrumpyGameDev, made for Nonbinary Game Jam 2026: a turn-based, menu-driven walk of 2,000 miles (a "Metaphor" in the author's terms). The jam build was VB.NET (Spectre.Console and Blazor); it was rebuilt in **Odin compiled to `js_wasm32`** on October 15 to 16, 2026 and the VB `src/` was removed (it is in git history). Live on itch.io: https://thegrumpygamedev.itch.io/gwens-peregrination-ii (channel `html`).

## Related knowledge vault

The author's Obsidian vault at `/home/yermom/git/bok-of-splorr/splorr/` is the source of truth for context and rules: `Home.md` (standing rules), `Games/Gwen's Peregrination II.md` (this game's decisions, rules and history), `Tech/Release standard.md`, `Tech/GitHub Pages.md`, `Tech/LootLocker.md`, `Gotchas.md`.

## Commands

```bash
tools/test.sh            # 68 native tests (one thread: shared globals)
tools/build.sh [--size]  # build/web (game.wasm, odin.js, index.html, lootlocker.js, font)
tools/pages.sh           # rebuild docs/ for GitHub Pages (main, /docs); never commits or pushes
./shippit.sh             # wrapper for tools/ship.sh: tests, size build, zip; uploads ONLY with --push
```

Odin is at `/home/yermom/ODIN/odin` (override with `ODIN=`). Serve `build/web` with any static server to play.

## Layout

- `odin/rules.odin`: all rules on one flat `Game` struct; `rng.odin` splitmix64; `screens.odin` the menu state machine; `app.odin`, `screen.odin`, `text.odin` the glyph grid (53x30 landscape, 36x54 portrait) and word wrap; `save.odin` JSON saves (`gwen2:save`); `web.odin` (`#+build js`) and `odin/web/index.html` are the only platform code.
- `QUIRKS.md`: every quirk kept or fixed in the port. `PORT_PLAN.md`: the port plan and the user's decisions.
- `docs/` is the committed GitHub Pages build (made by `tools/pages.sh`), not notes.
- Release material follows the vault's Release standard: `ITCH_DESCRIPTION.md`, `assets/cover.png` (`tools/make_cover.py`), `assets/screenshots/`, `devlog/YYYYMMDD/`.

## Rules

- **Never `git push`, never run `./shippit.sh --push` or `butler push`** unless the user says so ("push" means git push; "ship" means itch.io). One itch.io release a day, booked through Coxswain.
- **Do not "fix" the metaphor**: the name and pronoun prompts that correct you, the ads, `Gämë Mënü` and the sponsor links are deliberate. Original typos (`exicitement`, `elixer`, `They a holding`) are kept; `Pkastic` is a joke. Read `QUIRKS.md` before changing anything odd.
- Keep the DINOBYTE credit (Credits screen, the page footer, the itch page, `NOTICE-dinobyte.txt`).
- The page footer carries the back links (itch.io, more games) and the LootLocker "Send feedback" link; the feedback link stays hidden until `LL_KEY` in `odin/web/index.html` is set.
