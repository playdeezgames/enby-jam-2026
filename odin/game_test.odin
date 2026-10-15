#+build !js
package gwen

import "core:testing"

@(test)
rng_is_deterministic :: proc(t: ^testing.T) {
	a, b: Rng
	rng_seed(&a, 42)
	rng_seed(&b, 42)
	for _ in 0 ..< 100 {
		testing.expect_value(t, rng_next(&a), rng_next(&b))
	}
}

@(test)
dice_stay_in_range :: proc(t: ^testing.T) {
	r: Rng
	rng_seed(&r, 7)
	for _ in 0 ..< 1000 {
		v := roll_dice(&r, 2, 6)
		testing.expect(t, v >= 2 && v <= 12)
		w := roll_dice(&r, -2, 6)
		testing.expect(t, w >= -12 && w <= -2)
	}
}

@(test)
range_hits_both_ends :: proc(t: ^testing.T) {
	r: Rng
	rng_seed(&r, 1)
	lo, hi := false, false
	for _ in 0 ..< 1000 {
		v := rng_range(&r, 1, 4)
		testing.expect(t, v >= 1 && v <= 4)
		lo ||= v == 1
		hi ||= v == 4
	}
	testing.expect(t, lo && hi)
}

@(test)
weighted_respects_zero_weights :: proc(t: ^testing.T) {
	r: Rng
	rng_seed(&r, 3)
	weights := []int{0, 5, 0, 5}
	for _ in 0 ..< 500 {
		i := rng_weighted(&r, weights)
		testing.expect(t, i == 1 || i == 3)
	}
}

@(test)
draw_fits_grid_and_shows_title :: proc(t: ^testing.T) {
	g: Game
	grid: Grid
	game_init(&g, 1, 0)
	game_draw(&g, &grid)
	// Title is centred on row 3.
	found := false
	for c in grid[3] {
		if c.ch == 'G' {
			found = true
		}
	}
	testing.expect(t, found)
}
