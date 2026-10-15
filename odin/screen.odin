package gwen

COLS :: 53
ROWS :: 30

// One grid cell: a code page 437 glyph and an attribute byte.
// Attribute: low nibble is the palette index of the ink, bit 7 inverts (ink on a filled cell).
Cell :: struct {
	ch:   u8,
	attr: u8,
}

Grid :: [ROWS][COLS]Cell

ATTR_INVERSE :: u8(0x80)

// Palette indices; the page owns the actual colours.
Ink :: enum u8 {
	Default = 0,
	White   = 1,
	Yellow  = 2,
	Green   = 3,
	Red     = 4,
	Gray    = 5,
	Cyan    = 6,
	Magenta = 7,
}

grid_clear :: proc(g: ^Grid) {
	g^ = {}
}

grid_put :: proc(g: ^Grid, col, row: int, ch: u8, attr: u8 = 0) {
	if col < 0 || col >= COLS || row < 0 || row >= ROWS {
		return
	}
	g[row][col] = Cell{ch, attr}
}

// Draws UTF-8 text, converting to code page 437. Returns the column after the last glyph.
grid_text :: proc(g: ^Grid, col, row: int, text: string, attr: u8 = 0) -> int {
	buf: [MESSAGE_CAP]u8
	n := cp437_encode(buf[:], text)
	c := col
	for i in 0 ..< n {
		grid_put(g, c, row, buf[i], attr)
		c += 1
	}
	return c
}

grid_bytes :: proc(g: ^Grid, col, row: int, text: []u8, attr: u8 = 0) {
	for b, i in text {
		grid_put(g, col + i, row, b, attr)
	}
}

grid_text_centered :: proc(g: ^Grid, row: int, text: string, attr: u8 = 0) {
	buf: [MESSAGE_CAP]u8
	n := cp437_encode(buf[:], text)
	grid_bytes(g, (COLS - n) / 2, row, buf[:n], attr)
}

// Single-line box using CP437 box drawing.
grid_box :: proc(g: ^Grid, col, row, w, h: int, attr: u8 = 0) {
	if w < 2 || h < 2 {
		return
	}
	for x in 1 ..< w - 1 {
		grid_put(g, col + x, row, 0xC4, attr)
		grid_put(g, col + x, row + h - 1, 0xC4, attr)
	}
	for y in 1 ..< h - 1 {
		grid_put(g, col, row + y, 0xB3, attr)
		grid_put(g, col + w - 1, row + y, 0xB3, attr)
	}
	grid_put(g, col, row, 0xDA, attr)
	grid_put(g, col + w - 1, row, 0xBF, attr)
	grid_put(g, col, row + h - 1, 0xC0, attr)
	grid_put(g, col + w - 1, row + h - 1, 0xD9, attr)
}
