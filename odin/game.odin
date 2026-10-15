package gwen

import "core:fmt"

// Special key codes passed from the page; anything >= 32 is a printable character code.
Key :: enum i32 {
	None      = 0,
	Up        = 1,
	Down      = 2,
	Left      = 3,
	Right     = 4,
	Enter     = 5,
	Escape    = 6,
	Backspace = 7,
}

// Phase 2 developer harness: lets the rules be played from the keyboard before
// the real screens exist (phase 3). Replaced by the screen state machine.
App :: struct {
	game:   Game,
	now_ms: f64, // supplied by the platform, never read inside the rules
}

app_init :: proc(a: ^App, seed: u64, now_ms: f64) {
	a^ = {}
	rng_seed(&a.game.rng, seed)
	a.now_ms = now_ms
	embark(&a.game)
}

app_key :: proc(a: ^App, key: i32) {
	g := &a.game
	switch key {
	case 'c':
		perform(g, .Continue_Journey)
	case 'f':
		perform(g, .Forage)
	case 'e':
		perform(g, .Eat_Snax)
	case 'n':
		perform(g, .Take_Nap)
	case 's':
		perform(g, .Take_Shortcut)
	case 'x':
		status(g)
	case 'l':
		look_action(g)
	case 'r':
		embark(g)
	case 'd':
		// Drive/fish the first feature or character verb that is enabled, for poking at events.
		for i in 0 ..< g.feature_count {
			for v in feature_verbs[g.features[i].kind] {
				if verb_enabled(g, v, i) {
					perform(g, v, i)
					return
				}
			}
		}
		for i in 0 ..< g.character_count {
			for v in character_verbs[g.characters[i]] {
				if verb_enabled(g, v, i) {
					perform(g, v, i)
					return
				}
			}
		}
	}
}

app_draw :: proc(a: ^App, grid: ^Grid) {
	g := &a.game
	grid_clear(grid)
	grid_box(grid, 0, 0, COLS, ROWS, u8(Ink.Gray))
	grid_text(grid, 2, 1, "Gwen's Peregrination II", u8(Ink.Yellow))
	grid_text(grid, 2, 2, "dev harness: c continue  f forage  e eat  n nap  s shortcut", u8(Ink.Gray))
	grid_text(grid, 2, 3, "x status  l look  d first feature/person verb  r restart", u8(Ink.Gray))

	buf: [64]u8
	line := fmt.bprintf(
		buf[:],
		"HP %d  Sat %d  Stom %d  Fat %d  Snax %d  Jools %d  Miles %d",
		counter(g, .Health),
		counter(g, .Satiety),
		counter(g, .Stomach),
		counter(g, .Fatigue),
		counter(g, .Snax),
		counter(g, .Jools),
		counter(g, .Distance_Remaining),
	)
	grid_text(grid, 2, 5, line, u8(Ink.Cyan))

	// Newest messages at the bottom, wrapped by character for now (phase 3 wraps by word).
	width := COLS - 4
	row := ROWS - 2
	for i := g.message_count - 1; i >= 0 && row >= 7; i -= 1 {
		text := message_text(&g.messages[i])
		lines := (len(text) + width - 1) / width
		if lines == 0 {
			lines = 1
		}
		start := row - lines + 1
		for l in 0 ..< lines {
			lo := l * width
			hi := min(lo + width, len(text))
			if start + l >= 7 {
				ink := g.messages[i].link == .None ? u8(Ink.Default) : u8(Ink.White)
				grid_text(grid, 2, start + l, text[lo:hi], ink)
			}
		}
		row = start - 1
	}
}
