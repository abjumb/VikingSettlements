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
- Console command `vs_spawn [village|outpost|steading]` (requires
  `devcommands`) to place a settlement in already-explored terrain.
- Configurable: settlement counts per world, settler faction behavior,
  trader and chatter toggles — server-synced where it matters.

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

## Changelog

### 1.0.0

- Initial release: meadows villages, black forest outposts, plains steadings,
  named settlers, village trader, `vs_spawn` command, configuration.
