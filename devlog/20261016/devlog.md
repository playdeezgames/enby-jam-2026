# Gwen's Peregrination II learns to drive (and to save)

*Draft for October 16, 2026. Cover: `cover.png` in this folder.*

Gwen's Peregrination II is out in the browser again. It started life as a VB.NET entry for Nonbinary Game Jam 2026: a menu-driven walk of 2,000 miles with snax, flowers, a cat shrine and ad breaks. A few weeks on, it has been rebuilt in Odin and WebAssembly, and a few things came along for the ride.

## What changed

**It saves.** The jam build never did. Every choice is written to your browser, and when you come back you land right where you left off, mid-journey, with the message log intact.

**The screen is a text grid.** The old page was HTML buttons. Now it is a 53 by 30 grid of 6 by 8 pixel glyphs in the DINOBYTE font, with numbered choices, a cursor, word-wrapped text and taps. Hold a phone upright and it swaps to a taller 36 by 54 grid so the letters are big enough to read.

**You can finally drive.** The jam build had a car you could find and keys you could find, and no way to ever be allowed to use them. A driving-instruction counter had been added but never moved. That is fixed:

- find a **DMV** and pick up a **driver's handbook**,
- **study** it (it costs a little hunger and fatigue each time),
- take the **permit test**, which you pass if a 1d10 roll comes in at or below how much you studied,
- find a **driving instructor**, and pay 3 jools a lesson,
- at ten lessons yer permit becomes a **license**, and a car with keys will finally take you somewhere.

Drive is still a one-shot. The keys are used up, you go 5d6 miles, and the car runs out of fuel and stays behind. It is Gwen's Peregrination, not Gwen's Road Trip.

## What did not change

The numbers (hunger, fatigue, foraging, shortcuts, the weights of what you meet on the road), the wording, and the ads. Ad breaks are still two real minutes. Gämë Mënü still has its umlauts. The name and pronoun questions still correct you, and the only name that is right is Gwen.

## Small fixes

- Driving now shows you the new place when you arrive, like walking and shortcuts do.
- A car's spawn chance was a debug-only constant in the jam build; it is a real number now.
- Status only shows driving instruction once you have some.

## How it was made

The port was written with Claude Code: I read the old code, argued about the plan one question at a time, and tested it; it wrote the Odin, the 68 tests and the new DMV text. The rules are a plain game package with no browser in it, so the whole thing is tested natively (every event, the whole driving chain, saving and loading, and every screen), then wrapped in a thin page that blits the grid onto a canvas.

Play it in the browser on the game page. If you find something that smells like a bug rather than a metaphor, say so in the comments.

Sponsored by UMLAUT.FYI!
