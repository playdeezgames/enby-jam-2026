# Gwen's Peregrination II

A Production of TheGrumpyGameDev, made for **Nonbinary Game Jam 2026**.

A turn-based, menu-driven metaphor: you are Gwen, and there are 2,000 miles to walk.

- Play on itch.io: https://thegrumpygamedev.itch.io/gwens-peregrination-ii
- Play on GitHub Pages: https://playdeezgames.github.io/enby-jam-2026/ (once enabled)

The jam build (July 2026) was VB.NET with Spectre.Console and Blazor; it is in git history. The current game is a rebuild in [Odin](https://odin-lang.org/) compiled to WebAssembly (October 2026), written by an AI agent (Claude Code) with the author as product owner and tester.

## Building

```bash
tools/test.sh      # native tests
tools/build.sh     # build/web (add --size for the shipping build)
tools/pages.sh     # rebuild docs/ for GitHub Pages
./shippit.sh       # tests, builds and zips for itch.io; uploads only with --push
```

Serve `build/web` with any static server (`python3 -m http.server -d build/web`).

## Credits

- Written by TheGrumpyGameDev.
- Font: DINOBYTE by mby (https://mby.itch.io/dinobyte), CC BY 4.0, used unmodified (`assets/dinobyte/`).
- Sponsored by UMLAUT.FYI!
