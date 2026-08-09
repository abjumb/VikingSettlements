# Changelog

### 1.11.0

- Named rival clans: every clanless camp now belongs to one of eight
  named clans (the Ashwolves, the Blood Ravens, the Rime Serpents...).
  Your settlement is raided by the clan of its nearest camp - raid
  messages name them, and each camp's war totem shows whose it is.
  A raiding warlord carries his clan's banner: fell him and that clan
  is broken for good - its raids on your lands end permanently, on top
  of the existing temporary peace.
- Abductions & rescues: a rival raid can carry one assigned settler off
  to the clan's camp (20% of raids by default, one captive at a time).
  The banner shows who was taken and how many days remain - destroy
  that camp's war totem before the deadline (7 days by default) and
  they walk home with their name, stars and gear intact. Fail, and
  they are lost forever and the settlement mourns. Party members are
  never abducted - their fate stays under the party's permadeath
  contract.
- Defense works: two new Village-tier builder blueprints. The Palisade
  Ring (80 wood) raises a 10 m ring of sharp stakes with a gate and
  torches around wherever you stand; the Ballista Tower (60 wood, 20
  stone) is the watchtower crowned with a Settlement Ballista - a
  turret that targets only enemies and can never hit players, tamed
  animals or your people (bolts from any ballista, including vanilla
  ones you built, no longer damage recruited settlers).
- Engineer job (the thirteenth): needs a workbench. Each work tick the
  engineer keeps the settlement's ballista towers loaded from bolts in
  your chests (topping up the emptiest first, never mixing ammo types),
  and fletches 4 wood turret bolts from 2 wood when everything is
  loaded - up to a stockpile of 40.
- New config: Raids/AbductionChance and Raids/AbductionDeadlineDays.

### 1.10.0

- Settler morale: assigned settlers track a mood (shown in the talk
  panel) adjusted daily by housing, food and company, and knocked down
  when their settlement is raided. Cheerful settlers produce an extra
  item per tick, miserable ones skip half their work ticks, and a
  settler left at rock bottom for days packs up and leaves the
  settlement - becoming a wild villager again, levels and all.
- Courier job + caravans: with a second settlement within range
  (default 300 m), a Courier loads up to 8 surplus goods (whatever the
  settlement has more than 10 of) and physically walks them to the
  partner settlement's chests, then walks home. On the open road they
  can draw a clanless ambush - and a courier that dies drops
  everything they carried. Journeys progress while the area is loaded.
- Herder job + Livestock Pen: a new Village-tier blueprint raises a
  fenced pen with two tame boars (VS_PenBoar spawns tamed). Herders
  keep feed on the ground for the animals (carrots/turnips from your
  chests, so vanilla breeding runs), cull the herd above four head for
  the larder, and carry loose drops in the settlement to storage.
- Twelve jobs total. New config: Economy/MoraleEnabled, and a Trade
  section with CourierRange and CourierAmbushChance.

### 1.9.0

- Settler equipment: open a recruited settler's talk panel and press
  Equipment to hand over a weapon, shield, helmet, chest or leg armor
  from your inventory (take-back buttons included). Gear persists in
  the save, survives party travel, changes their combat strength -
  weapons are used by their AI, armor reduces damage they take - and
  drops at their corpse if they die. A geared war party is a real
  investment, and losing one hurts accordingly.
- Settlement tiers: settlements now grow Hamlet -> Village -> Town.
  Population plus a workbench earns Village; a real population plus a
  forge earns Town. Each tier raises the settler cap (Hamlet 60%,
  Village 100%, Town 150% of the configured max) and unlocks builder
  blueprints: the longhouse needs a Village, and Towns unlock the new
  Stone Great-Hall. The tier shows on the banner and in the panel.
- The clanless warlord: once three or more camps are cleared, rival
  raids can bring a warlord - a mini-boss whose health and stars scale
  with boss progression, with a heavy coin purse. Fell him and the
  settlement he marched on gets 10 days (configurable) with no rival
  raids. Camp-clearing now has a climax instead of a quiet fade-out.
- New config section Progression: TiersEnabled, WarlordEnabled,
  WarlordChance, WarlordPeaceDays.

### 1.8.2

- New look, from the mod's design system: the package icon is now the
  hand-made VS shield, the settlement management panel was redesigned
  (population bar, level badges, rank stars, job stepper wells and a
  working/hungry status column), and the panels share one consistent
  palette. The README gained the pixel-art banner and a ten-jobs
  reference graphic.

### 1.8.1

- Fixed settlement terrain spawning terraced, with buildings buried in
  mounds or hovering over pits: each settlement now levels its ground
  with a single terrain op sized to the whole footprint (village 18 m,
  steading 17 m, outpost 11 m, camp 10 m). The previous overlapping ops
  re-sloped each other's leveled ground. Applies to newly generated or
  vs_spawn-ed settlements; already-spawned terrain is not reshaped.
- Fixed the settler talk panel rendering its text half outside the
  panel, and two lines being cut off. The "who lives here?" door panel
  had the same alignment bug.

### 1.8.0

- War party: up to 4 recruited villagers travel and fight at your side.
  G toggles follow/hold for the party, H orders a fall-back (members stop
  fighting, run to you and take 75% reduced damage); E on a member posts
  them in place or brings them along, and near a banner E still settles
  them in. `vs_party` lists the roster, `vs_party recall` retrieves
  separated members (host/singleplayer).
- Party members survive every traversal system: boats and portals stow
  them into your character save and they step out with you at the other
  end; logging out pockets them the same way, and members who fall behind
  teleport to you instead of being lost to zone unloading.
- The permadeath contract: players can no longer damage recruited
  villagers at all (a stray swing cannot kill or aggro your own people),
  party members take no environmental damage (falls, drowning, smoke,
  fire), and they are untouchable while you are dead or away. The only
  way to lose one is a monster killing them in a fight you are standing
  in — telegraphed by wounded/gravely-wounded warnings and an automatic
  retreat below 25% health that you can override. Death is permanent.
- Members recover health out of combat, so losses are a stake inside the
  fight rather than an attrition tax between fights. Settler names now
  persist in the save (previously they were derived from the network id).
- Fixed villages spawning half-collapsed: settlement buildings are now
  built from hardened piece variants (VS_loc_*) with structural-integrity
  and rain wear disabled, so the support calculation racing the terrain
  flatten at spawn can no longer tear down towers, roofs and walls.
  Existing already-collapsed villages are not retroactively rebuilt; newly
  generated (or vs_spawn-ed) ones spawn intact. Raids can still damage the
  buildings and builders still repair them.
- Talk to settlers: a new hotkey (T, configurable) opens a talk panel for
  the settler you are looking at - health, hunger and next mealtime, and
  a live checklist of everything their job still needs (workstation,
  ingredients, chest space) evaluated with the same checks the work loop
  uses. Recruiting now also hints that followers must be settled at your
  banner before they take a job.
- Builder projects: order construction through a builder's talk menu.
  Stand where the building should go, pick a blueprint (cabin 40 wood,
  watchtower 30 wood, longhouse 100 wood + 10 stone - the wild meadows
  buildings), and a construction site is marked out. Builders carry
  materials into it from the new buildable Builders' Supply Chest on
  their work ticks and raise the finished building on the spot. A
  recurring warning fires while a project's materials have run dry, and
  lumberjacks and miners automatically deposit their haul into the
  supply chest while a project still needs it. Shift+E cancels a site.
- Housing: press the talk key on a door inside your settlement to choose
  which settler lives there (one per door; blueprint cabins and
  longhouses come with doors and beds). With HomesMatter enabled,
  settlers without a home work at half speed and say so in their talk
  panel.

### 1.7.0

- Wild-village reputation: each village tracks a shared standing (-100..100)
  toward players, anchored in an invisible Village Heart at its center.
  Defending villagers while monsters attack (+1) and donating coins via
  Shift+E (+5 per 10 coins) raise it; hitting villagers (-5), killing them
  (-25) and recruiting (-2) lower it. Standing tiers scale recruit costs
  from 50% (Honored) to 150% (Distrusted); Hated villages refuse recruits.
  Villages generated before 1.7 behave neutrally (spawn VS_VillageHeart to
  retrofit one).

### 1.6.0

- Settlement naming: Shift+E on the banner (or the panel's Rename button)
  opens the sign-style text dialog. The name shows on the banner's hover,
  in the panel header, and syncs to all players.
- Management panel: E on the banner opens a woodpanel UI listing every
  assigned settler with name, rank, job and hunger status, plus prev/next
  buttons to reassign any settler's job without hunting them down. Closes
  on Escape or when you walk away.

### 1.5.0

- Settler veterancy: settlers earn 1 XP per in-game day of assigned service
  and 2 XP per battle survived. At 20 XP they become a 1-star **Veteran**, at
  60 XP a 2-star **Elite**, with vanilla star stat scaling. Rank shows in
  hover text; levels and XP persist in the world save. Wild villagers also
  harden from combat, so old villages grow tougher over time.

### 1.4.0

- Four new jobs: **Cook** (cooks raw meat/fish from settlement chests, needs
  a cooking station), **Miner** (stone plus the occasional copper/tin ore),
  **Hunter** (raw meat, deer hide, feathers), and **Brewer** (2 honey → minor
  healing mead, 2 barley → barley wine, needs a fermenter).
- Hunters and cooks form a food chain with the settlement's meal upkeep.

### 1.3.0

- Clanless camps in world generation: bandit camps with shelters, loot and a
  destructible war totem. Each cleared totem permanently reduces the rival
  raid chance by 5%; clearing ten disables the native bandit raid event.
- Raid scaling: rival war parties grow with the target settlement's
  population (3–8 raiders) and gain star levels after The Elder and Bonemass.
- New `vs_find [village|outpost|steading|camp]` command marks the nearest
  settlement on your map with distance and direction — no cheats required.
- `vs_spawn` gained a `camp` variant.

### 1.2.0

- Settlement economy: settlers now eat one food item from settlement chests
  roughly once per in-game day (cheapest first); hungry settlers stop
  working until their next meal. Hover a settler or the banner to see hunger.
- Population growth: a settlement below its cap, with a spare unclaimed bed
  and enough food, can attract a newcomer each night — rarely a seer.
- Workstation-gated jobs: blacksmiths need a forge inside the settlement,
  builders a workbench, and farmers a beehive to produce honey.
- All of it is configurable (new Economy config section) and can be disabled
  to restore 1.1 behavior.

### 1.1.0

- Build your own settlement with the new Settlement Banner piece.
- Recruit settlers from wild settlements with coins; they follow you and can
  be assigned to your settlement.
- Jobs for assigned settlers: Lumberjack, Farmer, Builder, Blacksmith, Guard.
- Bandit raid event registered with Valheim's native random event system;
  rival clans can raid your settlement at night.

### 1.0.0

- Initial release: meadows villages, black forest outposts, plains steadings,
  named settlers, village trader, `vs_spawn` command, configuration.
