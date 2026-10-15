#+build !js
package gwen

import "core:testing"

new_test_game :: proc(seed: u64 = 1) -> Game {
	g: Game
	rng_seed(&g.rng, seed)
	embark(&g)
	return g
}

has_message :: proc(g: ^Game, needle: string) -> bool {
	for i in 0 ..< g.message_count {
		text := message_text(&g.messages[i])
		for s in 0 ..< len(text) - len(needle) + 1 {
			if text[s:s + len(needle)] == needle {
				return true
			}
		}
	}
	return false
}

@(test)
embark_sets_documented_start :: proc(t: ^testing.T) {
	g := new_test_game()
	testing.expect_value(t, counter(&g, .Health), 100)
	testing.expect_value(t, counter(&g, .Snax), 10)
	testing.expect_value(t, counter(&g, .Jools), 0)
	testing.expect_value(t, counter(&g, .Distance_Remaining), 2000)
	testing.expect_value(t, counter(&g, .Pace), 3)
	testing.expect(t, g.foraging_difficulty >= 2 && g.foraging_difficulty <= 20)
	testing.expect(t, has_message(&g, "So it begins!"))
	testing.expect(t, !has_item(&g, .Car_Keys))
}

@(test)
hunger_uses_stomach_then_satiety_then_health :: proc(t: ^testing.T) {
	g := new_test_game()
	counter_set(&g, .Stomach, 2)
	apply_hunger(&g, 5)
	testing.expect_value(t, counter(&g, .Stomach), 0)
	testing.expect_value(t, counter(&g, .Satiety), 97)

	counter_set(&g, .Satiety, 1)
	apply_hunger(&g, 4)
	testing.expect_value(t, counter(&g, .Satiety), 0)
	testing.expect_value(t, counter(&g, .Health), 97)
}

@(test)
full_stomach_with_no_hunger_gains_a_satiety :: proc(t: ^testing.T) {
	g := new_test_game()
	counter_set(&g, .Satiety, 50)
	counter_set(&g, .Stomach, 10)
	apply_hunger(&g, 0)
	testing.expect_value(t, counter(&g, .Satiety), 51)
}

@(test)
fatigue_overflow_becomes_damage :: proc(t: ^testing.T) {
	g := new_test_game()
	counter_set(&g, .Fatigue, 98)
	apply_fatigue(&g, 5)
	testing.expect_value(t, counter(&g, .Fatigue), 100)
	testing.expect_value(t, counter(&g, .Health), 97)
}

@(test)
death_ends_the_journey :: proc(t: ^testing.T) {
	g := new_test_game()
	counter_set(&g, .Health, 1)
	apply_damage(&g, 5)
	testing.expect(t, is_dead(&g))
	testing.expect(t, is_done(&g))
	testing.expect(t, !verb_enabled(&g, .Continue_Journey))
	testing.expect(t, !verb_enabled(&g, .Eat_Snax))
}

@(test)
continue_journey_walks_pace_miles :: proc(t: ^testing.T) {
	g := new_test_game()
	counter_set(&g, .Pace, 5)
	perform(&g, .Continue_Journey)
	testing.expect_value(t, counter(&g, .Distance_Remaining), 1995)
	// raw pace of hunger came out of satiety
	testing.expect_value(t, counter(&g, .Satiety), 95)
	testing.expect_value(t, counter(&g, .Fatigue), 5)
}

@(test)
bewwy_hertz_slows_the_walk_and_ticks_down :: proc(t: ^testing.T) {
	g := new_test_game()
	counter_set(&g, .Pace, 5)
	counter_set(&g, .Bewwy_Hertz, 2)
	perform(&g, .Continue_Journey)
	testing.expect_value(t, counter(&g, .Distance_Remaining), 1997) // (5 + 1) / 2
	testing.expect_value(t, counter(&g, .Bewwy_Hertz), 1)
	testing.expect_value(t, counter(&g, .Satiety), 95) // hunger uses the raw pace
}

@(test)
journey_can_be_completed_only_at_zero :: proc(t: ^testing.T) {
	g := new_test_game()
	testing.expect(t, !verb_enabled(&g, .Complete_Journey))
	counter_set(&g, .Distance_Remaining, 0)
	testing.expect(t, verb_enabled(&g, .Complete_Journey))
	perform(&g, .Complete_Journey)
	testing.expect(t, is_done(&g))
}

@(test)
eating_fills_stomach_and_overfull_hurts :: proc(t: ^testing.T) {
	g := new_test_game()
	counter_set(&g, .Stomach, 40)
	perform(&g, .Eat_Snax)
	testing.expect_value(t, counter(&g, .Snax), 9)
	testing.expect_value(t, counter(&g, .Stomach), 50)
	b := counter(&g, .Bewwy_Hertz)
	testing.expect(t, b >= 2 && b <= 12)
}

@(test)
nap_needs_half_fatigue_and_costs_hours_of_hunger :: proc(t: ^testing.T) {
	g := new_test_game()
	counter_set(&g, .Fatigue, 49)
	testing.expect(t, !verb_enabled(&g, .Take_Nap))
	counter_set(&g, .Fatigue, 50)
	testing.expect(t, verb_enabled(&g, .Take_Nap))
	perform(&g, .Take_Nap)
	testing.expect_value(t, counter(&g, .Fatigue), 0)
	testing.expect_value(t, counter(&g, .Satiety), 95) // 5 hours
}

@(test)
foraging_levels_up_and_doubles_the_threshold :: proc(t: ^testing.T) {
	g := new_test_game()
	g.foraging_difficulty = 1000 // never finds, so only xp moves
	for _ in 0 ..< 10 {
		perform(&g, .Forage)
	}
	testing.expect_value(t, counter(&g, .Forage_Skill), 1)
	testing.expect_value(t, counter(&g, .Forage_Experience), 0)
	testing.expect_value(t, g.counters[.Forage_Experience].max, 20)
}

@(test)
successful_forage_raises_the_bar :: proc(t: ^testing.T) {
	g := new_test_game()
	g.foraging_difficulty = 0 // always succeeds
	perform(&g, .Forage)
	testing.expect_value(t, counter(&g, .Snax), 11)
	testing.expect(t, g.foraging_difficulty >= 2 && g.foraging_difficulty <= 21)
}

@(test)
shortcut_gain_is_bounded_and_litter_hurts :: proc(t: ^testing.T) {
	for seed in 1 ..= u64(50) {
		g := new_test_game(seed)
		g.shortcut = true
		counter_set(&g, .Littering, 3)
		counter_set(&g, .Recycling, 1)
		before := counter(&g, .Distance_Remaining)
		perform(&g, .Take_Shortcut)
		delta := counter(&g, .Distance_Remaining) - before
		// 2d6 - 2d6 is -10..10, then -3 + 1
		testing.expect(t, delta >= -12 && delta <= 8)
	}
}

@(test)
shortcut_is_only_available_when_found :: proc(t: ^testing.T) {
	g := new_test_game()
	testing.expect(t, !verb_enabled(&g, .Take_Shortcut))
	spawn_event(&g, .Shortcut)
	testing.expect(t, verb_enabled(&g, .Take_Shortcut))
}

@(test)
moving_on_clears_the_place_and_counts_litter :: proc(t: ^testing.T) {
	g := new_test_game()
	spawn_event(&g, .Dishes)
	items_add(&g.ground, Item{kind = .Litter})
	items_add(&g.ground, Item{kind = .Litter})
	items_add(&g.ground, Item{kind = .Flower})
	update_location(&g)
	testing.expect_value(t, counter(&g, .Littering), 2)
	testing.expect_value(t, g.ground.count, 0)
}

@(test)
every_event_spawns_without_breaking :: proc(t: ^testing.T) {
	for e in Event {
		g := new_test_game()
		spawn_event(&g, e)
		testing.expect(t, g.feature_count <= MAX_FEATURES)
		testing.expect(t, g.message_count > 0)
	}
}

@(test)
event_table_picks_everything_eventually :: proc(t: ^testing.T) {
	g := new_test_game()
	seen: [Event]bool
	for _ in 0 ..< 200000 {
		seen[pick_event(&g)] = true
	}
	for e in Event {
		testing.expectf(t, seen[e], "event %v never picked", e)
	}
}

@(test)
dishes_pay_out_once :: proc(t: ^testing.T) {
	g := new_test_game()
	spawn_event(&g, .Dishes)
	reward := g.features[0].jools
	testing.expect(t, reward >= 2 && reward <= 12)
	perform(&g, .Wash_Dishes, 0)
	testing.expect_value(t, counter(&g, .Jools), reward)
	testing.expect_value(t, counter(&g, .Fatigue), 5)
	testing.expect_value(t, g.feature_count, 0)
}

@(test)
flower_patch_runs_out_and_traehi_pays_per_flower :: proc(t: ^testing.T) {
	g := new_test_game()
	spawn_event(&g, .Flower_Patch)
	n := g.features[0].remaining
	testing.expect(t, n >= 3 && n <= 12)
	for _ in 0 ..< n {
		testing.expect(t, verb_enabled(&g, .Pick_Flower, 0))
		perform(&g, .Pick_Flower, 0)
	}
	testing.expect_value(t, g.feature_count, 0)
	testing.expect_value(t, items_count(&g.inventory, .Flower), n)

	spawn_event(&g, .Traehi)
	perform(&g, .Give_Flower, 0)
	testing.expect_value(t, counter(&g, .Jools), 1)
	testing.expect_value(t, items_count(&g.inventory, .Flower), n - 1)
}

@(test)
give_flower_needs_a_flower_and_traehi :: proc(t: ^testing.T) {
	g := new_test_game()
	spawn_event(&g, .Traehi)
	testing.expect(t, !verb_enabled(&g, .Give_Flower, 0))
	items_add(&g.inventory, Item{kind = .Flower})
	testing.expect(t, verb_enabled(&g, .Give_Flower, 0))
	testing.expect(t, !verb_enabled(&g, .Give_Flower, 1)) // no such person
}

@(test)
vending_machine_costs_a_jool :: proc(t: ^testing.T) {
	g := new_test_game()
	spawn_event(&g, .Vending_Machine)
	testing.expect(t, !verb_enabled(&g, .Buy_Snax, 0))
	counter_set(&g, .Jools, 1)
	perform(&g, .Buy_Snax, 0)
	testing.expect_value(t, counter(&g, .Jools), 0)
	snax := counter(&g, .Snax)
	testing.expect(t, snax >= 10 && snax <= 12)
}

@(test)
dr_pepper_clears_fatigue_and_leaves_litter :: proc(t: ^testing.T) {
	g := new_test_game()
	spawn_event(&g, .Kwik_Trip)
	counter_set(&g, .Jools, 1)
	perform(&g, .Buy_Dr_Pepper, 0)
	testing.expect(t, has_item(&g, .Dr_Pepper))
	counter_set(&g, .Fatigue, 80)
	perform(&g, .Drink, items_find(&g.inventory, .Dr_Pepper))
	testing.expect_value(t, counter(&g, .Fatigue), 0)
	testing.expect(t, !has_item(&g, .Dr_Pepper))
	testing.expect(t, has_item(&g, .Litter))
}

@(test)
recycling_removes_all_litter_and_scores :: proc(t: ^testing.T) {
	g := new_test_game()
	spawn_event(&g, .Recycle_Bin)
	testing.expect(t, !verb_enabled(&g, .Recycle_Litter, 0))
	items_add(&g.inventory, Item{kind = .Litter})
	items_add(&g.inventory, Item{kind = .Litter})
	perform(&g, .Recycle_Litter, 0)
	testing.expect_value(t, counter(&g, .Recycling), 2)
	testing.expect(t, !has_item(&g, .Litter))
}

@(test)
abandoned_house_holds_a_bag_of_snax :: proc(t: ^testing.T) {
	g := new_test_game()
	spawn_event(&g, .Abandoned_House)
	f := &g.features[0]
	testing.expect_value(t, f.items.count, 2)
	take_from_feature(&g, 0, .Pkastic_Bag)
	bag := items_find(&g.inventory, .Pkastic_Bag)
	testing.expect(t, bag >= 0)
	snax := g.inventory.items[bag].snax
	testing.expect(t, snax >= 2 && snax <= 12)
	perform(&g, .Reach_In, bag)
	testing.expect_value(t, counter(&g, .Snax), 10 + snax)
	perform(&g, .Reach_In, bag)
	testing.expect(t, has_message(&g, "finds nothing"))
}

@(test)
abandoned_house_is_suppressed_while_holding_its_loot :: proc(t: ^testing.T) {
	g := new_test_game()
	items_add(&g.inventory, Item{kind = .Destroyed_Printer})
	spawn_event(&g, .Abandoned_House)
	testing.expect_value(t, g.feature_count, 0)
}

@(test)
car_keys_are_never_duplicated :: proc(t: ^testing.T) {
	g := new_test_game()
	items_add(&g.inventory, Item{kind = .Car_Keys})
	spawn_event(&g, .Car_Keys)
	testing.expect_value(t, g.ground.count, 0)
}

@(test)
cat_shrine_prayer_leaves_a_hairball :: proc(t: ^testing.T) {
	g := new_test_game()
	spawn_event(&g, .Cat_Shrine)
	perform(&g, .Pray, 0)
	testing.expect_value(t, items_count(&g.ground, .Hair_Ball), 1)
}

@(test)
whole_driving_chain :: proc(t: ^testing.T) {
	g := new_test_game(9)
	counter_set(&g, .Health, 100)

	// The DMV offers a handbook and a test.
	spawn_event(&g, .Dmv)
	testing.expect(t, verb_enabled(&g, .Pick_Up_Handbook, 0))
	perform(&g, .Pick_Up_Handbook, 0)
	testing.expect(t, has_item(&g, .Handbook))
	testing.expect(t, !verb_enabled(&g, .Pick_Up_Handbook, 0))

	// Study to the maximum; every study costs hunger and fatigue.
	book := items_find(&g.inventory, .Handbook)
	for _ in 0 ..< 10 {
		perform(&g, .Study_Handbook, book)
	}
	testing.expect_value(t, counter(&g, .Study), 10)
	testing.expect(t, !verb_enabled(&g, .Study_Handbook, book))
	testing.expect_value(t, counter(&g, .Fatigue), 10)

	// Studied to 10, the 1d10 test cannot fail.
	perform(&g, .Take_Permit_Test, 0)
	testing.expect(t, has_item(&g, .Learners_Permit))
	testing.expect(t, !verb_enabled(&g, .Take_Permit_Test, 0))

	// Lessons need an instructor and 3 jools each.
	spawn_event(&g, .Instructor)
	testing.expect(t, !verb_enabled(&g, .Take_Lesson, 0))
	counter_set(&g, .Jools, 30)
	for i in 1 ..= 10 {
		testing.expect(t, verb_enabled(&g, .Take_Lesson, 0))
		perform(&g, .Take_Lesson, 0)
		testing.expect_value(t, counter(&g, .Driving_Instruction), i)
	}
	testing.expect_value(t, counter(&g, .Jools), 0)
	testing.expect(t, !has_item(&g, .Learners_Permit))
	testing.expect(t, has_item(&g, .Drivers_License))
	testing.expect(t, !verb_enabled(&g, .Take_Lesson, 0))

	// Keys plus license plus a car: one drive, keys used up.
	g.feature_count = 0
	spawn_event(&g, .Car)
	testing.expect(t, !verb_enabled(&g, .Drive, 0))
	items_add(&g.inventory, Item{kind = .Car_Keys})
	testing.expect(t, verb_enabled(&g, .Drive, 0))
	perform(&g, .Drive, 0)
	testing.expect(t, !has_item(&g, .Car_Keys))
	miles := 2000 - counter(&g, .Distance_Remaining)
	testing.expect(t, miles >= 5 && miles <= 30)
}

@(test)
failing_the_test_with_no_study_is_certain :: proc(t: ^testing.T) {
	for seed in 1 ..= u64(30) {
		g := new_test_game(seed)
		spawn_event(&g, .Dmv)
		perform(&g, .Take_Permit_Test, 0)
		testing.expect(t, !has_item(&g, .Learners_Permit))
	}
}

@(test)
take_and_drop_move_items :: proc(t: ^testing.T) {
	g := new_test_game()
	for _ in 0 ..< 3 {
		items_add(&g.ground, Item{kind = .Flower})
	}
	take_from_ground(&g, .Flower, 2)
	testing.expect_value(t, items_count(&g.inventory, .Flower), 2)
	testing.expect_value(t, items_count(&g.ground, .Flower), 1)
	drop_item(&g, .Flower, 5) // only 2 to drop
	testing.expect_value(t, items_count(&g.inventory, .Flower), 0)
	testing.expect_value(t, items_count(&g.ground, .Flower), 3)
}

@(test)
ad_break_counts_down_then_pays_a_coupon :: proc(t: ^testing.T) {
	g := new_test_game()
	ad_start(&g, 1000)
	testing.expect(t, in_ad(&g))
	ad_show(&g, 1000 + 61_000)
	testing.expect(t, in_ad(&g))
	testing.expect(t, has_message(&g, "Time left in ad break: 00:59"))
	testing.expect(t, g.messages[g.message_count - 1].link == .Umlaut)
	ad_show(&g, 1000 + AD_DURATION_MS)
	testing.expect(t, !in_ad(&g))
	testing.expect(t, has_item(&g, .Coupon))
}

@(test)
message_log_drops_the_oldest :: proc(t: ^testing.T) {
	g: Game
	for i in 0 ..< MAX_MESSAGES + 5 {
		say(&g, "line %d", i)
	}
	testing.expect_value(t, g.message_count, MAX_MESSAGES)
	testing.expect_value(t, message_text(&g.messages[0]), "line 5")
}

@(test)
long_flavour_text_fits_a_message :: proc(t: ^testing.T) {
	g := new_test_game()
	for k in Item_Kind {
		testing.expect(t, len(item_flavors[k]) < MESSAGE_CAP)
	}
	for v in Verb {
		testing.expect(t, len(verb_flavors[v]) < MESSAGE_CAP)
	}
}

@(test)
same_seed_same_journey :: proc(t: ^testing.T) {
	run :: proc() -> (miles, health: int) {
		g := new_test_game(77)
		for _ in 0 ..< 200 {
			if is_dead(&g) {
				break
			}
			if verb_enabled(&g, .Eat_Snax) && counter(&g, .Stomach) == 0 {
				perform(&g, .Eat_Snax)
			}
			perform(&g, .Continue_Journey)
		}
		return counter(&g, .Distance_Remaining), counter(&g, .Health)
	}
	m1, h1 := run()
	m2, h2 := run()
	testing.expect_value(t, m1, m2)
	testing.expect_value(t, h1, h2)
}

@(test)
walking_without_food_eventually_kills :: proc(t: ^testing.T) {
	g := new_test_game(5)
	counter_set(&g, .Pace, 5)
	for i in 0 ..< 1000 {
		if is_dead(&g) {
			break
		}
		perform(&g, .Continue_Journey)
	}
	testing.expect(t, is_dead(&g) || counter(&g, .Distance_Remaining) == 0)
}
