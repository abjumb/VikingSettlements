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

## Installation (manual)

1. Install [BepInEx](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/)
   and [Jötunn](https://thunderstore.io/c/valheim/p/ValheimModding/Jotunn/).
2. Drop `VikingSettlements.dll` into `<Valheim>/BepInEx/plugins`.
3. All players and the server need the mod installed.

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
- **Jobs**: press E on an assigned settler to cycle their job —
  Villager, Lumberjack, Farmer, Builder, Blacksmith, Guard.
  Lumberjacks and farmers deposit resources into your settlement's chests,
  blacksmiths smelt ore they find in them, builders repair damaged
  structures, guards get sharper senses. Shift+E unassigns/dismisses.
- **Raids**: your settlement counts as a base for Valheim's native random
  event system — a new "The clanless are raiding!" event sends bandits
  against it. Rival clans may also assault your settlement at night.
- Console command `vs_spawn [village|outpost|steading]` (requires
  `devcommands`) to place a settlement in already-explored terrain.
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

## Changelog

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
