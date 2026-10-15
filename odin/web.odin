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
	// Saved game storage (localStorage). storage_get returns the byte length, or -1 for none.
	storage_get :: proc(buf: [^]u8, cap: i32) -> i32 ---
	storage_set :: proc(ptr: [^]u8, len: i32) ---
	storage_remove :: proc() ---
}

@(private = "file")
app: App
@(private = "file")
grid: Grid
@(private = "file")
dirty: bool
@(private = "file")
load_buf: [1 << 16]u8

main :: proc() {
	t := now_ms()
	app_init(&app, u64(t), t)
	n := storage_get(raw_data(load_buf[:]), len(load_buf))
	if n > 0 && int(n) <= len(load_buf) {
		app_resume(&app, string(load_buf[:n]))
	}
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
	if app.game.embarked {
		if text, ok := save_encode(&app.game); ok {
			storage_set(raw_data(text), i32(len(text)))
		}
	} else {
		storage_remove()
	}
	if app.open_link != .None {
		url := link_urls[app.open_link]
		open_url(raw_data(url), i32(len(url)))
		app.open_link = .None
	}
}
