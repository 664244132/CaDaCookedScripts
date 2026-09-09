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

**Commercial Sink Station**:
A dedicated, fully-clad brushed stainless steel dishwashing workstation featuring a deep recessed wash basin, Pre-Rinse gooseneck faucet with active water dripping, corrugated wire drying rack, authentic cleaning props (sponge and soap bottle), and high-visibility World-Space station signage.
_Avoid_: Wooden sink, flat sink, invisible wash counter

**Sink Orientation Alignment**:
The geometric alignment convention where a workstation's interactive front (doors, sink basin opening, faucet spray, and signage) strictly faces Local +Z (towards the player/chef in the kitchen), while its rear backsplash wall strictly faces Local -Z (towards the perimeter room wall), guaranteeing correct perspective across any procedural slot in the kitchen grid.
_Avoid_: Backwards sink, reverse counter, upside-down station

**Closed-Loop Dishwashing Ecosystem**:
A conservation law in the kitchen where the total number of plates is strictly bounded (fixed at 4). PlatesCounter ceases passive continuous generation; clean plates must be transported from SinkCounter to PlatesCounter (or used directly), and delivered dishes return exclusively as dirty plates to be scrubbed, establishing an interdependent gameplay cycle without infinite resources.
_Avoid_: Infinite plate dispenser, decoupled washing, orphan plate generation

**Stove Extrication Recovery**:
A fail-safe state restoration pattern where a cooking workstation (e.g. StoveCounter) immediately reverts from Frying/Fried/Progressing to Idle (extinguishing flame visuals, halting sizzling audio, and resetting progress gauges) the instant its cooking ingredient is abruptly removed or stolen outside the normal Player pickup flow (e.g. by KitchenCatNPC).
_Avoid_: Ghost frying, infinite sizzling, phantom cooking

**Flank Staging Zone**:
The designated perimeter waiting positions at the far left (X ≈ -9.2m) and far right (X ≈ +9.2m) screen boundaries where kitchen obstacle NPCs (e.g. KitchenCatNPC) lurk and patrol, keeping central chef workstations free from immediate proximity interference.
_Avoid_: Corridor spawning, chef path blocking, workstation crowding

**Steal Telegraph**:
A deliberate anticipation window (1.0–1.5s windup) where a thieving NPC pauses at a targeted workstation, emitting visual and acoustic cues (exclamation bubble, alert meow, tail twitch) before seizing the food, affording the chef fair reaction time to intervene.
_Avoid_: Instant snatch, zero-frame theft, unavoidable item loss

**Chef Presence Deterrent**:
The spatial protection rule preventing NPCs from targeting or snatching items from any workstation currently occupied by or adjacent to the active chef (within 1.5m), ensuring uninterrupted active cooking.
_Avoid_: Face-to-face theft, work disruption, interactive robbery

### UI & Presentation Layer

**UI Theme Design Tokens**:
A centralized repository of color tokens, Rich Text styling constants, and DRY text formatters (`UITheme.cs`) that decouples visual presentation and text formatting from gameplay logic, serving as the single source of truth for UI appearance across the project.
_Avoid_: Hardcoded hex tags, inline font markup, scattered text styles
