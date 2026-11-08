# Port plan: Gwen's Peregrination II to Odin / wasm

Source: this repo's VB.NET game (`src/`, ~5,200 lines, six layers). Target: Odin compiled to `js_wasm32`, browser only, following the author's vault (`/home/yermom/git/bok-of-splorr/splorr/`): `Tech/Odin wasm recipe.md` (variant "menu-driven text game as a view model") and the Shark Attackers port (`Games/Shark Attackers of SPLORR!!.md`), which ported the same TGGD/Metaphor framework.

## Recommended shape

Text-mode grid (see decision 7), fixing quirks as we go, browser only, pure rules testable natively. Proposed layout (`odin/` beside `src/` during the port, then `src/` deleted in its own commit, as Shark Attackers did):

| File | Role |
| --- | --- |
| `odin/game.odin` | Pure rules on one `Game` struct. Clock and RNG are arguments or a seedable generator held in the struct, never read from the platform. |
| `odin/rng.odin` | Port of `RNG`: `roll_dice`, weighted pick, range. Seeded from the page's entropy. |
| `odin/screens.odin` | Menu state machine over a `Screen` enum; `current_choices`, `choose(i)`, `submit_*`, `make_view`. |
| `odin/save.odin` | JSON save/load with range validation (see decisions). |
| `odin/web.odin` (`#+build js`) | `dom_*` imports, storage, exports `on_choice`, `on_submit_number`, `on_submit_text`. |
| `odin/web/index.html` | Canvas page: blits the glyph grid from the DINOBYTE sheet, scales crisply, routes keys/taps/links back to Odin. |
| `odin/game_test.odin` (`#+build !js`) | Native tests: `odin test odin -define:ODIN_TEST_THREADS=1`. |
| `shippit.sh` | Replace the dotnet publishes with test, build, zip; butler push to the `html` channel only. |

## What the VB game actually is (verified from source)

**Entity model collapses.** The Guid-keyed dictionaries (`Locations`, `Characters`, `Inventories`, `Items`, `Features`, `Verbs`) are generic machinery for a game that has **one location, one avatar, at most one Traehi, a handful of features and a few items**. In Odin this becomes a flat struct: avatar counters, a fixed `Location` (tags, foraging difficulty, `feature` slots, `ground` item list, optional Traehi flag), and avatar inventory. No Guids, no string-typed counters: enums for counters, items, features, verbs.

**Turn loop.** All turn actions go through `Location.Update` (regenerate foraging difficulty, kill non-avatar characters, remove all features, delete all ground items (each LITTER bumps `LITTERING`), clear `SHORTCUT` tag, roll one event). `Update` runs after Continue Journey, Take Shortcut and Drive.

**Avatar counters** (min/max/start): HEALTH 0–100/100, SATIETY 0–100/100, STOMACH 0–50/0, BEWWY_HERTZ 0–∞/0, LITTERING, RECYCLING 0–∞/0, DRIVING_INSTRUCTION 0–10/0, JOOLS 0–∞/0, FATIGUE 0–100/0, SNAX 0–∞/10, DISTANCE_REMAINING 0–∞/2000, PACE 1–5/3, FORAGE_SKILL 0–∞/0, FORAGE_EXPERIENCE 0–10/0 (max doubles on level-up).

**Avatar verbs:** Continue Journey, Complete Journey (only at distance 0), Change Pace, Eat Snax, Take Nap (fatigue ≥ 50), Forage; location verb Take Shortcut (only with SHORTCUT tag). Plus Status, Ground, Inventory, Characters, Features, Look, Watch Ad, Gämë Mënü.

**Core formulas** (port these exactly):
- Walk: effective pace = pace, or `(pace + 1) \ 2` while BEWWY_HERTZ > 0 (which then ticks down 1); distance −= effective pace; hunger and fatigue by *raw* pace.
- Hunger: stomach absorbs first; then satiety; if satiety was 0 and stomach > 0, +1 satiety; remainder is health damage. Fatigue over max becomes damage. Dead = HEALTH 0.
- Eat snax: −1 snax, stomach +10; at max stomach, BEWWY_HERTZ += 2d6.
- Forage: roll 1d20 + skill vs. location difficulty (2d10 per location update); success +1 snax and difficulty becomes roll+1; always +1 hunger, +1 fatigue, +1 xp; xp max → skill +1, xp max doubled.
- Nap: hours = fatigue \ 10, apply that much hunger, fatigue 0.
- Shortcut gain = `2d6 - 2d6 - LITTERING + RECYCLING` added to distance (can add miles).
- Drive (needs CAR_KEYS item and LEARNERS_PERMIT item): consumes keys, 5d6 miles off, +1 hunger and fatigue, Update.
- Event table weights: shortcut 100, flower patch 100, Traehi 50, vending machine 50, abandoned house 10, cat shrine 10, Kwik Trip 10, recycle bin 10, dishes 100, car keys 1, car (`TEST_WEIGHT`), nothing 1000.
- Features: flower patch (3d4 flowers, picking yields Flower items; Traehi gives 1 jool per flower), vending machine (1 jool: 10% nothing, 80% one snax, 10% two), Kwik Trip (1 jool: Dr Pepper → Drink removes all fatigue, creates Litter), recycle bin (litter → RECYCLING), dishes (2d6 jools, +5 fatigue, then vanishes), cat shrine (Pray → Hairball on the ground; cat name from Captain Jack / Nina / Körmy), abandoned house (Destroyed Printer + Pkastic Bag with 2d6 snax via Reach In; suppressed once either item is held), car.
- Ads: Watch Ad = 2 real minutes, refreshed by Ok, ends with a Coupon item. The sponsor link is Umlaut.fyi. Stop and Smell on a flower shows a Steam link for another game.

**Save format.** The VB game **never actually saves**: `World.Save` exists but nothing calls it; load runs at start and silently falls back to a fresh world. So the port has the same "no saving" baseline; adding saves is a decision (below).

**Screens (to mirror, in priority order from `InPlay.Run`):** ad → done (journey complete or dead; Ok → abandon to Main Menu) → change-pace prompt → navigation menu. Plus Title → Main Menu (Embark!, Quit only if quittable → hidden in browser) → name prompt → pronouns prompt (validated) → InPlay. Game menu: Continue, Abandon (confirm). Ground/Inventory use item stacks (Take/Drop One, Half when count > 3, All when count > 1, per the VB conditions) and per-item menus with verbs; features have their own inventories (abandoned house).

## Phases

1. **Scaffold.** `odin/` layout, `build.sh`, empty page loading the wasm, `step` stub, `dom_*` imports, seeded RNG. Check the Odin nightly path from the recipe.
2. **Pure rules + tests.** Port `game.odin` and `rng.odin`: counters, hunger/fatigue, verbs, events, update. Tests drive the same sequences a click would: forced RNG via a roll queue.
3. **Screens.** State machine: title, main menu, embark prompts, in-play navigation, stack menus, feature/character menus, ad, done. Unavailable choices are **hidden**: `DialogPrompt` filters choices on `Enabled`.
4. **Web shell.** Canvas glyph blit, scaling, key/tap routing, in-grid prompts, links, mobile layout.
5. **Saves (if chosen), quirks log, ship.** `docs/QUIRKS.md`; `shippit.sh` rewrite; update vault note.

## Decisions for the author (ask one at a time)

1. **Fidelity: DECIDED — fix quirks as we go.** Not an exact port; obvious quirks are fixed during the port and noted in `docs/QUIRKS.md`. Deliberate design (message voice, difficulty) is still left alone.
2. **Saves: DECIDED — add saving and resume straight into play** (skip Title and Main Menu when a save exists). JSON in `localStorage`, every field range-checked on load, failed load leaves a fresh game; Abandon clears the save.
3. **Native builds: DECIDED — browser only.** Spectre console and the Win/Linux/Mac downloads are retired; `shippit.sh` pushes the `html` channel only.
4. **Driving: DECIDED — finish it.** Earn the learner's permit through the `DRIVING_INSTRUCTION` counter (0–10), and give the car a sensible spawn weight. **Author's design so far:** with a learner's permit, the player finds an **instructor** (a character event, like Traehi) who instructs them for a **fee** (jools), raising `DRIVING_INSTRUCTION`. **Permit:** at a **DMV** (new feature event) the player can pick up a **driver's handbook** (item) and take the **learner's permit test**; success depends on how well they have studied the handbook. **Study: DECIDED** — "Study Handbook" verb on the item adds 1 to a `STUDY` counter (0–10) and costs 1 hunger and 1 fatigue; the test passes if `1d10 <= study`, retry allowed. **Test fee: DECIDED** — free. **Instructor: DECIDED** — random location event (character, like Traehi), requires the permit; "Take Lesson" costs 3 jools and adds 1 `DRIVING_INSTRUCTION`. **At 10: DECIDED** — the permit is replaced by a Driver's License item; Drive requires car keys + license (this replaces the `LEARNERS_PERMIT` check on Drive; permit is still needed to take lessons). **Car weight: DECIDED** — 10. **Event weights: DECIDED** — DMV 10, instructor 20. **Drive: DECIDED** — stays one-shot (keys consumed, 5d6 miles). Driving is fully decided; the rest of this item is the original notes:  the original notes below. (Was: the instructor event, the fee, instruction gained per lesson, what happens at 10, the car's spawn weight, and whether Drive stays one-shot (keys consumed, 5d6 miles). Original notes follow.
   **`TEST_WEIGHT`:** `SpawnCar` uses a constant that is only defined under `#If DEBUG` (250, i.e. a car on ~15% of turns). In Release it presumably doesn't compile, or the shipped build differed. What should the car's real weight be? Also nothing grants `LEARNERS_PERMIT` and `DRIVING_INSTRUCTION` is displayed but never changed (last commit "driving instruction counter" is unfinished), so Drive is currently unreachable in practice. Finish it, keep it as-is, or cut it?
5. **Debug leftovers: DECIDED** — dropped; release behavior only (0 jools, no Macguffin).
6. **Ads: DECIDED** — keep the 2-minute ad break, Umlaut.fyi sponsor link, Coupon reward, and the Stop and Smell Steam link.
7. **Presentation: DECIDED — text-mode grid, not DOM.** Odin keeps a `[ROWS][COLS]u8` glyph grid (plus attributes if colour is wanted) and one foreign `present(cells)` call hands it to JS, which blits glyphs from the **DINOBYTE 6x8** sheet (`Assets/dbyte_1x.png`/`dbyte_2x.png` in the vault; CC BY 4.0, credit on the title/credits and itch page; licence file shipped) onto a 2D canvas with crisp scaling (the Bus Anticipator variant; no WebGL). Consequences for the plan: `screens.odin` renders into the grid with word-wrapping and a scrolling message log; choices are numbered/lettered text rows that are keyboard- and tap-selectable (record row to choice mapping while drawing; `touch-action: none`); number and name prompts are drawn in-grid with keyboard input and a hidden input element on touch devices; links (TheGrumpyGameDev, jam, Umlaut.fyi, Steam promo) need a clickable-row mechanism that opens a new tab. **Grid: 53x30** cells of 6x8 (318x240 px native, scaled to the window). Replaces the `index.html` DOM replay and `dom_*` imports in the layout table above.
8. **Vault bookkeeping: DECIDED — at ship time.** add `Games/Gwen's Peregrination II.md` and a row in `Steam pivot evaluation`, and link from `Home.md`.

## Risks

- **Verification without tests.** The VB code has none; ground truth is reading it. Prefer porting function by function with the VB line next to it, and play the original (`dotnet run --project src/Metaphor.Spectre`) for spot checks of message text.
- **Message text is the game.** Most of the content is strings; port them verbatim (including `Körmy`, `PÖÖF!`, `Pkastic`, `bewwy hertz`, `Gämë Mënü`). Don't "correct" them.
- **Quirks to preserve or log:** every counter write clamps to its min/max (`Entity.SetCounter`), which the formulas rely on; the "left to go" message after Drive lacks the unit; `Snax(1 jools)`; `ApplyFatigue` reports `capacity` but adds the full amount (clamped).
- **Time-based ad** needs the clock passed in; tests must control it.
