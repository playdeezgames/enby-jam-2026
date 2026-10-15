package gwen

import "core:fmt"

// Pure rules of Gwen's Peregrination II, ported from the VB.NET game.
// No platform imports: the clock is an argument, the RNG lives in the Game.
// Everything is plain data (no pointers, no allocations) so saving is a straight copy.

NAME :: "Gwen"
PRONOUNS :: "They/Them"

INF :: 1 << 30
ITEM_CAP :: 48
MAX_FEATURES :: 8
MAX_CHARACTERS :: 4
MAX_MESSAGES :: 48
MESSAGE_CAP :: 400
AD_DURATION_MS :: 2 * 60 * 1000
LESSON_COST :: 3

Counter :: enum {
	Health,
	Satiety,
	Stomach,
	Bewwy_Hertz,
	Littering,
	Recycling,
	Driving_Instruction,
	Jools,
	Fatigue,
	Snax,
	Distance_Remaining,
	Pace,
	Forage_Skill,
	Forage_Experience,
	Study,
}

Counter_State :: struct {
	value, min, max: int,
}

// {start, min, max}
counter_defaults := [Counter][3]int {
	.Health              = {100, 0, 100},
	.Satiety             = {100, 0, 100},
	.Stomach             = {0, 0, 50},
	.Bewwy_Hertz         = {0, 0, INF},
	.Littering           = {0, 0, INF},
	.Recycling           = {0, 0, INF},
	.Driving_Instruction = {0, 0, 10},
	.Jools               = {0, 0, INF},
	.Fatigue             = {0, 0, 100},
	.Snax                = {10, 0, INF},
	.Distance_Remaining  = {2000, 0, INF},
	.Pace                = {3, 1, 5},
	.Forage_Skill        = {0, 0, INF},
	.Forage_Experience   = {0, 0, 10},
	.Study               = {0, 0, 10},
}

Item_Kind :: enum {
	Car_Keys,
	Coupon,
	Destroyed_Printer,
	Dr_Pepper,
	Flower,
	Hair_Ball,
	Litter,
	Pkastic_Bag,
	Handbook,
	Learners_Permit,
	Drivers_License,
}

Item :: struct {
	kind: Item_Kind,
	snax: int, // only used by the Pkastic Bag
}

Items :: struct {
	count: int,
	items: [ITEM_CAP]Item,
}

Feature_Kind :: enum {
	Abandoned_House,
	Car,
	Cat_Shrine,
	Dishes,
	Flower_Patch,
	Kwik_Trip,
	Recycle_Bin,
	Vending_Machine,
	Dmv,
}

Feature :: struct {
	kind:      Feature_Kind,
	remaining: int, // flowers left in a patch
	jools:     int, // reward for the dishes
	cat:       int, // index into CAT_NAMES
	items:     Items,
}

Character_Kind :: enum {
	Traehi,
	Instructor,
}

Link :: enum {
	None,
	Umlaut,
	Steam_Smell,
}

Message :: struct {
	text: [MESSAGE_CAP]u8,
	len:  int,
	link: Link,
}

Game :: struct {
	rng:                 Rng,
	embarked:            bool,
	counters:            [Counter]Counter_State,
	journey_complete:    bool,
	changing_pace:       bool,
	inventory:           Items,
	ground:              Items,
	features:            [MAX_FEATURES]Feature,
	feature_count:       int,
	characters:          [MAX_CHARACTERS]Character_Kind,
	character_count:     int,
	foraging_difficulty: int,
	shortcut:            bool,
	ad_finish_ms:        f64, // 0 means no ad break
	messages:            [MAX_MESSAGES]Message,
	message_count:       int,
}

CAT_NAMES := [3]string{"Captain Jack", "Nina", "Körmy"}

// ---------------------------------------------------------------- messages

message_text :: proc(m: ^Message) -> string {
	return string(m.text[:m.len])
}

clear_messages :: proc(g: ^Game) {
	g.message_count = 0
}

@(private = "file")
next_message :: proc(g: ^Game) -> ^Message {
	if g.message_count == MAX_MESSAGES {
		copy(g.messages[:], g.messages[1:])
		g.message_count -= 1
	}
	m := &g.messages[g.message_count]
	g.message_count += 1
	m.len = 0
	m.link = .None
	return m
}

say :: proc(g: ^Game, format: string, args: ..any) {
	m := next_message(g)
	s := fmt.bprintf(m.text[:], format, ..args)
	m.len = len(s)
}

say_link :: proc(g: ^Game, text: string, link: Link) {
	m := next_message(g)
	n := copy(m.text[:], text)
	m.len = n
	m.link = link
}

// ---------------------------------------------------------------- counters

counter :: proc(g: ^Game, c: Counter) -> int {
	return g.counters[c].value
}

counter_set :: proc(g: ^Game, c: Counter, v: int) {
	s := &g.counters[c]
	s.value = clamp(v, s.min, s.max)
}

counter_add :: proc(g: ^Game, c: Counter, delta: int) {
	counter_set(g, c, g.counters[c].value + delta)
}

counter_at_min :: proc(g: ^Game, c: Counter) -> bool {
	return g.counters[c].value <= g.counters[c].min
}

counter_at_max :: proc(g: ^Game, c: Counter) -> bool {
	return g.counters[c].value >= g.counters[c].max
}

is_dead :: proc(g: ^Game) -> bool {
	return counter_at_min(g, .Health)
}

is_done :: proc(g: ^Game) -> bool {
	return g.journey_complete || is_dead(g)
}

in_ad :: proc(g: ^Game) -> bool {
	return g.ad_finish_ms != 0
}

// ---------------------------------------------------------------- item lists

items_add :: proc(l: ^Items, item: Item) -> bool {
	if l.count >= ITEM_CAP {
		return false
	}
	l.items[l.count] = item
	l.count += 1
	return true
}

items_remove :: proc(l: ^Items, index: int) {
	if index < 0 || index >= l.count {
		return
	}
	copy(l.items[index:], l.items[index + 1:l.count])
	l.count -= 1
}

items_find :: proc(l: ^Items, kind: Item_Kind) -> int {
	for i in 0 ..< l.count {
		if l.items[i].kind == kind {
			return i
		}
	}
	return -1
}

items_count :: proc(l: ^Items, kind: Item_Kind) -> int {
	n := 0
	for i in 0 ..< l.count {
		if l.items[i].kind == kind {
			n += 1
		}
	}
	return n
}

has_item :: proc(g: ^Game, kind: Item_Kind) -> bool {
	return items_find(&g.inventory, kind) >= 0
}

item_names := [Item_Kind]string {
	.Car_Keys          = "Car Keys",
	.Coupon            = "Coupon",
	.Destroyed_Printer = "Destroyed Printer",
	.Dr_Pepper         = "Big Buddy with Dr Pepper In It",
	.Flower            = "Flower",
	.Hair_Ball         = "Hairball",
	.Litter            = "Litter",
	.Pkastic_Bag       = "Pkastic Bag",
	.Handbook          = "Driver's Handbook",
	.Learners_Permit   = "Learner's Permit",
	.Drivers_License   = "Driver's License",
}

item_flavors := [Item_Kind]string {
	.Car_Keys          = "These are car keys. They weren't where you'd thought they'd be!",
	.Coupon            = "This is a coupon.",
	.Destroyed_Printer = "This printer has been smashed to smithereens. It will never work again. It is an ex-printer.",
	.Dr_Pepper         = "This is plastic cup filled with Dr Pepper. It is capped with a plastic cap, and has a plastic straw in it.",
	.Flower            = "This is a flower. You picked it from a flower patch. Murderer.",
	.Hair_Ball         = "This is the hairy vomit of a cat. It is hairy because of how cats clean themselves with their tongue. It is gross. I don't know why you'd want to have this in yer inventory.",
	.Litter            = "This is litter. Don't drop it on the ground and leave it there, or yer a bad person!",
	.Pkastic_Bag       = "This is a bag. It is made of pkastic. No, don't ask me what pkastic is, just look at the 'K' key and notice how it is right next to the 'L' key.",
	.Handbook          = "This is the state driver's handbook. It is thick, dull and full of signs shaped like things. Studying it is the first step to a learner's permit.",
	.Learners_Permit   = "This is a learner's permit. It says you may drive, as long as an instructor is within shouting distance.",
	.Drivers_License   = "This is a driver's license. It has a photo of you looking startled. You are, officially, a driver.",
}

// ---------------------------------------------------------------- verbs

Verb :: enum {
	Continue_Journey,
	Complete_Journey,
	Change_Pace,
	Eat_Snax,
	Take_Nap,
	Forage,
	Take_Shortcut,
	Give_Flower,
	Take_Lesson,
	Pick_Flower,
	Buy_Snax,
	Buy_Dr_Pepper,
	Recycle_Litter,
	Wash_Dishes,
	Drive,
	Pray,
	Pick_Up_Handbook,
	Take_Permit_Test,
	Stop_And_Smell,
	Drink,
	Reach_In,
	Study_Handbook,
}

verb_names := [Verb]string {
	.Continue_Journey = "Continue Journey...",
	.Complete_Journey = "Complete Journey",
	.Change_Pace      = "Change Pace...",
	.Eat_Snax         = "Eat Snax",
	.Take_Nap         = "Take Nap",
	.Forage           = "Forage",
	.Take_Shortcut    = "Take Shortcut...",
	.Give_Flower      = "Give Flower",
	.Take_Lesson      = "Take Lesson(3 jools)",
	.Pick_Flower      = "Pick a Flower",
	.Buy_Snax         = "Buy Snax(1 jools)",
	.Buy_Dr_Pepper    = "Buy Dr Pepper",
	.Recycle_Litter   = "Recycle Litter",
	.Wash_Dishes      = "Wash Dishes",
	.Drive            = "Drive",
	.Pray             = "Pray",
	.Pick_Up_Handbook = "Pick Up Handbook",
	.Take_Permit_Test = "Take Permit Test",
	.Stop_And_Smell   = "Stop and Smell",
	.Drink            = "Drink",
	.Reach_In         = "Reach In...",
	.Study_Handbook   = "Study Handbook",
}

verb_flavors := [Verb]string {
	.Continue_Journey = "The road goes ever on and on.",
	.Complete_Journey = "It is a privilege to see something end. You then get to either remember it fondly, or be glad that it's over. Sometimes, it's a bit of both. Life is messy that way.",
	.Change_Pace      = "New York City? (Get a rope!)",
	.Eat_Snax         = "Snax are delicious!",
	.Take_Nap         = "ZZZZZZZZZZZZZZZ.",
	.Forage           = "You scrounge for resources.",
	.Take_Shortcut    = "No guts, no glory!",
	.Give_Flower      = "You give them a flower. Isn't that nice?",
	.Take_Lesson      = "You hand over the jools and buckle up. The instructor grips the dashboard-mounted second brake pedal with the calm of a person who has seen things.",
	.Pick_Flower      = "You use your plant murdering hand to pluck a defenseless flower from the flower patch. You do not hear the wailing lament of the other flowers in response to the death of one of its brethren.",
	.Buy_Snax         = "You put the jools into the jools receptacle and press the `Snax` button....",
	.Buy_Dr_Pepper    = "You pour yerself a Dr Pepper from the fountain. You can barely contain yer exicitement!",
	.Recycle_Litter   = "You put litter into the bin and feel better about yerself.",
	.Wash_Dishes      = "You wash the stinking, disgusting dishes, dry them, and put them away.",
	.Drive            = "You stick the key in, fire it up, and drive it.",
	.Pray             = "You stand reverently quiet for a moment, contemplating.",
	.Pick_Up_Handbook = "You take a handbook from the rack. The rack is mostly handbooks. It is that kind of place.",
	.Take_Permit_Test = "You take a number, wait, and are handed a pencil that is mostly eraser.",
	.Stop_And_Smell   = "Sometimes it is a good idea to take a break for moment, and enjoy the simpler things in life, like flowers that you horribly massacred and sniffing their floral corpses. Sicko.",
	.Drink            = "You drink the beverage!",
	.Reach_In         = "You steel yer nerves and reach in to the pkastic bag, knowing the developer to be a devious person.",
	.Study_Handbook   = "You open the handbook and read about right of way until the words stop meaning things.",
}

avatar_verbs := [?]Verb{.Continue_Journey, .Complete_Journey, .Change_Pace, .Eat_Snax, .Take_Nap, .Forage}

feature_verbs := [Feature_Kind][]Verb {
	.Abandoned_House = []Verb{},
	.Car             = []Verb{.Drive},
	.Cat_Shrine      = []Verb{.Pray},
	.Dishes          = []Verb{.Wash_Dishes},
	.Flower_Patch    = []Verb{.Pick_Flower},
	.Kwik_Trip       = []Verb{.Buy_Dr_Pepper},
	.Recycle_Bin     = []Verb{.Recycle_Litter},
	.Vending_Machine = []Verb{.Buy_Snax},
	.Dmv             = []Verb{.Pick_Up_Handbook, .Take_Permit_Test},
}

character_verbs := [Character_Kind][]Verb {
	.Traehi     = []Verb{.Give_Flower},
	.Instructor = []Verb{.Take_Lesson},
}

item_verbs := [Item_Kind][]Verb {
	.Car_Keys          = []Verb{},
	.Coupon            = []Verb{},
	.Destroyed_Printer = []Verb{},
	.Dr_Pepper         = []Verb{.Drink},
	.Flower            = []Verb{.Stop_And_Smell},
	.Hair_Ball         = []Verb{},
	.Litter            = []Verb{},
	.Pkastic_Bag       = []Verb{.Reach_In},
	.Handbook          = []Verb{.Study_Handbook},
	.Learners_Permit   = []Verb{},
	.Drivers_License   = []Verb{},
}

feature_names := [Feature_Kind]string {
	.Abandoned_House = "Abandoned House",
	.Car             = "Car",
	.Cat_Shrine      = "Cat Shrine",
	.Dishes          = "Sink Full of Dirty Dishes",
	.Flower_Patch    = "Flower Patch",
	.Kwik_Trip       = "Kwik Trip",
	.Recycle_Bin     = "Recycle Bin",
	.Vending_Machine = "Vending Machine",
	.Dmv             = "DMV",
}

character_names := [Character_Kind]string {
	.Traehi     = "Traehi",
	.Instructor = "Driving Instructor",
}

character_flavors := [Character_Kind]string {
	.Traehi     = "This person obviously likes flowers. Their clothing has a floral pattern. They have flowers in their hair. They a holding a basket of fresh cut flowers.",
	.Instructor = "This person wears a windbreaker, a whistle and the thousand-yard stare of someone who has been in a lot of passenger seats. They will teach you to drive, for a fee.",
}

feature_is :: proc(g: ^Game, index: int, kind: Feature_Kind) -> bool {
	return index >= 0 && index < g.feature_count && g.features[index].kind == kind
}

item_is :: proc(g: ^Game, index: int, kind: Item_Kind) -> bool {
	return index >= 0 && index < g.inventory.count && g.inventory.items[index].kind == kind
}

character_is :: proc(g: ^Game, index: int, kind: Character_Kind) -> bool {
	return index >= 0 && index < g.character_count && g.characters[index] == kind
}

// Whether the verb can be performed now. target is the inventory index (item verbs),
// feature index (feature verbs) or character index (character verbs); ignored otherwise.
verb_enabled :: proc(g: ^Game, v: Verb, target: int = 0) -> bool {
	switch v {
	case .Continue_Journey, .Change_Pace:
		return !is_dead(g)
	case .Complete_Journey:
		return !is_dead(g) && counter_at_min(g, .Distance_Remaining)
	case .Eat_Snax:
		return !is_dead(g) && counter(g, .Snax) > 0
	case .Take_Nap:
		return counter(g, .Fatigue) >= g.counters[.Fatigue].max / 2
	case .Forage:
		return true
	case .Take_Shortcut:
		return g.shortcut
	case .Give_Flower:
		return character_is(g, target, .Traehi) && has_item(g, .Flower)
	case .Take_Lesson:
		ready := has_item(g, .Learners_Permit) && !counter_at_max(g, .Driving_Instruction)
		return character_is(g, target, .Instructor) && ready && counter(g, .Jools) >= LESSON_COST
	case .Pick_Flower:
		return feature_is(g, target, .Flower_Patch) && g.features[target].remaining > 0
	case .Buy_Snax:
		return feature_is(g, target, .Vending_Machine) && counter(g, .Jools) > 0
	case .Buy_Dr_Pepper:
		return feature_is(g, target, .Kwik_Trip) && counter(g, .Jools) > 0
	case .Recycle_Litter:
		return feature_is(g, target, .Recycle_Bin) && has_item(g, .Litter)
	case .Wash_Dishes:
		return feature_is(g, target, .Dishes)
	case .Drive:
		return feature_is(g, target, .Car) && has_item(g, .Car_Keys) && has_item(g, .Drivers_License)
	case .Pray:
		return feature_is(g, target, .Cat_Shrine)
	case .Pick_Up_Handbook:
		no_papers := !has_item(g, .Learners_Permit) && !has_item(g, .Drivers_License)
		return feature_is(g, target, .Dmv) && !has_item(g, .Handbook) && no_papers
	case .Take_Permit_Test:
		no_papers := !has_item(g, .Learners_Permit) && !has_item(g, .Drivers_License)
		return feature_is(g, target, .Dmv) && no_papers
	case .Stop_And_Smell:
		return item_is(g, target, .Flower)
	case .Drink:
		return item_is(g, target, .Dr_Pepper)
	case .Reach_In:
		return item_is(g, target, .Pkastic_Bag)
	case .Study_Handbook:
		return item_is(g, target, .Handbook) && !counter_at_max(g, .Study)
	}
	return false
}

// ---------------------------------------------------------------- hunger, fatigue, damage

apply_damage :: proc(g: ^Game, damage: int) {
	say(g, "%s loses %d health.", NAME, damage)
	counter_add(g, .Health, -damage)
	if is_dead(g) {
		say(g, "%s is dead.", NAME)
	} else {
		say(g, "%s now has %d/%d health.", NAME, counter(g, .Health), g.counters[.Health].max)
	}
}

apply_fatigue :: proc(g: ^Game, fatigue: int) {
	capacity := min(g.counters[.Fatigue].max - counter(g, .Fatigue), fatigue)
	overage := fatigue - capacity
	if capacity > 0 {
		counter_add(g, .Fatigue, fatigue)
		say(g, "%s gains %d fatigue.", NAME, capacity)
		say(g, "%s now has %d/%d fatigue.", NAME, counter(g, .Fatigue), g.counters[.Fatigue].max)
	}
	if overage > 0 {
		apply_damage(g, overage)
	}
}

apply_hunger :: proc(g: ^Game, hunger: int) {
	hunger := hunger
	stomach := min(hunger, counter(g, .Stomach))
	hunger -= stomach
	counter_add(g, .Stomach, -stomach)
	satiety := min(hunger, counter(g, .Satiety))
	if satiety > 0 {
		hunger -= satiety
		say(g, "%s loses %d satiety.", NAME, satiety)
		counter_add(g, .Satiety, -satiety)
		say(g, "%s now has %d/%d satiety.", NAME, counter(g, .Satiety), g.counters[.Satiety].max)
	} else if counter(g, .Stomach) > 0 {
		if !counter_at_max(g, .Satiety) {
			say(g, "%s gains 1 satiety.", NAME)
			counter_add(g, .Satiety, 1)
			say(g, "%s now has %d/%d satiety.", NAME, counter(g, .Satiety), g.counters[.Satiety].max)
		}
	}
	if hunger > 0 {
		apply_damage(g, hunger)
	}
}

earn_foraging_experience :: proc(g: ^Game, xp: int) {
	say(g, "%s earns %d foraging xp.", NAME, xp)
	counter_add(g, .Forage_Experience, xp)
	if counter_at_max(g, .Forage_Experience) {
		counter_add(g, .Forage_Skill, 1)
		say(g, "%s's foraging skill goes up to %d!", NAME, counter(g, .Forage_Skill))
		counter_set(g, .Forage_Experience, 0)
		g.counters[.Forage_Experience].max *= 2
	} else {
		say(g, "%s's foraging xp is %d/%d.", NAME, counter(g, .Forage_Experience), g.counters[.Forage_Experience].max)
	}
}

effective_pace :: proc(g: ^Game) -> int {
	pace := counter(g, .Pace)
	if counter(g, .Bewwy_Hertz) > 0 {
		pace = (pace + g.counters[.Pace].min) / 2
	}
	return pace
}

// ---------------------------------------------------------------- look and status

look :: proc(g: ^Game) {
	say(g, "Yer on yer journey.")
	say(g, "Distance Remaining: %d.", counter(g, .Distance_Remaining))
	say(g, "Pace: %d.", counter(g, .Pace))
	if g.character_count > 0 {
		say(g, "Characters:")
		for i in 0 ..< g.character_count {
			say(g, "- %s", character_names[g.characters[i]])
		}
	}
	if g.feature_count > 0 {
		say(g, "Features:")
		for i in 0 ..< g.feature_count {
			say(g, "- %s", feature_names[g.features[i].kind])
		}
	}
	say(g, "Foraging Difficulty: %d", g.foraging_difficulty)
	if g.ground.count > 0 {
		say(g, "There are items on the ground.")
	}
}

look_action :: proc(g: ^Game) {
	clear_messages(g)
	look(g)
}

status :: proc(g: ^Game) {
	clear_messages(g)
	say(g, "%s's Status:", NAME)
	say(g, "%s's pronouns are they/them. Knowing Finnish won't help here.", NAME)
	if counter(g, .Bewwy_Hertz) > 0 {
		say(g, "%s bewwy hertz.", NAME)
	}
	say(g, "Distance Remaining: %d.", counter(g, .Distance_Remaining))
	say(g, "Pace: %d.", counter(g, .Pace))
	say(g, "- Health: %d/%d", counter(g, .Health), g.counters[.Health].max)
	say(g, "- Satiety: %d/%d", counter(g, .Satiety), g.counters[.Satiety].max)
	say(g, "- Fatigue: %d/%d", counter(g, .Fatigue), g.counters[.Fatigue].max)
	if counter(g, .Stomach) > 0 {
		say(g, "- Stomach: %d/%d", counter(g, .Stomach), g.counters[.Stomach].max)
	}
	if counter(g, .Snax) > 0 {
		say(g, "- Snax: %d", counter(g, .Snax))
	}
	if counter(g, .Study) > 0 && !has_item(g, .Learners_Permit) && !has_item(g, .Drivers_License) {
		say(g, "- Studied: %d/%d", counter(g, .Study), g.counters[.Study].max)
	}
	if counter(g, .Driving_Instruction) > 0 {
		say(g, "- Driving Instruction: %d/%d", counter(g, .Driving_Instruction), g.counters[.Driving_Instruction].max)
	}
	if counter(g, .Littering) > 0 {
		say(g, "- Littering: %d", counter(g, .Littering))
	}
	if counter(g, .Recycling) > 0 {
		say(g, "- Recycling: %d", counter(g, .Recycling))
	}
	say(g, "- Jools: %d", counter(g, .Jools))
}

// ---------------------------------------------------------------- world setup and update

game_reset :: proc(g: ^Game) {
	rng := g.rng
	g^ = {}
	g.rng = rng
	for c in Counter {
		d := counter_defaults[c]
		g.counters[c] = Counter_State{d[0], d[1], d[2]}
	}
}

embark :: proc(g: ^Game) {
	game_reset(g)
	g.embarked = true
	roll_foraging_difficulty(g)
	say(g, "So it begins!")
	look(g)
}

abandon :: proc(g: ^Game) {
	game_reset(g)
}

roll_foraging_difficulty :: proc(g: ^Game) {
	g.foraging_difficulty = roll_dice(&g.rng, 2, 10)
}

// What happens when the avatar moves on: the place is rebuilt from scratch.
update_location :: proc(g: ^Game) {
	roll_foraging_difficulty(g)
	g.character_count = 0
	g.feature_count = 0
	for i in 0 ..< g.ground.count {
		if g.ground.items[i].kind == .Litter {
			counter_add(g, .Littering, 1)
		}
	}
	g.ground.count = 0
	g.shortcut = false
	spawn_event(g, pick_event(g))
}

Event :: enum {
	Shortcut,
	Flower_Patch,
	Traehi,
	Vending_Machine,
	Abandoned_House,
	Cat_Shrine,
	Kwik_Trip,
	Recycle_Bin,
	Dishes,
	Car_Keys,
	Car,
	Dmv,
	Instructor,
	Nothing,
}

event_weights := [Event]int {
	.Shortcut        = 100,
	.Flower_Patch    = 100,
	.Traehi          = 50,
	.Vending_Machine = 50,
	.Abandoned_House = 10,
	.Cat_Shrine      = 10,
	.Kwik_Trip       = 10,
	.Recycle_Bin     = 10,
	.Dishes          = 100,
	.Car_Keys        = 1,
	.Car             = 10,
	.Dmv             = 10,
	.Instructor      = 20,
	.Nothing         = 1000,
}

pick_event :: proc(g: ^Game) -> Event {
	w: [len(Event)]int
	for e in Event {
		w[int(e)] = event_weights[e]
	}
	return Event(rng_weighted(&g.rng, w[:]))
}

@(private = "file")
add_feature :: proc(g: ^Game, kind: Feature_Kind) -> ^Feature {
	if g.feature_count >= MAX_FEATURES {
		return nil
	}
	f := &g.features[g.feature_count]
	g.feature_count += 1
	f^ = {}
	f.kind = kind
	return f
}

@(private = "file")
add_character :: proc(g: ^Game, kind: Character_Kind) {
	if g.character_count < MAX_CHARACTERS {
		g.characters[g.character_count] = kind
		g.character_count += 1
	}
}

spawn_event :: proc(g: ^Game, e: Event) {
	switch e {
	case .Shortcut:
		say(g, "%s discovers a potential shortcut!", NAME)
		g.shortcut = true
	case .Flower_Patch:
		say(g, "%s discovers a patch of wild flower!", NAME)
		if f := add_feature(g, .Flower_Patch); f != nil {
			f.remaining = clamp(roll_dice(&g.rng, 3, 4), 0, 12)
		}
	case .Traehi:
		add_character(g, .Traehi)
		say(g, "%s encounters Traehi, the well-known lover of all things floral!", NAME)
	case .Vending_Machine:
		add_feature(g, .Vending_Machine)
		say(g, "%s finds a Vending Machine.", NAME)
	case .Abandoned_House:
		if has_item(g, .Destroyed_Printer) || has_item(g, .Pkastic_Bag) {
			spawn_event(g, .Nothing)
			return
		}
		if f := add_feature(g, .Abandoned_House); f != nil {
			items_add(&f.items, Item{kind = .Destroyed_Printer})
			items_add(&f.items, Item{kind = .Pkastic_Bag, snax = clamp(roll_dice(&g.rng, 2, 6), 0, 12)})
		}
		say(g, "%s finds an Abandoned House.", NAME)
	case .Cat_Shrine:
		if f := add_feature(g, .Cat_Shrine); f != nil {
			f.cat = rng_range(&g.rng, 0, len(CAT_NAMES) - 1)
		}
		say(g, "%s finds a Cat Shrine.", NAME)
	case .Kwik_Trip:
		add_feature(g, .Kwik_Trip)
		say(g, "%s finds a Kwik Trip.", NAME)
	case .Recycle_Bin:
		add_feature(g, .Recycle_Bin)
		say(g, "%s finds a Recycle Bin.", NAME)
	case .Dishes:
		if f := add_feature(g, .Dishes); f != nil {
			f.jools = roll_dice(&g.rng, 2, 6)
		}
		say(g, "%s finds a Sink Full of Dirty Dishes.", NAME)
	case .Car_Keys:
		if has_item(g, .Car_Keys) {
			spawn_event(g, .Nothing)
			return
		}
		items_add(&g.ground, Item{kind = .Car_Keys})
		say(g, "%s sees Car Keys on the ground.", NAME)
	case .Car:
		add_feature(g, .Car)
		say(g, "%s finds Car.", NAME)
	case .Dmv:
		add_feature(g, .Dmv)
		say(g, "%s finds a DMV.", NAME)
	case .Instructor:
		add_character(g, .Instructor)
		say(g, "%s encounters a Driving Instructor, who is looking for someone to be afraid of.", NAME)
	case .Nothing:
		say(g, "Nothing eventful occurs.")
	}
}

// Descriptions that depend on the feature's own data.
feature_flavor :: proc(g: ^Game, f: ^Feature) {
	switch f.kind {
	case .Abandoned_House:
		say(g, "This house is abandoned. The lawn is overgrown. The doors have been ripped off of the hinges, and the windows are made of sheet goods.")
	case .Car:
		say(g, "This is a car. Assuming you have keys to it and a driver's license, you can drive it.")
	case .Cat_Shrine:
		say(g, "This is the shrine to a cat. Specifically, this is a shrine for a cat named `%s`.", CAT_NAMES[f.cat])
	case .Dishes:
		say(g, "This is a sink full to the brim with dirty dishes. They didn't even rinse them. However, if you wash them, you will get %d jools.", f.jools)
	case .Flower_Patch:
		say(g, "This is patch of flowers. Which means that it is an area of ground that has flowers growing from it.")
	case .Kwik_Trip:
		say(g, "This is a Kwik Trip, a convenience store common in the midwestern US that sell gasoline/petrol and more importantly has a fountain drink dispenser that dispenses Dr Pepper, the elixer of the gods.")
	case .Recycle_Bin:
		say(g, "This is a recycle bin. You place litter in here, so that you are not a bad person.")
	case .Vending_Machine:
		say(g, "This is a vending machine that sells snax in exchange for jools. I don't know what it is doing standing out here in the middle of nowhere, either.")
	case .Dmv:
		say(g, "This is the Department of Motor Vehicles. There is a line. There is always a line. A rack by the door holds driver's handbooks, and a clerk will let you take the learner's permit test.")
	}
}

// Shows a feature's description and any items it holds (what the Feature menu does).
examine_feature :: proc(g: ^Game, index: int) {
	if index < 0 || index >= g.feature_count {
		return
	}
	f := &g.features[index]
	clear_messages(g)
	say(g, "%s interacts with %s.", NAME, feature_names[f.kind])
	feature_flavor(g, f)
	if f.items.count > 0 {
		say(g, "Items:")
		for i in 0 ..< f.items.count {
			say(g, "- %s", item_names[f.items.items[i].kind])
		}
	}
}

// ---------------------------------------------------------------- performing verbs

perform :: proc(g: ^Game, v: Verb, target: int = 0) {
	if !verb_enabled(g, v, target) {
		return
	}
	clear_messages(g)
	say(g, "%s", verb_flavors[v])
	switch v {
	case .Continue_Journey:
		pace := counter(g, .Pace)
		eff := effective_pace(g)
		if counter(g, .Bewwy_Hertz) > 0 {
			say(g, "%s's bewwy still hertz, so effective pace will be slower.", NAME)
			counter_add(g, .Bewwy_Hertz, -1)
		}
		say(g, "%s walks %d miles.", NAME, eff)
		counter_add(g, .Distance_Remaining, -eff)
		say(g, "%s has %d miles left to go.", NAME, counter(g, .Distance_Remaining))
		apply_hunger(g, pace)
		apply_fatigue(g, pace)
		update_location(g)
		look(g)
	case .Complete_Journey:
		g.journey_complete = true
	case .Change_Pace:
		say(g, "%s's current pace is %d.", NAME, counter(g, .Pace))
		g.changing_pace = true
	case .Eat_Snax:
		say(g, "%s eats 1 snax.", NAME)
		counter_add(g, .Snax, -1)
		say(g, "%s has %d snax remaining.", NAME, counter(g, .Snax))
		counter_add(g, .Stomach, 10)
		say(g, "%s's stomach goes up by 10 to %d.", NAME, counter(g, .Stomach))
		if counter_at_max(g, .Stomach) {
			say(g, "%s's bewwy hertz.", NAME)
			counter_add(g, .Bewwy_Hertz, roll_dice(&g.rng, 2, 6))
		}
	case .Take_Nap:
		hours := counter(g, .Fatigue) / 10
		say(g, "%s naps for %d hours.", NAME, hours)
		apply_hunger(g, hours)
		say(g, "%s awakens from their nap refreshed!", NAME)
		counter_set(g, .Fatigue, 0)
	case .Forage:
		say(g, "%s forages...", NAME)
		roll := roll_dice(&g.rng, 1, 20) + counter(g, .Forage_Skill)
		say(g, "%s rolls a forage of %d.", NAME, roll)
		if roll >= g.foraging_difficulty {
			say(g, "%s finds 1 snax!", NAME)
			counter_add(g, .Snax, 1)
			say(g, "%s now has %d snax.", NAME, counter(g, .Snax))
			g.foraging_difficulty = roll + 1
		}
		apply_hunger(g, 1)
		apply_fatigue(g, 1)
		earn_foraging_experience(g, 1)
	case .Take_Shortcut:
		gain := roll_dice(&g.rng, 2, 6) + roll_dice(&g.rng, -2, 6) - counter(g, .Littering) + counter(g, .Recycling)
		counter_add(g, .Distance_Remaining, gain)
		if gain < 0 {
			say(g, "The shortcut removes %d miles from %s's journey.", -gain, NAME)
		} else if gain > 0 {
			say(g, "The shortcut adds %d miles to %s's journey.", gain, NAME)
		} else {
			say(g, "The shortcut does not help %s's journey.", NAME)
		}
		say(g, "Distance remaining: %d miles.", counter(g, .Distance_Remaining))
		apply_hunger(g, 1)
		apply_fatigue(g, 1)
		update_location(g)
		look(g)
	case .Give_Flower:
		who := character_names[g.characters[target]]
		items_remove(&g.inventory, items_find(&g.inventory, .Flower))
		say(g, "%s gives %s a flower.", NAME, who)
		say(g, "%s says `Thank you!`, and gives %s 1 jools.", who, NAME)
		counter_add(g, .Jools, 1)
		say(g, "%s now has %d jools.", NAME, counter(g, .Jools))
	case .Take_Lesson:
		counter_add(g, .Jools, -LESSON_COST)
		say(g, "%s pays %d jools.", NAME, LESSON_COST)
		counter_add(g, .Driving_Instruction, 1)
		say(g, "%s's driving instruction is now %d/%d.", NAME, counter(g, .Driving_Instruction), g.counters[.Driving_Instruction].max)
		if counter_at_max(g, .Driving_Instruction) {
			items_remove(&g.inventory, items_find(&g.inventory, .Learners_Permit))
			items_add(&g.inventory, Item{kind = .Drivers_License})
			say(g, "The instructor signs a form, stamps it twice and hands %s a Driver's License.", NAME)
		}
	case .Pick_Flower:
		f := &g.features[target]
		items_add(&g.inventory, Item{kind = .Flower})
		say(g, "%s picks Flower.", NAME)
		f.remaining -= 1
		if f.remaining <= 0 {
			say(g, "%s has completely eliminated the flower patch. Way to be a genocidal maniac!", NAME)
			feature_remove(g, target)
		}
	case .Buy_Snax:
		counter_add(g, .Jools, -1)
		say(g, "%s now has %d jools.", NAME, counter(g, .Jools))
		weights := [3]int{1, 8, 1}
		snax := rng_weighted(&g.rng, weights[:])
		switch snax {
		case 0:
			say(g, "The machine rips %s off!", NAME)
		case 1:
			say(g, "A package of snax thunks down!")
		case 2:
			say(g, "Two packages of snax thunk down! Bonus!")
		}
		counter_add(g, .Snax, snax)
		if snax > 0 {
			say(g, "%s now has %d snax.", NAME, counter(g, .Snax))
		}
	case .Buy_Dr_Pepper:
		counter_add(g, .Jools, -1)
		say(g, "%s spends 1 jools.", NAME)
		say(g, "%s now has %d jools.", NAME, counter(g, .Jools))
		items_add(&g.inventory, Item{kind = .Dr_Pepper})
	case .Recycle_Litter:
		n := items_count(&g.inventory, .Litter)
		say(g, "%s recycles %d piece(s) of litter.", NAME, n)
		counter_add(g, .Recycling, n)
		for items_find(&g.inventory, .Litter) >= 0 {
			items_remove(&g.inventory, items_find(&g.inventory, .Litter))
		}
	case .Wash_Dishes:
		f := &g.features[target]
		say(g, "%s gets %d jools.", NAME, f.jools)
		counter_add(g, .Jools, f.jools)
		say(g, "%s disappears! PÖÖF!", feature_names[f.kind])
		apply_fatigue(g, 5)
		feature_remove(g, target)
	case .Drive:
		items_remove(&g.inventory, items_find(&g.inventory, .Car_Keys))
		miles := roll_dice(&g.rng, 5, 6)
		say(g, "%s drives %s for %d miles before running out of fuel.", NAME, feature_names[g.features[target].kind], miles)
		counter_add(g, .Distance_Remaining, -miles)
		say(g, "%s has %d miles left to go.", NAME, counter(g, .Distance_Remaining))
		apply_hunger(g, 1)
		apply_fatigue(g, 1)
		update_location(g)
		look(g)
	case .Pray:
		items_add(&g.ground, Item{kind = .Hair_Ball})
		say(g, "After praying, %s sees Hairball on the ground, mysteriously.", NAME)
	case .Pick_Up_Handbook:
		items_add(&g.inventory, Item{kind = .Handbook})
		say(g, "%s takes a Driver's Handbook.", NAME)
	case .Take_Permit_Test:
		study := counter(g, .Study)
		roll := roll_dice(&g.rng, 1, 10)
		say(g, "%s has studied %d out of %d. The test rolls a %d.", NAME, study, g.counters[.Study].max, roll)
		if roll <= study {
			say(g, "%s passes! The clerk stamps a Learner's Permit and slides it across the counter.", NAME)
			items_add(&g.inventory, Item{kind = .Learners_Permit})
		} else {
			say(g, "%s fails. The clerk suggests studying the handbook, which is the entire point of the handbook.", NAME)
		}
	case .Stop_And_Smell:
		say(g, "Hey! Speaking of Stopping and Smelling Flowers...")
		say(g, "There's a new game out! You should play it!")
		say_link(g, "Stop and Smell the Flowers, now on Steam", .Steam_Smell)
	case .Drink:
		say(g, "%s consumes %s.", NAME, item_names[.Dr_Pepper])
		say(g, "%s's fatigue is completely gone!", NAME)
		counter_set(g, .Fatigue, g.counters[.Fatigue].min)
		items_remove(&g.inventory, target)
		items_add(&g.inventory, Item{kind = .Litter})
	case .Reach_In:
		bag := &g.inventory.items[target]
		if bag.snax > 0 {
			found := bag.snax
			bag.snax = 0
			counter_add(g, .Snax, found)
			say(g, "%s finds %d snax!", NAME, found)
			say(g, "%s has %d snax.", NAME, counter(g, .Snax))
		} else {
			say(g, "%s finds nothing!", NAME)
		}
	case .Study_Handbook:
		counter_add(g, .Study, 1)
		say(g, "%s has now studied %d out of %d.", NAME, counter(g, .Study), g.counters[.Study].max)
		apply_hunger(g, 1)
		apply_fatigue(g, 1)
	}
}

feature_remove :: proc(g: ^Game, index: int) {
	if index < 0 || index >= g.feature_count {
		return
	}
	copy(g.features[index:], g.features[index + 1:g.feature_count])
	g.feature_count -= 1
}

// ---------------------------------------------------------------- items: take, drop, describe

describe_item :: proc(g: ^Game, kind: Item_Kind) {
	clear_messages(g)
	say(g, "%s", item_flavors[kind])
}

// Moves up to count items of a kind between lists; returns how many moved.
@(private = "file")
move_items :: proc(from, to: ^Items, kind: Item_Kind, count: int) -> int {
	moved := 0
	for moved < count {
		i := items_find(from, kind)
		if i < 0 || !items_add(to, from.items[i]) {
			break
		}
		items_remove(from, i)
		moved += 1
	}
	return moved
}

take_from_ground :: proc(g: ^Game, kind: Item_Kind, count: int = 1) {
	clear_messages(g)
	n := move_items(&g.ground, &g.inventory, kind, count)
	if n == 1 {
		say(g, "%s takes %s.", NAME, item_names[kind])
		say(g, "%s", item_flavors[kind])
	} else {
		say(g, "%s takes %d %s.", NAME, n, item_names[kind])
	}
}

take_from_feature :: proc(g: ^Game, feature: int, kind: Item_Kind, count: int = 1) {
	if feature < 0 || feature >= g.feature_count {
		return
	}
	clear_messages(g)
	n := move_items(&g.features[feature].items, &g.inventory, kind, count)
	if n == 1 {
		say(g, "%s takes %s.", NAME, item_names[kind])
		say(g, "%s", item_flavors[kind])
	} else {
		say(g, "%s takes %d %s.", NAME, n, item_names[kind])
	}
}

drop_item :: proc(g: ^Game, kind: Item_Kind, count: int = 1) {
	clear_messages(g)
	n := move_items(&g.inventory, &g.ground, kind, count)
	if n == 1 {
		say(g, "%s drops %s.", NAME, item_names[kind])
	} else {
		say(g, "%s drops %d %s.", NAME, n, item_names[kind])
	}
}

// ---------------------------------------------------------------- ad breaks

ad_start :: proc(g: ^Game, now_ms: f64) {
	g.ad_finish_ms = now_ms + AD_DURATION_MS
}

ad_show :: proc(g: ^Game, now_ms: f64) {
	clear_messages(g)
	if g.ad_finish_ms > now_ms {
		secs := int((g.ad_finish_ms - now_ms) / 1000)
		say(g, "Time left in ad break: %02d:%02d", secs / 60, secs % 60)
		say(g, "(This is a turn based game. As such, this counter will not automatically change. You have to click the OK button to refresh.)")
		say_link(g, "For all yer umlauting needs! umlaut.fyi", .Umlaut)
	} else {
		say(g, "Ad break is complete! You may return to yer metaphor!")
		items_add(&g.inventory, Item{kind = .Coupon})
		say(g, "%s receives Coupon.", NAME)
		g.ad_finish_ms = 0
	}
}
