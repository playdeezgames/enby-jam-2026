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
	// Opens a URL in a new tab; called only from inside a key or tap handler.
	open_url :: proc(ptr: [^]u8, len: i32) ---
}

@(private = "file")
app: App
@(private = "file")
grid: Grid
@(private = "file")
dirty: bool

main :: proc() {
	t := now_ms()
	app_init(&app, u64(t), t)
	dirty = true
}

// odin.js calls this every animation frame.
@(export)
step :: proc(dt: f64) -> bool {
	context = runtime.default_context()
	if dirty {
		app_draw(&app, &grid)
		present(&grid[0][0], COLS, ROWS)
		dirty = false
	}
	return true
}

// Keys: see Key in game.odin; printable characters pass their code.
@(export)
on_key :: proc "c" (key: i32) {
	context = runtime.default_context()
	app.now_ms = now_ms()
	app_key(&app, key)
	after_event()
}

// A click or tap on a grid cell.
@(export)
on_tap :: proc "c" (col, row: i32) {
	context = runtime.default_context()
	app.now_ms = now_ms()
	app_tap(&app, int(col), int(row))
	after_event()
}

@(export)
wants_text :: proc "c" () -> bool {
	context = runtime.default_context()
	return app_wants_text(&app)
}

@(private = "file")
after_event :: proc() {
	dirty = true
	if app.open_link != .None {
		url := link_urls[app.open_link]
		open_url(raw_data(url), i32(len(url)))
		app.open_link = .None
	}
}
