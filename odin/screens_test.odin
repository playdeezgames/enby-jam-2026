#+build !js
package gwen

import "core:testing"

new_app :: proc(seed: u64 = 1) -> App {
	a: App
	app_init(&a, seed, 1_000_000)
	return a
}

// Activates the choice with this label; false if it is not on the screen.
pick :: proc(a: ^App, label: string) -> bool {
	for i in 0 ..< a.ui.choice_count {
		if choice_label(&a.ui.choices[i]) == label {
			ui_choose(a, i)
			return true
		}
	}
	return false
}

has_choice :: proc(a: ^App, label: string) -> bool {
	for i in 0 ..< a.ui.choice_count {
		if choice_label(&a.ui.choices[i]) == label {
			return true
		}
	}
	return false
}

type_text :: proc(a: ^App, s: string) {
	for ch in s {
		app_key(a, i32(ch))
	}
	app_key(a, i32(Key.Enter))
}

// Plays through the title, name and pronoun screens into the game.
embark_through_menus :: proc(a: ^App, name, pronouns: string) {
	pick(a, "OK")
	pick(a, "Embark!")
	type_text(a, name)
	pick(a, "Next")
	pick(a, pronouns)
	pick(a, "Next")
}

@(test)
starts_at_title_with_links :: proc(t: ^testing.T) {
	a := new_app()
	testing.expect_value(t, a.ui.screen, Screen.Title)
	testing.expect(t, has_choice(&a, "OK"))
	testing.expect(t, pick(&a, "UMLAUT.FYI!"))
	testing.expect_value(t, a.open_link, Link.Umlaut)
	testing.expect_value(t, a.ui.screen, Screen.Title)
}

@(test)
menus_lead_into_play :: proc(t: ^testing.T) {
	a := new_app()
	embark_through_menus(&a, "Gwen", "They/Them")
	testing.expect_value(t, a.ui.screen, Screen.Play)
	testing.expect(t, a.game.embarked)
	testing.expect(t, has_choice(&a, "Forage"))
	testing.expect(t, has_choice(&a, "Gämë Mënü"))
	testing.expect(t, !has_choice(&a, "Complete Journey")) // hidden, not greyed
	testing.expect(t, !has_choice(&a, "Take Nap"))
	testing.expect(t, !has_choice(&a, "Take Shortcut..."))
}

@(test)
wrong_answers_are_corrected_but_accepted :: proc(t: ^testing.T) {
	a := new_app()
	pick(&a, "OK")
	pick(&a, "Embark!")
	type_text(&a, "Bob")
	testing.expect_value(t, a.ui.screen, Screen.Name_Result)
	testing.expect(t, has_message(&a.game, "Incorrect! Yer name is `Gwen`!"))
	pick(&a, "Next")
	pick(&a, "He/Him")
	testing.expect(t, has_message(&a.game, "Incorrect! Gwen's pronouns are `They/Them`!"))
	pick(&a, "Next")
	testing.expect_value(t, a.ui.screen, Screen.Play)
}

@(test)
name_is_case_insensitive_and_editable :: proc(t: ^testing.T) {
	a := new_app()
	pick(&a, "OK")
	pick(&a, "Embark!")
	for ch in "gwex" {
		app_key(&a, i32(ch))
	}
	app_key(&a, i32(Key.Backspace))
	for ch in "N" {
		app_key(&a, i32(ch))
	}
	app_key(&a, i32(Key.Enter))
	testing.expect(t, has_message(&a.game, "Correct!"))
}

@(test)
hotkeys_and_cursor_work :: proc(t: ^testing.T) {
	a := new_app()
	app_key(&a, '1') // OK
	testing.expect_value(t, a.ui.screen, Screen.Main_Menu)
	app_key(&a, i32(Key.Down)) // only one choice: wraps
	testing.expect_value(t, a.ui.cursor, 0)
	app_key(&a, i32(Key.Enter))
	testing.expect_value(t, a.ui.screen, Screen.Name_Prompt)
	app_key(&a, i32(Key.Escape))
	testing.expect_value(t, a.ui.screen, Screen.Main_Menu)
}

@(test)
avatar_verbs_return_straight_to_play :: proc(t: ^testing.T) {
	a := new_app()
	embark_through_menus(&a, "Gwen", "They/Them")
	testing.expect(t, pick(&a, "Continue Journey..."))
	testing.expect_value(t, a.ui.screen, Screen.Play)
	testing.expect(t, counter(&a.game, .Distance_Remaining) < 2000)
}

@(test)
pace_menu_sets_pace :: proc(t: ^testing.T) {
	a := new_app()
	embark_through_menus(&a, "Gwen", "They/Them")
	pick(&a, "Change Pace...")
	testing.expect_value(t, a.ui.screen, Screen.Pace)
	pick(&a, "Very Fast(5)")
	testing.expect_value(t, a.ui.screen, Screen.Play)
	testing.expect_value(t, counter(&a.game, .Pace), 5)

	pick(&a, "Change Pace...")
	pick(&a, "Never Mind")
	testing.expect_value(t, counter(&a.game, .Pace), 5)
	testing.expect_value(t, a.ui.screen, Screen.Play)
}

@(test)
feature_flow_picks_up_the_handbook :: proc(t: ^testing.T) {
	a := new_app()
	embark_through_menus(&a, "Gwen", "They/Them")
	spawn_event(&a.game, .Dmv)
	ui_enter(&a, .Play)
	testing.expect(t, pick(&a, "Features..."))
	testing.expect(t, pick(&a, "DMV"))
	testing.expect_value(t, a.ui.screen, Screen.Feature)
	testing.expect(t, has_choice(&a, "Pick Up Handbook"))
	testing.expect(t, has_choice(&a, "Take Permit Test"))
	pick(&a, "Pick Up Handbook")
	testing.expect_value(t, a.ui.screen, Screen.Verb_Result)
	pick(&a, "Ok")
	testing.expect_value(t, a.ui.screen, Screen.Feature)
	testing.expect(t, !has_choice(&a, "Pick Up Handbook"))
	testing.expect(t, has_item(&a.game, .Handbook))
}

@(test)
removed_feature_sends_you_back_to_the_list :: proc(t: ^testing.T) {
	a := new_app()
	embark_through_menus(&a, "Gwen", "They/Them")
	spawn_event(&a.game, .Dishes)
	ui_enter(&a, .Play)
	pick(&a, "Features...")
	pick(&a, "Sink Full of Dirty Dishes")
	pick(&a, "Wash Dishes")
	pick(&a, "Ok")
	testing.expect_value(t, a.game.feature_count, 0)
	testing.expect_value(t, a.ui.screen, Screen.Play) // Features had nothing left, so on to Play
}

@(test)
ground_stacks_take_one_half_all :: proc(t: ^testing.T) {
	a := new_app()
	embark_through_menus(&a, "Gwen", "They/Them")
	for _ in 0 ..< 6 {
		items_add(&a.game.ground, Item{kind = .Flower})
	}
	items_add(&a.game.ground, Item{kind = .Car_Keys})
	ui_enter(&a, .Play)
	pick(&a, "Ground...")
	testing.expect(t, has_choice(&a, "Flower x6"))
	testing.expect(t, has_choice(&a, "Car Keys"))
	pick(&a, "Flower x6")
	testing.expect_value(t, a.ui.screen, Screen.Item_Stack)
	testing.expect(t, has_choice(&a, "Take Half(3)"))
	pick(&a, "Take Half(3)")
	testing.expect_value(t, items_count(&a.game.inventory, .Flower), 3)
	pick(&a, "Take All")
	testing.expect_value(t, items_count(&a.game.inventory, .Flower), 6)
	testing.expect_value(t, a.ui.screen, Screen.Item_List) // stack is gone

	// A single item goes straight to its own menu.
	pick(&a, "Car Keys")
	testing.expect_value(t, a.ui.screen, Screen.Item_Menu)
	pick(&a, "Take")
	testing.expect(t, has_item(&a.game, .Car_Keys))
	testing.expect_value(t, a.game.ground.count, 0)
}

@(test)
inventory_item_menu_has_verbs_and_drop :: proc(t: ^testing.T) {
	a := new_app()
	embark_through_menus(&a, "Gwen", "They/Them")
	items_add(&a.game.inventory, Item{kind = .Handbook})
	ui_enter(&a, .Play)
	pick(&a, "Inventory...")
	pick(&a, "Driver's Handbook")
	testing.expect_value(t, a.ui.screen, Screen.Item_Menu)
	testing.expect(t, has_choice(&a, "Study Handbook"))
	pick(&a, "Study Handbook")
	testing.expect_value(t, counter(&a.game, .Study), 1)
	pick(&a, "Ok")
	testing.expect_value(t, a.ui.screen, Screen.Play)
	pick(&a, "Inventory...")
	pick(&a, "Driver's Handbook")
	pick(&a, "Drop")
	testing.expect_value(t, items_count(&a.game.ground, .Handbook), 1)
}

@(test)
character_menu_gives_flowers :: proc(t: ^testing.T) {
	a := new_app()
	embark_through_menus(&a, "Gwen", "They/Them")
	spawn_event(&a.game, .Traehi)
	items_add(&a.game.inventory, Item{kind = .Flower})
	ui_enter(&a, .Play)
	pick(&a, "Characters...")
	pick(&a, "Traehi")
	testing.expect(t, has_choice(&a, "Give Flower"))
	pick(&a, "Give Flower")
	pick(&a, "Ok")
	testing.expect_value(t, counter(&a.game, .Jools), 1)
	testing.expect_value(t, a.ui.screen, Screen.Character)
	testing.expect(t, !has_choice(&a, "Give Flower"))
}

@(test)
messages_with_links_become_choices :: proc(t: ^testing.T) {
	a := new_app()
	embark_through_menus(&a, "Gwen", "They/Them")
	items_add(&a.game.inventory, Item{kind = .Flower})
	ui_enter(&a, .Play)
	pick(&a, "Inventory...")
	pick(&a, "Flower")
	pick(&a, "Stop and Smell")
	testing.expect_value(t, a.ui.screen, Screen.Verb_Result)
	testing.expect(t, pick(&a, "Visit: Stop and Smell the Flowers, now on Steam"))
	testing.expect_value(t, a.open_link, Link.Steam_Smell)
}

@(test)
ad_break_flow :: proc(t: ^testing.T) {
	a := new_app()
	embark_through_menus(&a, "Gwen", "They/Them")
	pick(&a, "Watch Ad...")
	testing.expect_value(t, a.ui.screen, Screen.Ad)
	testing.expect(t, has_message(&a.game, "Time left in ad break: 02:00"))
	a.now_ms += 30_000
	pick(&a, "Ok") // refresh: still in the ad
	testing.expect_value(t, a.ui.screen, Screen.Ad)
	testing.expect(t, has_message(&a.game, "Time left in ad break: 01:30"))
	a.now_ms += 200_000
	pick(&a, "Ok")
	testing.expect(t, has_message(&a.game, "Ad break is complete!"))
	pick(&a, "Ok")
	testing.expect_value(t, a.ui.screen, Screen.Play)
	testing.expect(t, has_item(&a.game, .Coupon))
}

@(test)
game_menu_abandon_needs_confirmation :: proc(t: ^testing.T) {
	a := new_app()
	embark_through_menus(&a, "Gwen", "They/Them")
	pick(&a, "Gämë Mënü")
	pick(&a, "Abandon")
	testing.expect_value(t, a.ui.screen, Screen.Confirm_Abandon)
	pick(&a, "No")
	testing.expect_value(t, a.ui.screen, Screen.Play)
	testing.expect(t, a.game.embarked)
	pick(&a, "Gämë Mënü")
	pick(&a, "Abandon")
	pick(&a, "Yes")
	testing.expect_value(t, a.ui.screen, Screen.Main_Menu)
	testing.expect(t, !a.game.embarked)
}

@(test)
dying_leads_to_done_then_main_menu :: proc(t: ^testing.T) {
	a := new_app()
	embark_through_menus(&a, "Gwen", "They/Them")
	apply_damage(&a.game, 500)
	ui_enter(&a, play_screen(&a.game))
	testing.expect_value(t, a.ui.screen, Screen.Done)
	testing.expect_value(t, a.ui.choice_count, 1)
	pick(&a, "Ok")
	testing.expect_value(t, a.ui.screen, Screen.Main_Menu)
}

@(test)
finishing_the_journey_leads_to_done :: proc(t: ^testing.T) {
	a := new_app()
	embark_through_menus(&a, "Gwen", "They/Them")
	counter_set(&a.game, .Distance_Remaining, 0)
	ui_enter(&a, .Play)
	testing.expect(t, pick(&a, "Complete Journey"))
	testing.expect_value(t, a.ui.screen, Screen.Done)
}

@(test)
taps_activate_the_row_under_them :: proc(t: ^testing.T) {
	a := new_app()
	grid: Grid
	app_draw(&a, &grid)
	first_row, rows, _ := choice_layout(&a.ui)
	testing.expect(t, rows == 4)
	app_tap(&a, 5, first_row) // OK
	testing.expect_value(t, a.ui.screen, Screen.Main_Menu)
}

@(test)
every_screen_draws_inside_the_grid :: proc(t: ^testing.T) {
	a := new_app()
	embark_through_menus(&a, "Gwen", "They/Them")
	// Pile everything into play so the busiest menu is shown.
	for e in Event {
		spawn_event(&a.game, e)
	}
	for k in Item_Kind {
		items_add(&a.game.inventory, Item{kind = k})
		items_add(&a.game.ground, Item{kind = k})
	}
	counter_set(&a.game, .Jools, 10)
	counter_set(&a.game, .Fatigue, 80)
	for s in Screen {
		grid: Grid
		ui_enter(&a, s)
		app_draw(&a, &grid)
		inked := 0
		for row in grid {
			for c in row {
				if c.ch != 0 {
					inked += 1
				}
			}
		}
		testing.expectf(t, inked > 0, "screen %v drew nothing", s)
	}
}

@(test)
cp437_maps_the_umlauts_the_game_uses :: proc(t: ^testing.T) {
	buf: [32]u8
	n := cp437_encode(buf[:], "Gämë Mënü PÖÖF Körmy")
	testing.expect_value(t, n, 20)
	testing.expect_value(t, buf[1], 0x84)
	testing.expect_value(t, buf[3], 0x89)
	testing.expect_value(t, buf[8], 0x81)
}

@(test)
wrapping_breaks_at_spaces_and_long_words :: proc(t: ^testing.T) {
	text := transmute([]u8)string("aaa bbb ccc")
	end, next := wrap_line(text, 0, 5)
	testing.expect_value(t, end, 3)
	testing.expect_value(t, next, 4)
	end, next = wrap_line(text, 4, 5)
	testing.expect_value(t, end, 7)
	testing.expect_value(t, next, 8)
	end, next = wrap_line(text, 0, 7) // exactly fits: breaks on the space after it
	testing.expect_value(t, end, 7)
	testing.expect_value(t, next, 8)

	long := transmute([]u8)string("abcdefghij")
	end, next = wrap_line(long, 0, 4)
	testing.expect_value(t, end, 4)
	testing.expect_value(t, next, 4)
}
