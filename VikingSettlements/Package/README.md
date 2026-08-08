# VikingSettlements

Vikings have finally learned to build homes of their own. This mod adds
**inhabited NPC settlements** to Valheim's world generation:

- **Meadows villages** — a longhouse, cabins, a farm, a maypole, a watchtower,
  a market stall with a trader, and villagers around a central fire.
- **Black Forest outposts** — small fortified camps ringed by sharp stakes,
  manned by a few hardy settlers.
- **Plains steadings** — stone-walled halls with barley and flax fields,
  home to settlers and a village seer.

Settlers are peaceful villagers with their own names. They defend their home
against raiding monsters and turn hostile if you attack them. Villages contain
loot chests, crops to pick, working crafting spots, and (in meadows villages)
a trader with a small store of early-game supplies.

## Requirements

- Valheim 0.221.4 or compatible
- [BepInExPack Valheim](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/) 5.4.2333+
- [Jötunn](https://thunderstore.io/c/valheim/p/ValheimModding/Jotunn/) 2.29.2+

## Installation

**With a mod manager:** install BepInExPack Valheim and Jötunn into your
profile, then install this mod. Launch through the manager.

**Manually:**

1. Extract BepInExPack Valheim into your Valheim folder so `winhttp.dll` sits
   next to `valheim.exe`, launch the game once, then quit.
2. Copy `Jotunn.dll` into `<Valheim>/BepInEx/plugins/`.
3. Copy `VikingSettlements.dll` into `<Valheim>/BepInEx/plugins/`.
4. Launch Valheim. The config is written to
   `BepInEx/config/com.abjumb.vikingsettlements.cfg` on first run.

**Multiplayer:** the server and every client need the mod at the same minor
version, or clients are rejected at connect. Install it on dedicated servers
the same way; world and raid settings are admin-only and sync from the server.

> **Settlements only generate in new terrain.** Installing on an existing save
> will not add villages to areas you have already explored — start a new world,
> sail somewhere new, or use `vs_spawn` (requires `devcommands`).

## New player guide

**[Building Your First Settlement](https://github.com/abjumb/VikingSettlements/blob/master/docs/first-settlement.md)**
— step by step from finding your first village to defending your own.

## Features

- Three settlement location types spawned by world generation
  (new worlds, or unexplored areas of existing worlds).
- Named settler NPCs that stay in their village, chat with visiting players,
  and fight off raiding monsters.
- A village trader with a small store.
- **Build your own settlement**: craft the *Settlement Banner* (hammer →
  Misc, needs a workbench) to found a settlement. Name it (Shift+E on the
  banner), and manage it from one screen: E opens a panel listing every
  settler — name, rank, job, hunger — with job reassignment buttons.
- **Recruit settlers**: press E on a settler in a wild settlement to recruit
  them for coins. They follow you; bring them home and press E near your
  banner to assign them.
- **A war party you can lose**: up to 4 recruited villagers fight at your
  side as a persistent party. Command them with hotkeys — G toggles
  follow/hold, H orders a protected fall-back — or E on a member to post
  them somewhere. They ride boats and take portals with you (stowed safely
  into your character save) and survive logout. You can never hurt your own
  people, and no fall, fire or forgotten corner of the map can kill them:
  a party member can only die to a monster **in a fight you are standing
  in**, after loud low-health warnings and an auto-retreat you can override.
  When one falls, they are gone.
- **Ten jobs**: press E on an assigned settler to cycle — Villager,
  Lumberjack, Farmer, Builder, Blacksmith, Guard, Cook, Miner, Hunter,
  Brewer. Producers fill your settlement's chests, cooks and brewers refine
  what they find in them, builders repair damage, guards keep watch.
  The jobs chain: hunters bring raw meat, cooks turn it into the food that
  keeps the whole settlement fed. Shift+E unassigns/dismisses.
- **Talk to your settlers**: press T while looking at any settler for a
  panel with their health, hunger and next mealtime, plus a live checklist
  of everything their job still needs — the workstation, the ingredients,
  the chest space — so an idle settler is never a mystery.
- **Food & growth**: settlers eat from your chests (cheapest food first) about
  once per game day; a hungry settler stops working. A well-fed settlement
  below its cap, with a spare bed, attracts newcomers on its own.
- **Veterancy**: settlers earn XP from days of service and battles survived,
  rising to Veteran and Elite star levels with vanilla stat scaling — your
  longest-serving villagers become your best defenders.
- **Village standing**: wild villages remember how you treat them. Defend
  and donate to recruit at a discount; rob and murder and they refuse to
  deal with you.
- **Workstations matter**: blacksmiths need a forge in the settlement,
  builders a workbench, and honey production a beehive.
- **Raids**: your settlement counts as a base for Valheim's native random
  event system — a new "The clanless are raiding!" event sends bandits
  against it. Rival clans may also assault your settlement at night, with war
  parties that scale with your population and the bosses you've killed.
- **Clanless camps**: the raiders have homes — bandit camps in world gen.
  Destroy a camp's war totem to permanently weaken rival raids; break ten
  and the native raid event goes silent.
- Console command `vs_find [village|outpost|steading|camp]` marks the
  nearest one on your map (no cheats needed); `vs_spawn` (requires
  `devcommands`) places one in already-explored terrain.
- Configurable: settlement counts per world, recruit cost, settlement size,
  work speed, raid chance — server-synced where it matters.

## Configuration

Edit `BepInEx/config/com.abjumb.vikingsettlements.cfg` (created on first run):

| Setting | Default | Description |
|---|---|---|
| Locations / MeadowsVillages | 60 | Placement attempts for meadows villages (0 disables) |
| Locations / ForestOutposts | 80 | Placement attempts for black forest outposts (0 disables) |
| Locations / PlainsSteadings | 50 | Placement attempts for plains steadings (0 disables) |
| Settlers / DefendPlayers | false | Settlers join the player faction and fight alongside you |
| Settlers / EnableTrader | true | Meadows villages include a trader |
| Settlers / Chatter | true | Settlers greet nearby players (client-side) |
| Settlers / ChatterIntervalSeconds | 25 | Minimum time between chatter lines |
| Settlers / TalkHotkey | T | Talk to the settler you're looking at: health, hunger, job needs (client-side) |
| Recruiting / RecruitCostCoins | 50 | Coins to recruit a settler |
| Settlement / MaxSettlers | 10 | Max settlers per settlement banner |
| Settlement / SettlementRadius | 32 | Settlement area radius in meters |
| Settlement / WorkIntervalSeconds | 60 | Seconds between settler work ticks |
| Raids / EnableRaids | true | Enable bandit raid event and rival clan raids |
| Raids / RaidsAfterFirstBoss | true | Raids only start once Eikthyr is dead |
| Raids / RivalRaidChancePerDay | 0.15 | Nightly chance of a rival clan raid per settlement |
| Raids / ClanlessCamps | 60 | Bandit camp placement attempts in world gen (0 disables) |
| Raids / ScaleRaids | true | War parties scale with population and boss progression |
| Raids / CampClearRaidReduction | 0.05 | Rival raid chance reduction per cleared camp (max 10) |
| Economy / FoodUpkeep | true | Settlers eat from settlement chests; hungry settlers stop working |
| Economy / MealIntervalSeconds | 1800 | In-game seconds between settler meals (~1 per game day) |
| Economy / GrowthEnabled | true | Settlements attract newcomers when beds and food allow |
| Economy / GrowthChancePerDay | 0.35 | Nightly chance of a newcomer when conditions are met |
| Economy / GrowthFoodCost | 3 | Food consumed when a newcomer arrives |
| Economy / RequireWorkstations | true | Blacksmith needs a forge, builder a workbench, honey a beehive |
| Veterancy / VeterancyEnabled | true | Settlers earn XP and star levels from service and battles |
| Veterancy / XpPerStar | 20 | XP for the first star; second star costs three times as much |
| Reputation / ReputationEnabled | true | Wild villages track standing; scales recruit costs |
| Reputation / DonationCostCoins | 10 | Coins per donation (Shift+E on a wild settler) |
| Reputation / DonationReputation | 5 | Standing gained per donation |
| Party / MaxPartySize | 4 | Villagers that can travel with you at once (max 4) |
| Party / AutoFallbackWhenGravelyWounded | true | Members below 25% health retreat to you automatically |
| Party / OutOfCombatRegenPerSecond | 2 | Member health regen after 10s without damage (0 disables) |
| Party / StanceHotkey | G | Toggle party follow/hold (client-side) |
| Party / FallbackHotkey | H | Order a protected fall-back (client-side) |

## Changelog

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
