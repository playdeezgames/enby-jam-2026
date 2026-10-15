package gwen

import "core:encoding/json"
import "core:fmt"
import "core:mem"
import "core:strconv"

// Saving: a versioned JSON document. Enums are stored as integers and every field is
// range-checked on load; a failed load leaves the game untouched.

SAVE_VERSION :: 1
SAVE_MESSAGES :: 24

Save_Item :: struct {
	kind: int,
	snax: int,
}

Save_Feature :: struct {
	kind:      int,
	remaining: int,
	jools:     int,
	cat:       int,
	items:     []Save_Item,
}

Save_Message :: struct {
	text: string,
	link: int,
}

Save_Data :: struct {
	version:             int,
	rng:                 string, // hex, JSON numbers lose the top bits of a u64
	embarked:            bool,
	counters:            [len(Counter)][2]int, // value, maximum (minimums never change)
	journey_complete:    bool,
	changing_pace:       bool,
	inventory:           []Save_Item,
	ground:              []Save_Item,
	features:            []Save_Feature,
	characters:          []int,
	foraging_difficulty: int,
	shortcut:            bool,
	ad_finish_ms:        f64,
	messages:            []Save_Message,
}

// Scratch space for encode and decode (never both at once).
@(private = "file")
scratch: [1 << 18]u8

@(private = "file")
scratch_allocator :: proc() -> mem.Allocator {
	@(static) arena: mem.Arena
	mem.arena_init(&arena, scratch[:])
	return mem.arena_allocator(&arena)
}

// The result lives in a package buffer until the next save_encode or save_decode.
save_encode :: proc(g: ^Game) -> (text: string, ok: bool) {
	alloc := scratch_allocator()
	d: Save_Data
	d.version = SAVE_VERSION
	d.rng = fmt.aprintf("%x", g.rng.state, allocator = alloc)
	d.embarked = g.embarked
	for c in Counter {
		d.counters[int(c)] = {g.counters[c].value, g.counters[c].max}
	}
	d.journey_complete = g.journey_complete
	d.changing_pace = g.changing_pace
	d.inventory = items_to_save(&g.inventory, alloc)
	d.ground = items_to_save(&g.ground, alloc)
	features := make([]Save_Feature, g.feature_count, alloc)
	for i in 0 ..< g.feature_count {
		f := &g.features[i]
		features[i] = Save_Feature{int(f.kind), f.remaining, f.jools, f.cat, items_to_save(&f.items, alloc)}
	}
	d.features = features
	characters := make([]int, g.character_count, alloc)
	for i in 0 ..< g.character_count {
		characters[i] = int(g.characters[i])
	}
	d.characters = characters
	d.foraging_difficulty = g.foraging_difficulty
	d.shortcut = g.shortcut
	d.ad_finish_ms = g.ad_finish_ms
	keep := min(g.message_count, SAVE_MESSAGES)
	first := g.message_count - keep
	messages := make([]Save_Message, keep, alloc)
	for i in 0 ..< keep {
		m := &g.messages[first + i]
		messages[i] = Save_Message{message_text(m), int(m.link)}
	}
	d.messages = messages

	data, err := json.marshal(d, {}, alloc)
	if err != nil {
		return "", false
	}
	return string(data), true
}

@(private = "file")
items_to_save :: proc(l: ^Items, alloc: mem.Allocator) -> []Save_Item {
	out := make([]Save_Item, l.count, alloc)
	for i in 0 ..< l.count {
		out[i] = Save_Item{int(l.items[i].kind), l.items[i].snax}
	}
	return out
}

@(private = "file")
items_from_save :: proc(l: ^Items, src: []Save_Item) -> bool {
	if len(src) > ITEM_CAP {
		return false
	}
	l^ = {}
	for s in src {
		if s.kind < 0 || s.kind >= len(Item_Kind) || s.snax < 0 || s.snax > 12 {
			return false
		}
		items_add(l, Item{Item_Kind(s.kind), s.snax})
	}
	return true
}

// Replaces g only if the whole document is valid.
save_decode :: proc(g: ^Game, text: string) -> bool {
	alloc := scratch_allocator()
	d: Save_Data
	if json.unmarshal(transmute([]u8)text, &d, allocator = alloc) != nil {
		return false
	}
	if d.version != SAVE_VERSION {
		return false
	}
	out: Game
	state, state_ok := strconv.parse_u64_of_base(d.rng, 16)
	if !state_ok {
		return false
	}
	out.rng.state = state
	out.embarked = d.embarked
	for c in Counter {
		def := counter_defaults[c]
		value, maximum := d.counters[int(c)][0], d.counters[int(c)][1]
		if maximum < def[1] || maximum > INF || value < def[1] || value > maximum {
			return false
		}
		// Only the foraging threshold ever changes its maximum.
		if c != .Forage_Experience && maximum != def[2] {
			return false
		}
		out.counters[c] = Counter_State{value, def[1], maximum}
	}
	out.journey_complete = d.journey_complete
	out.changing_pace = d.changing_pace
	if !items_from_save(&out.inventory, d.inventory) || !items_from_save(&out.ground, d.ground) {
		return false
	}
	if len(d.features) > MAX_FEATURES || len(d.characters) > MAX_CHARACTERS {
		return false
	}
	for s in d.features {
		if s.kind < 0 || s.kind >= len(Feature_Kind) {
			return false
		}
		if s.remaining < 0 || s.remaining > 12 || s.jools < 0 || s.jools > 12 || s.cat < 0 || s.cat >= len(CAT_NAMES) {
			return false
		}
		f := &out.features[out.feature_count]
		out.feature_count += 1
		f.kind = Feature_Kind(s.kind)
		f.remaining = s.remaining
		f.jools = s.jools
		f.cat = s.cat
		if !items_from_save(&f.items, s.items) {
			return false
		}
	}
	for k in d.characters {
		if k < 0 || k >= len(Character_Kind) {
			return false
		}
		out.characters[out.character_count] = Character_Kind(k)
		out.character_count += 1
	}
	if d.foraging_difficulty < 0 || d.foraging_difficulty > 1000 {
		return false
	}
	out.foraging_difficulty = d.foraging_difficulty
	out.shortcut = d.shortcut
	if d.ad_finish_ms < 0 {
		return false
	}
	out.ad_finish_ms = d.ad_finish_ms
	if len(d.messages) > MAX_MESSAGES {
		return false
	}
	for s in d.messages {
		if s.link < 0 || s.link >= len(Link) {
			return false
		}
		m := &out.messages[out.message_count]
		out.message_count += 1
		n := copy(m.text[:], s.text)
		m.len = n
		m.link = Link(s.link)
	}
	g^ = out
	return true
}
