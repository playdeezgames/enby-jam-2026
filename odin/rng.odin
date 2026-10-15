package gwen

// Small deterministic generator (splitmix64) so tests can seed it and the
// wasm build never depends on the platform's default RNG.
Rng :: struct {
	state: u64,
}

rng_seed :: proc(r: ^Rng, seed: u64) {
	r.state = seed
}

rng_next :: proc(r: ^Rng) -> u64 {
	r.state += 0x9E3779B97F4A7C15
	z := r.state
	z = (z ~ (z >> 30)) * 0xBF58476D1CE4E5B9
	z = (z ~ (z >> 27)) * 0x94D049BB133111EB
	return z ~ (z >> 31)
}

// Uniform integer in [lo, hi], inclusive. Same semantics as the VB RNG.FromRange.
rng_range :: proc(r: ^Rng, lo, hi: int) -> int {
	if hi <= lo {
		return lo
	}
	span := u64(hi - lo + 1)
	return lo + int(rng_next(r) % span)
}

// count dice of the given size; a negative count subtracts (VB "2d6+-2d6").
roll_dice :: proc(r: ^Rng, count, size: int) -> int {
	total := 0
	for _ in 0 ..< abs(count) {
		total += rng_range(r, 1, size)
	}
	return count < 0 ? -total : total
}

// Weighted pick over a table of integer weights; returns the chosen index.
rng_weighted :: proc(r: ^Rng, weights: []int) -> int {
	sum := 0
	for w in weights {
		sum += w
	}
	if sum <= 0 {
		return 0
	}
	roll := int(rng_next(r) % u64(sum))
	for w, i in weights {
		roll -= w
		if roll < 0 {
			return i
		}
	}
	return len(weights) - 1
}
