package gwen

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
	Page_Up   = 8,
	Page_Down = 9,
}

// Everything the platform owns: the game, the screen state, and the two things the
// rules never touch themselves (the clock and opening links).
App :: struct {
	game:      Game,
	ui:        Ui,
	cols:      int, // grid size in use (a layout from screen.odin)
	rows:      int,
	now_ms:    f64, // set by the platform before each event
	open_link: Link, // set by a screen; the platform opens it and clears it
}

app_init :: proc(a: ^App, seed: u64, now_ms: f64) {
	a^ = {}
	a.cols = LANDSCAPE_COLS
	a.rows = LANDSCAPE_ROWS
	rng_seed(&a.game.rng, seed)
	a.now_ms = now_ms
	ui_enter(a, .Title)
}

// Restores a saved game. On success the player lands straight in play; on failure the
// app is left at the title screen with a fresh game.
app_resume :: proc(a: ^App, text: string) -> bool {
	saved: Game
	if !save_decode(&saved, text) || !saved.embarked {
		return false
	}
	a.game = saved
	ui_enter(a, play_screen(&a.game))
	return true
}

// The page tells us the window is tall (phone in portrait) or wide.
app_set_layout :: proc(a: ^App, portrait: bool) {
	a.cols = portrait ? PORTRAIT_COLS : LANDSCAPE_COLS
	a.rows = portrait ? PORTRAIT_ROWS : LANDSCAPE_ROWS
}
