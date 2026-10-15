#+build !js
package gwen

import "core:testing"

busy_game :: proc() -> Game {
	g := new_test_game(31)
	for e in Event {
		spawn_event(&g, e)
	}
	for k in Item_Kind {
		items_add(&g.inventory, Item{kind = k, snax = 3})
		items_add(&g.ground, Item{kind = k})
	}
	counter_set(&g, .Jools, 7)
	counter_set(&g, .Health, 63)
	g.counters[.Forage_Experience].max = 40
	counter_set(&g, .Forage_Experience, 12)
	g.changing_pace = true
	ad_start(&g, 5_000)
	g.rng.state = 0xFEDC_BA98_7654_3210
	return g
}

@(test)
save_round_trips_everything :: proc(t: ^testing.T) {
	g := busy_game()
	text, ok := save_encode(&g)
	testing.expect(t, ok)
	// Copy out: the encoder's buffer is reused by decode.
	saved := make([]u8, len(text))
	defer delete(saved)
	copy(saved, text)

	out: Game
	testing.expect(t, save_decode(&out, string(saved)))
	testing.expect_value(t, out.rng.state, g.rng.state)
	testing.expect_value(t, out.embarked, g.embarked)
	for c in Counter {
		testing.expect_value(t, out.counters[c], g.counters[c])
	}
	testing.expect_value(t, out.inventory.count, g.inventory.count)
	for i in 0 ..< g.inventory.count {
		testing.expect_value(t, out.inventory.items[i], g.inventory.items[i])
	}
	testing.expect_value(t, out.ground.count, g.ground.count)
	testing.expect_value(t, out.feature_count, g.feature_count)
	for i in 0 ..< g.feature_count {
		testing.expect_value(t, out.features[i].kind, g.features[i].kind)
		testing.expect_value(t, out.features[i].items.count, g.features[i].items.count)
		testing.expect_value(t, out.features[i].remaining, g.features[i].remaining)
		testing.expect_value(t, out.features[i].cat, g.features[i].cat)
	}
	testing.expect_value(t, out.character_count, g.character_count)
	testing.expect_value(t, out.changing_pace, true)
	testing.expect_value(t, out.ad_finish_ms, g.ad_finish_ms)
	testing.expect_value(t, out.foraging_difficulty, g.foraging_difficulty)
	testing.expect_value(t, out.message_count, min(g.message_count, SAVE_MESSAGES))
	last := out.message_count - 1
	testing.expect_value(t, message_text(&out.messages[last]), message_text(&g.messages[g.message_count - 1]))
}

@(test)
umlauts_survive_a_save :: proc(t: ^testing.T) {
	g := new_test_game()
	say(&g, "Körmy says PÖÖF in the Gämë Mënü")
	text, _ := save_encode(&g)
	out: Game
	testing.expect(t, save_decode(&out, text))
	testing.expect_value(t, message_text(&out.messages[out.message_count - 1]), "Körmy says PÖÖF in the Gämë Mënü")
}

@(test)
bad_saves_are_refused_and_leave_the_game_alone :: proc(t: ^testing.T) {
	g := new_test_game()
	counter_set(&g, .Jools, 42)
	text, _ := save_encode(&g)
	good := make([]u8, len(text))
	defer delete(good)
	copy(good, text)

	cases := []string{
		"",
		"{",
		"not json at all",
		"[]",
		"{\"version\":2}",
		string(good[:len(good) / 2]),
	}
	for c in cases {
		target := new_test_game(2)
		before := counter(&target, .Jools)
		testing.expectf(t, !save_decode(&target, c), "accepted %q", c)
		testing.expect_value(t, counter(&target, .Jools), before)
	}
}

@(test)
out_of_range_values_are_refused :: proc(t: ^testing.T) {
	g := new_test_game()
	check :: proc(t: ^testing.T, mutate: proc(g: ^Game), what: string) {
		g := new_test_game()
		mutate(&g)
		text, ok := save_encode(&g)
		testing.expect(t, ok)
		out: Game
		testing.expectf(t, !save_decode(&out, text), "accepted %s", what)
	}
	check(t, proc(g: ^Game) { g.counters[.Health].value = 500 }, "health over max")
	check(t, proc(g: ^Game) { g.counters[.Pace].value = 0 }, "pace under min")
	check(t, proc(g: ^Game) { g.counters[.Health].max = 999 }, "changed max")
	check(t, proc(g: ^Game) { g.foraging_difficulty = -3 }, "negative difficulty")
	check(t, proc(g: ^Game) {
		g.inventory.items[0] = Item{kind = Item_Kind(99)}
		g.inventory.count = 1
	}, "unknown item")
	check(t, proc(g: ^Game) {
		g.features[0] = Feature{kind = Feature_Kind(99)}
		g.feature_count = 1
	}, "unknown feature")
	check(t, proc(g: ^Game) { g.ad_finish_ms = -1 }, "negative ad time")
	_ = g
}

@(test)
resume_goes_straight_to_play :: proc(t: ^testing.T) {
	a := new_app()
	embark_through_menus(&a, "Gwen", "They/Them")
	pick(&a, "Continue Journey...")
	text, _ := save_encode(&a.game)
	miles := counter(&a.game, .Distance_Remaining)

	b := new_app(5)
	testing.expect_value(t, b.ui.screen, Screen.Title)
	testing.expect(t, app_resume(&b, text))
	testing.expect_value(t, b.ui.screen, Screen.Play)
	testing.expect_value(t, counter(&b.game, .Distance_Remaining), miles)
}

@(test)
resume_ignores_an_unstarted_game :: proc(t: ^testing.T) {
	a := new_app()
	text, _ := save_encode(&a.game) // never embarked
	b := new_app(5)
	testing.expect(t, !app_resume(&b, text))
	testing.expect_value(t, b.ui.screen, Screen.Title)
}

@(test)
credits_credit_the_font :: proc(t: ^testing.T) {
	a := new_app()
	pick(&a, "OK")
	pick(&a, "Credits")
	testing.expect_value(t, a.ui.screen, Screen.Credits)
	testing.expect(t, has_message(&a.game, "DINOBYTE by mby"))
	pick(&a, "Ok")
	testing.expect_value(t, a.ui.screen, Screen.Main_Menu)
}
