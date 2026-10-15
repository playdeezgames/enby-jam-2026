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
