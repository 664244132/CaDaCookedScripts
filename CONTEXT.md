# CaDaCook

A fast-paced casual cooking simulation game where players prepare recipes under dynamic kitchen hazards and time pressure.

## Language

### Player Locomotion & Mechanics

**Chef**:
The primary controllable player character navigating the kitchen to prepare and deliver dishes.
_Avoid_: Avatar, worker, character

**Dash**:
A short-duration, high-velocity movement burst triggered by the player with a brief cooldown to quickly reposition across the kitchen.
_Avoid_: Sprint, roll, flash

**Slide Bonus**:
An intensified forward momentum boost applied when a Chef initiates a Dash across a Slippery Floor surface.
_Avoid_: Ice slide, skid

### Safety & Emergency Tools

**Fire Extinguisher**:
A portable safety tool used to spray mist extinguishing kitchen counter fires.
_Avoid_: Foam tank, water hose

**Extinguisher Respawn**:
An automated recovery mechanism that repositions a stolen Fire Extinguisher back to its original spawn bracket after a 15-second cooldown.
_Avoid_: Item respawn, reload

### Dynamic Hazards & Pests

**Stray Cat**:
An AI-controlled kitchen intruder that targets food and tools on counters, steals them, and sprints off-screen.
_Avoid_: Thief, animal, pest

**Slippery Floor**:
An environmental oil puddle hazard that disrupts Chef movement with forward momentum and rotational spin.
_Avoid_: Oil spill, trap

### Kitchen Resource Loop & Customer Experience

**Dirty Plate**:
A stackable, soiled dish coated with residual food stains generated upon successful recipe delivery that must be scrubbed at the Sink Counter.
_Avoid_: Trash plate, soiled dish

**Sink Counter**:
A specialized interactive kitchen counter equipped with a washing basin, faucet, and drying rack where Chefs repeatedly press the alternate interact key to scrub dirty plates into clean plates.
_Avoid_: Washbasin, cleaning station, water tap

**Customer Patience**:
The diminishing countdown timer allocated to normal recipe orders before the waiting diner becomes frustrated.
_Avoid_: Hunger meter, order duration

**Angry Customer**:
A penalty state triggered when Customer Patience expires, marked by an alert red card on HUD; fulfilling this order awards base score only without combo streak or tip multipliers.
_Avoid_: Expired order, failed diner

### Dynamic Kitchen Procedural Generation

**Counter Shuffling**:
A procedural round-initialization mechanism that permutes workstation positions and orientations across valid kitchen grid slots each match.
_Avoid_: Random counter, kitchen scramble

**Collision-Free Counter Shuffling**:
A mathematical 1-to-1 bijection guarantee ensuring distinct counter GameObjects map exclusively to unique, deduplicated kitchen slots (>= 0.8m apart) without coordinate overlap.
_Avoid_: Counter stacking, duplicate spawn

**Path Clearance Check**:
A geometric swept-line raycast/distance verification ensuring moving workstation paths maintain >= 1.35m separation from all stationary counters and scene boundaries.
_Avoid_: Counter sliding through wall, moving counter collision

**Rack Full Lock**:
A safety lockout mechanism on the Sink Counter that halts plate scrubbing when the clean drying rack reaches its 4-plate maximum capacity, preventing plate vaporization.
_Avoid_: Sink jam, plate overflow

**Angry Patience Timer**:
A 20-second grace countdown initiated once an order enters the Angry Customer state; failure to serve within this window causes customer departure with a 50-point score penalty.
_Avoid_: Rage quit timer, order timeout 2

**Plate Scraping**:
The action of clearing burned or unwanted ingredients from a Plate into the Trash Counter while preserving the clean dishware in the Chef's hands.
_Avoid_: Plate disposal, dish trashing

