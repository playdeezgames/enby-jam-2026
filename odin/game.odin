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

// Phase 1: scaffold only. The rules (counters, verbs, events) arrive in phase 2.
Game :: struct {
	rng:      Rng,
	now_ms:   f64, // supplied by the platform, never read inside the rules
	last_key: i32,
	presses:  int,
	last_roll: int,
}

game_init :: proc(g: ^Game, seed: u64, now_ms: f64) {
	g^ = {}
	rng_seed(&g.rng, seed)
	g.now_ms = now_ms
}

game_key :: proc(g: ^Game, key: i32) {
	g.last_key = key
	g.presses += 1
	g.last_roll = roll_dice(&g.rng, 2, 6)
}

game_draw :: proc(g: ^Game, grid: ^Grid) {
	grid_clear(grid)
	grid_box(grid, 0, 0, COLS, ROWS, u8(Ink.Gray))
	grid_text_centered(grid, 3, "Gwen's Peregrination II", u8(Ink.Yellow))
	grid_text_centered(grid, 5, "A Production of TheGrumpyGameDev", u8(Ink.Default))
	grid_text_centered(grid, 7, "Phase 1: the scaffold runs.", u8(Ink.White))

	buf: [64]u8
	line := fmt.bprintf(buf[:], "Key presses: %d   last key code: %d", g.presses, g.last_key)
	grid_text_centered(grid, 11, line, u8(Ink.Cyan))

	grid_text_centered(grid, 14, "2d6 rolled on each key press:", u8(Ink.Default))
	line = fmt.bprintf(buf[:], "%d", g.last_roll)
	grid_text_centered(grid, 15, line, u8(Ink.Green))

	grid_text_centered(grid, 27, "Font: DINOBYTE by mby, CC BY 4.0", u8(Ink.Gray))
}
