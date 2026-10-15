package gwen

import "core:fmt"

// The menu state machine. Every screen is: some messages, a prompt, and either a list of
// choices or a line of text to type. Entering a screen runs its side effects and rebuilds
// its choices, so a screen is always consistent with the game state.

Screen :: enum {
	Title,
	Main_Menu,
	Name_Prompt,
	Name_Result,
	Pronouns,
	Pronouns_Result,
	Play,
	Pace,
	Ad,
	Done,
	Game_Menu,
	Confirm_Abandon,
	Characters,
	Character,
	Features,
	Feature,
	Item_List,
	Item_Stack,
	Item_Menu,
	Verb_Result,
}

Item_Source :: enum {
	Ground,
	Inventory,
	Feature,
}

Choice_Kind :: enum {
	Go, // a = Screen
	Avatar_Verb, // a = Verb
	Location_Verb, // a = Verb
	Feature_Verb, // a = Verb
	Character_Verb, // a = Verb
	Item_Verb, // a = Verb; the item is ui.sel_item in the inventory
	Pick_Character, // a = index
	Pick_Feature, // a = index
	Pick_Pronouns, // a = index into PRONOUN_OPTIONS
	Set_Pace, // a = pace
	Cancel_Pace,
	Status,
	Look,
	Watch_Ad,
	Embark_Now,
	Abandon_Yes,
	Open_Link, // a = Link
	Open_List, // a = Item_Source
	Pick_Stack, // a = Item_Kind
	Pick_Item, // a = list index
	Move_Many, // a = count; moves ui.sel_kind from the source (take or drop)
	Move_One, // a = list index
}

MAX_CHOICES :: 40

Choice :: struct {
	label: [64]u8,
	len:   int,
	kind:  Choice_Kind,
	a:     int,
}

Ui :: struct {
	screen:       Screen,
	after:        Screen, // where Verb_Result returns to
	src:          Item_Source,
	sel_character: int,
	sel_feature:  int,
	sel_feature_kind: Feature_Kind,
	sel_kind:     Item_Kind,
	sel_item:     int,
	choices:      [MAX_CHOICES]Choice,
	choice_count: int,
	cursor:       int,
	prompt:       [96]u8,
	prompt_len:   int,
	typing:       bool,
	text:         [32]u8,
	text_len:     int,
	hide_messages: bool,
	scroll:       int, // wrapped lines scrolled back from the newest
	chosen_name:  [32]u8,
	chosen_name_len: int,
	chosen_pronouns: int,
}

NAME_PROMPT :: "What is your name, \"Brave Adventurer\"?"
PRONOUN_OPTIONS := [?]string{"He/Him", "He/They", "They/He", "They/Them", "They/She", "She/They", "She/Her", "Any", "Use Name Only"}
CORRECT_PRONOUNS :: 3
PACE_NAMES := [5]string{"Very Slow", "Slow", "Moderate", "Fast", "Very Fast"}

// ---------------------------------------------------------------- building a screen

@(private = "file")
set_prompt :: proc(ui: ^Ui, format: string, args: ..any) {
	s := fmt.bprintf(ui.prompt[:], format, ..args)
	ui.prompt_len = len(s)
}

@(private = "file")
add_choice :: proc(ui: ^Ui, kind: Choice_Kind, a: int, format: string, args: ..any) {
	if ui.choice_count >= MAX_CHOICES {
		return
	}
	c := &ui.choices[ui.choice_count]
	ui.choice_count += 1
	s := fmt.bprintf(c.label[:], format, ..args)
	c.len = len(s)
	c.kind = kind
	c.a = a
}

choice_label :: proc(c: ^Choice) -> string {
	return string(c.label[:c.len])
}

@(private = "file")
go :: proc(ui: ^Ui, label: string, s: Screen) {
	add_choice(ui, .Go, int(s), "%s", label)
}

play_screen :: proc(g: ^Game) -> Screen {
	if in_ad(g) {
		return .Ad
	}
	if is_done(g) {
		return .Done
	}
	if g.changing_pace {
		return .Pace
	}
	return .Play
}

@(private = "file")
list_for :: proc(a: ^App) -> ^Items {
	g := &a.game
	switch a.ui.src {
	case .Ground:
		return &g.ground
	case .Inventory:
		return &g.inventory
	case .Feature:
		if a.ui.sel_feature >= 0 && a.ui.sel_feature < g.feature_count {
			return &g.features[a.ui.sel_feature].items
		}
	}
	return &g.ground
}

@(private = "file")
src_back :: proc(src: Item_Source) -> Screen {
	return src == .Feature ? .Feature : .Play
}

// Distinct kinds in a list, in order of first appearance.
@(private = "file")
stacks_of :: proc(l: ^Items, kinds: ^[ITEM_CAP]Item_Kind) -> int {
	n := 0
	outer: for i in 0 ..< l.count {
		k := l.items[i].kind
		for j in 0 ..< n {
			if kinds[j] == k {
				continue outer
			}
		}
		kinds[n] = k
		n += 1
	}
	return n
}

ui_enter :: proc(a: ^App, screen: Screen) {
	g := &a.game
	ui := &a.ui
	ui.screen = screen
	ui.choice_count = 0
	ui.cursor = 0
	ui.scroll = 0
	ui.prompt_len = 0
	ui.typing = false
	ui.hide_messages = false

	switch screen {
	case .Title:
		clear_messages(g)
		say(g, "Gwen's Peregrination II")
		say(g, "A Production of TheGrumpyGameDev")
		say(g, "Made for Nonbinary Game Jam 2026")
		say(g, "Sponsored by: UMLAUT.FYI!")
		go(ui, "OK", .Main_Menu)
		add_choice(ui, .Open_Link, int(Link.Itch_Profile), "TheGrumpyGameDev")
		add_choice(ui, .Open_Link, int(Link.Jam), "Nonbinary Game Jam 2026")
		add_choice(ui, .Open_Link, int(Link.Umlaut), "UMLAUT.FYI!")

	case .Main_Menu:
		ui.hide_messages = true
		set_prompt(ui, "Main Menu:")
		go(ui, "Embark!", .Name_Prompt)

	case .Name_Prompt:
		ui.hide_messages = true
		ui.typing = true
		ui.text_len = 0
		set_prompt(ui, "%s", NAME_PROMPT)

	case .Name_Result:
		clear_messages(g)
		if equal_fold(string(ui.chosen_name[:ui.chosen_name_len]), NAME) {
			say(g, "Correct!")
		} else {
			say(g, "Incorrect! Yer name is `%s`! (Did you not read the title of the metaphor?)", NAME)
		}
		go(ui, "Next", .Pronouns)

	case .Pronouns:
		ui.hide_messages = true
		set_prompt(ui, "What are yer pronouns?")
		for p, i in PRONOUN_OPTIONS {
			add_choice(ui, .Pick_Pronouns, i, "%s", p)
		}

	case .Pronouns_Result:
		clear_messages(g)
		if ui.chosen_pronouns == CORRECT_PRONOUNS {
			say(g, "Correct!")
		} else {
			say(
				g,
				"Incorrect! %s's pronouns are `%s`! (This metaphor is an entry in `Nonbinary Game Jam 2026`, so the main character has to be an enby. Next time, you can engage with the metaphor using whichever pronouns you feel most comfortable.)",
				NAME,
				PRONOUNS,
			)
		}
		add_choice(ui, .Embark_Now, 0, "Next")

	case .Play:
		set_prompt(ui, "Now What?")
		for v in avatar_verbs {
			if verb_enabled(g, v) {
				add_choice(ui, .Avatar_Verb, int(v), "%s", verb_names[v])
			}
		}
		add_choice(ui, .Status, 0, "Status...")
		if g.ground.count > 0 {
			add_choice(ui, .Open_List, int(Item_Source.Ground), "Ground...")
		}
		if g.inventory.count > 0 {
			add_choice(ui, .Open_List, int(Item_Source.Inventory), "Inventory...")
		}
		if g.character_count > 0 {
			go(ui, "Characters...", .Characters)
		}
		if g.feature_count > 0 {
			go(ui, "Features...", .Features)
		}
		add_choice(ui, .Look, 0, "Look")
		if verb_enabled(g, .Take_Shortcut) {
			add_choice(ui, .Location_Verb, int(Verb.Take_Shortcut), "%s", verb_names[.Take_Shortcut])
		}
		add_choice(ui, .Watch_Ad, 0, "Watch Ad...")
		go(ui, "Gämë Mënü", .Game_Menu)

	case .Pace:
		set_prompt(ui, "Choose Pace:")
		add_choice(ui, .Cancel_Pace, 0, "Never Mind")
		for name, i in PACE_NAMES {
			add_choice(ui, .Set_Pace, i + 1, "%s(%d)", name, i + 1)
		}

	case .Ad:
		ad_show(g, a.now_ms)
		go(ui, "Ok", .Play)

	case .Done:
		go(ui, "Ok", .Main_Menu)

	case .Game_Menu:
		ui.hide_messages = true
		set_prompt(ui, "Game Menu:")
		go(ui, "Continue", .Play)
		go(ui, "Abandon", .Confirm_Abandon)

	case .Confirm_Abandon:
		ui.hide_messages = true
		set_prompt(ui, "Are you sure you want to abandon?")
		go(ui, "No", .Play)
		add_choice(ui, .Abandon_Yes, 0, "Yes")

	case .Characters:
		if g.character_count == 0 {
			ui_enter(a, .Play)
			return
		}
		clear_messages(g)
		say(g, "Characters:")
		for i in 0 ..< g.character_count {
			say(g, "- %s", character_names[g.characters[i]])
		}
		set_prompt(ui, "Which Character?")
		go(ui, "Never Mind", .Play)
		for i in 0 ..< g.character_count {
			add_choice(ui, .Pick_Character, i, "%s", character_names[g.characters[i]])
		}

	case .Character:
		if ui.sel_character >= g.character_count {
			ui_enter(a, .Characters)
			return
		}
		kind := g.characters[ui.sel_character]
		clear_messages(g)
		say(g, "%s", character_flavors[kind])
		set_prompt(ui, "Do what with %s?", character_names[kind])
		go(ui, "Never Mind", .Characters)
		for v in character_verbs[kind] {
			if verb_enabled(g, v, ui.sel_character) {
				add_choice(ui, .Character_Verb, int(v), "%s", verb_names[v])
			}
		}

	case .Features:
		if g.feature_count == 0 {
			ui_enter(a, .Play)
			return
		}
		clear_messages(g)
		say(g, "Features:")
		for i in 0 ..< g.feature_count {
			say(g, "- %s", feature_names[g.features[i].kind])
		}
		set_prompt(ui, "Which feature?")
		go(ui, "Never Mind", .Play)
		for i in 0 ..< g.feature_count {
			add_choice(ui, .Pick_Feature, i, "%s", feature_names[g.features[i].kind])
		}

	case .Feature:
		i := ui.sel_feature
		if i < 0 || i >= g.feature_count || g.features[i].kind != ui.sel_feature_kind {
			ui_enter(a, .Features)
			return
		}
		examine_feature(g, i)
		set_prompt(ui, "Do what with %s?", feature_names[g.features[i].kind])
		go(ui, "Never Mind", .Features)
		if g.features[i].items.count > 0 {
			add_choice(ui, .Open_List, int(Item_Source.Feature), "Items...")
		}
		for v in feature_verbs[g.features[i].kind] {
			if verb_enabled(g, v, i) {
				add_choice(ui, .Feature_Verb, int(v), "%s", verb_names[v])
			}
		}

	case .Item_List:
		l := list_for(a)
		if l.count == 0 {
			ui_enter(a, src_back(ui.src))
			return
		}
		switch ui.src {
		case .Ground:
			set_prompt(ui, "On the ground:")
		case .Inventory:
			set_prompt(ui, "Inventory:")
		case .Feature:
			set_prompt(ui, "Items in %s:", feature_names[g.features[ui.sel_feature].kind])
		}
		go(ui, "Never Mind", src_back(ui.src))
		kinds: [ITEM_CAP]Item_Kind
		n := stacks_of(l, &kinds)
		for i in 0 ..< n {
			count := items_count(l, kinds[i])
			if count > 1 {
				add_choice(ui, .Pick_Stack, int(kinds[i]), "%s x%d", item_names[kinds[i]], count)
			} else {
				add_choice(ui, .Pick_Stack, int(kinds[i]), "%s", item_names[kinds[i]])
			}
		}

	case .Item_Stack:
		l := list_for(a)
		count := items_count(l, ui.sel_kind)
		if count == 0 {
			ui_enter(a, .Item_List)
			return
		}
		set_prompt(ui, "Items in stack:")
		go(ui, "Never Mind", .Item_List)
		verb := ui.src == .Inventory ? "Drop" : "Take"
		if count > 1 {
			add_choice(ui, .Move_Many, 1, "%s One", verb)
		}
		if count > 3 {
			add_choice(ui, .Move_Many, count / 2, "%s Half(%d)", verb, count / 2)
		}
		if count > 1 {
			add_choice(ui, .Move_Many, count, "%s All", verb)
		}
		for i in 0 ..< l.count {
			if l.items[i].kind == ui.sel_kind {
				add_choice(ui, .Pick_Item, i, "%s", item_names[ui.sel_kind])
			}
		}

	case .Item_Menu:
		l := list_for(a)
		if ui.sel_item < 0 || ui.sel_item >= l.count {
			ui_enter(a, .Item_List)
			return
		}
		kind := l.items[ui.sel_item].kind
		ui.sel_kind = kind
		set_prompt(ui, "Do what with %s?", item_names[kind])
		go(ui, "Never Mind", .Item_List)
		if ui.src == .Inventory {
			for v in item_verbs[kind] {
				if verb_enabled(g, v, ui.sel_item) {
					add_choice(ui, .Item_Verb, int(v), "%s", verb_names[v])
				}
			}
			add_choice(ui, .Move_One, ui.sel_item, "Drop")
		} else {
			add_choice(ui, .Move_One, ui.sel_item, "Take")
		}

	case .Verb_Result:
		go(ui, "Ok", ui.after)
	}

	// Links inside messages become choices too, so they work from the keyboard.
	if !ui.hide_messages && !ui.typing {
		for i in 0 ..< g.message_count {
			m := &g.messages[i]
			if m.link != .None && screen != .Title {
				add_choice(ui, .Open_Link, int(m.link), "Visit: %s", message_text(m))
			}
		}
	}
}

@(private = "file")
equal_fold :: proc(a, b: string) -> bool {
	if len(a) != len(b) {
		return false
	}
	for i in 0 ..< len(a) {
		x, y := a[i], b[i]
		if x >= 'A' && x <= 'Z' {
			x += 32
		}
		if y >= 'A' && y <= 'Z' {
			y += 32
		}
		if x != y {
			return false
		}
	}
	return true
}

// ---------------------------------------------------------------- acting on a choice

ui_choose :: proc(a: ^App, index: int) {
	ui := &a.ui
	g := &a.game
	if index < 0 || index >= ui.choice_count {
		return
	}
	c := ui.choices[index]
	switch c.kind {
	case .Go:
		if Screen(c.a) == .Play {
			ui_enter(a, play_screen(g))
		} else {
			ui_enter(a, Screen(c.a))
		}
	case .Avatar_Verb:
		perform(g, Verb(c.a))
		ui_enter(a, play_screen(g))
	case .Location_Verb:
		perform(g, Verb(c.a))
		ui.after = .Play
		ui_enter(a, .Verb_Result)
	case .Feature_Verb:
		perform(g, Verb(c.a), ui.sel_feature)
		ui.after = .Feature
		ui_enter(a, .Verb_Result)
	case .Character_Verb:
		perform(g, Verb(c.a), ui.sel_character)
		ui.after = .Character
		ui_enter(a, .Verb_Result)
	case .Item_Verb:
		perform(g, Verb(c.a), ui.sel_item)
		ui.after = .Play
		ui_enter(a, .Verb_Result)
	case .Pick_Character:
		ui.sel_character = c.a
		ui_enter(a, .Character)
	case .Pick_Feature:
		ui.sel_feature = c.a
		ui.sel_feature_kind = g.features[c.a].kind
		ui_enter(a, .Feature)
	case .Pick_Pronouns:
		ui.chosen_pronouns = c.a
		ui_enter(a, .Pronouns_Result)
	case .Set_Pace:
		counter_set(g, .Pace, c.a)
		g.changing_pace = false
		ui_enter(a, play_screen(g))
	case .Cancel_Pace:
		g.changing_pace = false
		ui_enter(a, play_screen(g))
	case .Status:
		status(g)
		ui_enter(a, play_screen(g))
	case .Look:
		look_action(g)
		ui_enter(a, play_screen(g))
	case .Watch_Ad:
		ad_start(g, a.now_ms)
		ui_enter(a, play_screen(g))
	case .Embark_Now:
		embark(g)
		ui_enter(a, play_screen(g))
	case .Abandon_Yes:
		abandon(g)
		ui_enter(a, .Main_Menu)
	case .Open_Link:
		a.open_link = Link(c.a)
	case .Open_List:
		ui.src = Item_Source(c.a)
		ui_enter(a, .Item_List)
	case .Pick_Stack:
		ui.sel_kind = Item_Kind(c.a)
		l := list_for(a)
		if items_count(l, ui.sel_kind) == 1 {
			ui.sel_item = items_find(l, ui.sel_kind)
			ui_enter(a, .Item_Menu)
		} else {
			ui_enter(a, .Item_Stack)
		}
	case .Pick_Item:
		ui.sel_item = c.a
		ui_enter(a, .Item_Menu)
	case .Move_Many:
		move_stack(a, ui.sel_kind, c.a)
		ui_enter(a, .Item_Stack)
	case .Move_One:
		move_stack(a, list_for(a).items[c.a].kind, 1)
		ui_enter(a, .Item_List)
	}
}

@(private = "file")
move_stack :: proc(a: ^App, kind: Item_Kind, count: int) {
	g := &a.game
	switch a.ui.src {
	case .Ground:
		take_from_ground(g, kind, count)
	case .Inventory:
		drop_item(g, kind, count)
	case .Feature:
		take_from_feature(g, a.ui.sel_feature, kind, count)
	}
}

// ---------------------------------------------------------------- input

choice_hotkey :: proc(i: int) -> u8 {
	if i < 9 {
		return u8('1' + i)
	}
	return u8('a' + (i - 9))
}

app_key :: proc(a: ^App, key: i32) {
	ui := &a.ui
	if ui.typing {
		switch Key(key) {
		case .Enter:
			if ui.text_len > 0 {
				ui.chosen_name_len = ui.text_len
				copy(ui.chosen_name[:], ui.text[:ui.text_len])
			} else {
				ui.chosen_name_len = 0
			}
			ui_enter(a, .Name_Result)
		case .Backspace:
			if ui.text_len > 0 {
				ui.text_len -= 1
			}
		case .Escape:
			ui_enter(a, .Main_Menu)
		case .Up, .Down, .Left, .Right, .Page_Up, .Page_Down, .None:
		case:
			if key >= 32 && key < 127 && ui.text_len < len(ui.text) {
				ui.text[ui.text_len] = u8(key)
				ui.text_len += 1
			}
		}
		return
	}
	switch Key(key) {
	case .Up:
		if ui.choice_count > 0 {
			ui.cursor = (ui.cursor + ui.choice_count - 1) % ui.choice_count
		}
	case .Down:
		if ui.choice_count > 0 {
			ui.cursor = (ui.cursor + 1) % ui.choice_count
		}
	case .Enter:
		ui_choose(a, ui.cursor)
	case .Escape:
		if ui.choice_count > 0 {
			label := choice_label(&ui.choices[0])
			if label == "Never Mind" || label == "No" {
				ui_choose(a, 0)
			}
		}
	case .Page_Up:
		ui.scroll += 4
	case .Page_Down:
		ui.scroll = max(ui.scroll - 4, 0)
	case .Left, .Right, .Backspace, .None:
	case:
		k := key
		if k >= 'A' && k <= 'Z' {
			k += 32
		}
		for i in 0 ..< ui.choice_count {
			if i64(choice_hotkey(i)) == i64(k) {
				ui_choose(a, i)
				return
			}
		}
	}
}

// ---------------------------------------------------------------- drawing

MSG_LEFT :: 1
MSG_WIDTH :: COLS - 2

Wrapped :: struct {
	msg:        int,
	start, end: int,
}

// Where the choice list begins and how many rows it shows (set by app_draw, used by taps).
choice_layout :: proc(ui: ^Ui) -> (first_row, rows, first_index: int) {
	prompt_rows := ui.prompt_len > 0 ? 1 : 0
	if ui.typing {
		return ROWS - 2, 1, 0
	}
	rows = min(ui.choice_count, ROWS - 8)
	first_row = ROWS - 1 - rows
	if ui.choice_count > rows {
		first_index = clamp(ui.cursor - rows / 2, 0, ui.choice_count - rows)
	}
	_ = prompt_rows
	return
}

app_draw :: proc(a: ^App, grid: ^Grid) {
	g := &a.game
	ui := &a.ui
	grid_clear(grid)

	first_row, rows, first_index := choice_layout(ui)
	prompt_row := first_row - 1
	if ui.typing {
		prompt_row = first_row - 1
	}
	msg_bottom := prompt_row - 2 // blank row above the prompt

	// Messages: wrapped, newest at the bottom, PageUp scrolls back.
	if !ui.hide_messages {
		lines: [512]Wrapped
		n := 0
		text: [MESSAGE_CAP]u8
		for i in 0 ..< g.message_count {
			m := &g.messages[i]
			len_ := cp437_encode(text[:], message_text(m))
			start := 0
			for {
				end, next := wrap_line(text[:len_], start, MSG_WIDTH)
				if n < len(lines) {
					lines[n] = Wrapped{i, start, end}
					n += 1
				}
				if next >= len_ {
					break
				}
				start = next
			}
		}
		avail := msg_bottom + 1
		max_scroll := max(n - avail, 0)
		ui.scroll = min(ui.scroll, max_scroll)
		last := n - 1 - ui.scroll
		row := msg_bottom
		for li := last; li >= 0 && row >= 0; li -= 1 {
			w := lines[li]
			m := &g.messages[w.msg]
			len_ := cp437_encode(text[:], message_text(m))
			ink := m.link == .None ? u8(Ink.Default) : u8(Ink.Cyan)
			grid_bytes(grid, MSG_LEFT, row, text[w.start:min(w.end, len_)], ink)
			row -= 1
		}
		if ui.scroll > 0 {
			grid_text(grid, COLS - 12, 0, "(PgDn: newer)", u8(Ink.Gray))
		}
		if last - avail + 1 > 0 {
			grid_text(grid, COLS - 12, 0, "(PgUp: older)", u8(Ink.Gray))
		}
	}

	// Prompt and choices.
	if ui.prompt_len > 0 {
		grid_text(grid, MSG_LEFT, prompt_row, string(ui.prompt[:ui.prompt_len]), u8(Ink.Yellow))
	}
	if ui.typing {
		row := first_row
		c := grid_text(grid, MSG_LEFT, row, "> ", u8(Ink.White))
		grid_bytes(grid, c, row, ui.text[:ui.text_len], u8(Ink.White))
		grid_put(grid, c + ui.text_len, row, '_', u8(Ink.Yellow))
		return
	}
	for r in 0 ..< rows {
		i := first_index + r
		ch := &ui.choices[i]
		ink := u8(Ink.Default)
		if ch.kind == .Open_Link {
			ink = u8(Ink.Cyan)
		}
		attr := ink
		if i == ui.cursor {
			attr |= ATTR_INVERSE
		}
		row := first_row + r
		// Fill the whole row so the selection reads as a bar.
		if i == ui.cursor {
			for c in MSG_LEFT ..< COLS - 1 {
				grid_put(grid, c, row, ' ', attr)
			}
		}
		col := grid_text(grid, MSG_LEFT, row, "", attr)
		grid_put(grid, col, row, choice_hotkey(i), attr)
		grid_put(grid, col + 1, row, '.', attr)
		grid_text(grid, col + 3, row, choice_label(ch), attr)
	}
	if ui.choice_count > rows {
		if first_index > 0 {
			grid_put(grid, COLS - 2, first_row, 0x18, u8(Ink.Gray))
		}
		if first_index + rows < ui.choice_count {
			grid_put(grid, COLS - 2, first_row + rows - 1, 0x19, u8(Ink.Gray))
		}
	}
}

// A tap or click on a grid cell: selects and activates the choice under it.
app_tap :: proc(a: ^App, col, row: int) {
	ui := &a.ui
	if ui.typing {
		return
	}
	first_row, rows, first_index := choice_layout(ui)
	if row >= first_row && row < first_row + rows {
		ui_choose(a, first_index + (row - first_row))
	}
}

// True when the page should bring up a keyboard (touch devices).
app_wants_text :: proc(a: ^App) -> bool {
	return a.ui.typing
}
