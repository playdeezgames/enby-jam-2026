# Quirks of the VB original, and what the port did

Decided policy: **fix quirks as we go**; deliberate design and the message voice are left alone.

## Fixed in the port

| Quirk in the VB game | Port |
| --- | --- |
| Drive ended without a Look, so the new place (and its event) was not shown, unlike Continue Journey and Take Shortcut | Drive ends with Look |
| Drive's "left to go" line had no unit | "N miles left to go." |
| Car spawn weight `TEST_WEIGHT` existed only in DEBUG builds; Release would not compile | Car weight 10 |
| `LEARNERS_PERMIT` was checked by Drive but nothing ever granted it; `DRIVING_INSTRUCTION` was shown but never changed | Full chain: DMV handbook, study, permit test, instructor lessons, license (see PORT_PLAN decision 4) |
| Status showed "Driving Instruction: 0%" from turn one (shown whenever below maximum) | Shown only once above 0, as "n/10" |
| DEBUG start gave 10 jools and a Macguffin | Removed |
| Save code existed but was never called | The port saves (phase 5) |
| Verbs were enabled by type only, so a verb could be run against the wrong target | Verbs check that the target feature, person or item is the right kind |

## Kept on purpose

- Name is always `Gwen` and pronouns always `They/Them`; the name and pronoun prompts are a gag that corrects you.
- Cat Shrine: every prayer drops a Hairball.
- Moving on removes every feature, person and ground item, and each Litter left behind raises LITTERING (which makes shortcuts worse).
- Hunger and fatigue on a walk use the raw pace, not the slowed pace from bewwy hertz.
- Abandoned House cannot appear while you carry its Destroyed Printer or Pkastic Bag.
- Typos in the text (`exicitement`, `elixer`, `They a holding`, `Pkastic`) are **kept verbatim** for now. `Pkastic` is a joke; the rest are for the author to decide.

## Open

- The new texts (handbook, permit, license, DMV, instructor, test messages) are mine and should be read by the author.
- Lessons cost money only (no hunger or fatigue); say if that should change.

## Screens (phase 3)

- Unavailable choices are hidden, as in the original.
- Choices are numbered 1-9 then a-z; Up/Down + Enter, a tap or click, or the hotkey all work. Escape picks "Never Mind" or "No" when it is the first choice. PageUp/PageDown and the mouse wheel scroll long message logs.
- Links inside messages (the ad's sponsor, Stop and Smell) also appear as a "Visit: ..." choice so they work from the keyboard.
- The Title screen's three links are choices under OK.
- Item stacks are labelled `Name xN`; the original's exact stack label was not checked.
- Quit is omitted in the browser, as the original hid it when not quittable.

## Saving and shipping (phase 5)

- The game saves after every action to `localStorage` key `gwen2:save` (JSON, version 1, 24 most recent messages kept) and resumes straight into play on load. Abandon, or finishing and dismissing the end screen, removes the save. A corrupt or out-of-range save is ignored and the game starts at the title.
- The ad break survives a reload because its end time is an absolute clock time.
- `./shippit.sh` (a wrapper for `tools/ship.sh`, since November 8, 2026) runs the tests, builds `build/web` and zips it; it uploads only with `--push`.
- A Credits screen (Main Menu) carries the DINOBYTE CC BY 4.0 credit; the font licence ships as `NOTICE-dinobyte.txt`.

## Layouts

- Wide windows use a 53 by 30 grid; a window taller than it is wide (a phone held upright) switches to 36 by 54, so the same 6 by 8 glyphs draw about 1.5 times larger. The page picks the layout and switches live on resize; saves are unaffected.
- Not verified on a real phone: only a 375 by 812 emulated viewport, with real touch and the soft keyboard still untested.
