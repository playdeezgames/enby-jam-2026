# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

"Gwen's Peregrination II" by TheGrumpyGameDev, a game jam entry (title screen says Nonbinary Game Jam 2026; README says Knight Jam), written in VB.NET on .NET 10. It is a turn-based, menu-driven text game: drive a long journey, manage satiety/fatigue/health, buy snax, forage, give flowers, and so on. A "Metaphor" in the author's terms (see the vault below).

**Current goal: plan a port to Odin compiled to `js_wasm32`** (browser), as the author has done for many of their other games. The VB code here is the specification; there are no tests, so behavior must be read from the code.

## Related knowledge vault

The author's Obsidian vault at `/home/yermom/git/bok-of-splorr/splorr/` documents prior Odin/wasm ports. Start with `Home.md`, then:
- `Tech/Odin wasm recipe.md` — build command, frame loop, 2D canvas shim, DOM variants. The **"menu-driven text game as a view model (Shark Attackers)"** and **"tile picture (Feretory)"** variants are the closest fit for this game.
- `Gotchas.md` — WebGL doesn't work on the author's machine (use 2D canvas / plain DOM); wasm RNG seeding; keyboard `e.key` vs `e.code`.
- `Tech/Shipping to itch.io.md` and `Games/Shark Attackers of SPLORR!!.md`, `Games/Feretory of SPLORR!!.md` — prior ports of this same VB architecture (Spectre console + Blazor, TGGD/Metaphor layering). Those are the template for this port.
- Vault standing rules: don't `git push` or run publish scripts unless explicitly told; don't "fix" a game's deliberate design.

## Commands

Solution is `src/Metaphor.slnx`. No test projects exist.

```bash
dotnet build src/Metaphor.slnx
dotnet run --project src/Metaphor.Spectre/Metaphor.Spectre.vbproj     # console client
dotnet run --project src/Metaphor.Blazor/Metaphor.Blazor.csproj       # Blazor WASM client (dev server)
./shippit.sh                                                          # publish all targets + butler push to itch.io (do not run without explicit user request)
```

`Metaphor.Spectre` builds with `TreatWarningsAsErrors` and `OptionStrict On`.

## Architecture

Six layers, each split into a generic `TGGD.*` project (reusable framework) and a game-specific `Metaphor.*` project; lower layers never reference higher ones:

1. **Provision** — plain serializable data classes (`WorldData` holds dictionaries of `Guid -> LocationData/CharacterData/ItemData/FeatureData/InventoryData/VerbData`, plus messages, avatar id, ad timer). This is the save format (JSON).
2. **Persistence** — entity wrappers over the data (`IWorld`, `ILocation`, `ICharacter`, `IItem`, `IInventory`, `IFeature`, `IVerb`), with base `Verbable`/`Inventoried` entity mixins. Everything is Guid-keyed and string-typed: counters, tags, metadata and types are string constants. `IPersister` (`SaveAsync/LoadAsync(filename)`) is the only I/O abstraction.
3. **Processing** — game rules. `Model/*` wraps entities as view-facing models (`WorldModel`, `LocationModel`, `AvatarModel`, ...); `IVerbModel` exposes `IsEnabled/Name/Perform`. Verbs are the core mechanic: `Enum/VerbTypes.vb`, `Counters.vb`, `ItemTypes.vb`, `FeatureTypes.vb`, `Tags.vb` define the vocabulary; `Extensions/Verbs/*` hold `CanPerform`/`Perform` logic per verb on character/feature/item/location; `Initialize/` builds a new world from the chosen name and pronouns.
4. **Presentation** — screens as dialogs/menus: `Boilerplate` (title, main menu, game menu, abandon), `Embark` (name and pronouns prompts), `InPlay` (location, avatar/pace, items on ground/inventory/feature, characters, ads). Each screen emits display elements plus one prompt (choose / integer / double / string) via `TGGD.Presentation` dialog types. Navigation is closures: a screen's `Launch(...)` takes a "return to" function.
5. **Platform** — `MetaphorDisplay` (an `IDisplay`) starts at `Title.Launch`; exposes `Elements` and `Prompt` and a `Running` flag. `TGGD.Platform` has the display/element/prompt base types.
6. **Play** — thin clients that only render `display.Elements` and answer `display.Prompt`: `Metaphor.Spectre` (console, file persister) and `Metaphor.Blazor` (Razor page, `wwwroot/persister.js` over `localStorage`). Elements carry string `Hints` (e.g. `ELEMENT_TYPE=TITLE|LINK`, `URL`) that clients interpret.

Key implication for the port: the whole game is a state machine of "screen → elements + prompt → choice index → next screen" over a Guid-keyed entity store, with turn-based verbs and no real-time loop. The only time-dependent piece is the ad break (`AdFinishes`, a wall-clock countdown the player refreshes by clicking). Flattening Guid dictionaries to enum-indexed arrays/index-keyed pools, and string-typed counters/tags/types to Odin enums, is the natural translation (matches the author's earlier ports).

## Editing notes

- VB files carry a UTF-8 BOM; `src/.editorconfig` governs style.
- Verb types are string constants in `Metaphor.Processing/Enum/VerbTypes.vb`; their logic lives in `Extensions/Verbs/*`.
