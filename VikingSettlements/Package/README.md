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
  Misc, needs a workbench) to found a settlement.
- **Recruit settlers**: press E on a settler in a wild settlement to recruit
  them for coins. They follow you; bring them home and press E near your
  banner to assign them.
- **Ten jobs**: press E on an assigned settler to cycle — Villager,
  Lumberjack, Farmer, Builder, Blacksmith, Guard, Cook, Miner, Hunter,
  Brewer. Producers fill your settlement's chests, cooks and brewers refine
  what they find in them, builders repair damage, guards keep watch.
  The jobs chain: hunters bring raw meat, cooks turn it into the food that
  keeps the whole settlement fed. Shift+E unassigns/dismisses.
- **Food & growth**: settlers eat from your chests (cheapest food first) about
  once per game day; a hungry settler stops working. A well-fed settlement
  below its cap, with a spare bed, attracts newcomers on its own.
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

## Changelog

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
