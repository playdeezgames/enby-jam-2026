#+build js
package gwen

import "base:runtime"

foreign import host "env"

@(default_calling_convention = "contextless")
foreign host {
	// Hands the grid to the page, which blits glyphs from the font sheet.
	present :: proc(cells: [^]Cell, cols, rows: i32) ---
	// Milliseconds since the epoch, used only to seed the RNG.
	now_ms :: proc() -> f64 ---
}

@(private = "file")
game: Game
@(private = "file")
grid: Grid
@(private = "file")
dirty: bool

main :: proc() {
	t := now_ms()
	game_init(&game, u64(t), t)
	dirty = true
}

// odin.js calls this every animation frame.
@(export)
step :: proc(dt: f64) -> bool {
	context = runtime.default_context()
	if dirty {
		game_draw(&game, &grid)
		present(&grid[0][0], COLS, ROWS)
		dirty = false
	}
	return true
}

// Keys: see Key in game.odin; printable characters pass their code.
@(export)
on_key :: proc "c" (key: i32) {
	context = runtime.default_context()
	game_key(&game, key)
	dirty = true
}
