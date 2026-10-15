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
	now_ms:    f64, // set by the platform before each event
	open_link: Link, // set by a screen; the platform opens it and clears it
}

app_init :: proc(a: ^App, seed: u64, now_ms: f64) {
	a^ = {}
	rng_seed(&a.game.rng, seed)
	a.now_ms = now_ms
	ui_enter(a, .Title)
}
