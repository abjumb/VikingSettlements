# Changelog

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
