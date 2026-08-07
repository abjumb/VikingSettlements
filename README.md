# VikingSettlements

A [Jötunn](https://github.com/Valheim-Modding/Jotunn)-based Valheim mod that
adds **inhabited NPC settlements** to world generation. Built from the
[JötunnModStub](https://github.com/Valheim-Modding/JotunnModStub) template.

## What the mod does

Three new location types are woven into Valheim's world generation:

| Location | Biome | Contents |
|---|---|---|
| `VS_MeadowsVillage` | Meadows | Longhouse, cabins, farm, maypole, watchtower, trader stall, fire plaza, 7 settlers + trader |
| `VS_ForestOutpost` | Black Forest | Watchtower, cabin, stake ring, campfire, 3 settlers |
| `VS_PlainsSteading` | Plains | Stone hall, barley/flax farm, watchtower, stake ring, 4 settlers |

Settlements are assembled procedurally from vanilla building pieces, so no
custom assets or asset bundles are required. Settler NPCs are cloned from
vanilla humanoids and re-purposed:

- Each settler gets a persistent personal name (derived from its network id,
  so all clients agree without extra syncing).
- Settlers stay in their settlement (their AI patrol point is pinned to
  their home), fight raiding monsters, and only turn on players if attacked.
  A config option can put them on the player faction instead.
- Settlers greet players who come close (client-side chatter).
- Meadows villages include a trader (cloned from Haldor) with a small store
  of early-game supplies.
- The ground under a settlement is levelled at spawn by a one-shot terrain op.

Because locations are only placed by the world generator, settlements appear
in **new worlds or unexplored areas** of existing worlds. For explored areas
there is a console command (`devcommands` required):

```
vs_spawn [village|outpost|steading]
```

## Building without a Valheim install

The repository can be compiled headless (CI, containers) — no game copy
needed:

```sh
./scripts/fetch-libs.sh          # assembles reference assemblies under vendor/
dotnet build VikingSettlements.sln -c Release
```

`fetch-libs.sh` downloads publicized game assemblies (ValheimGameLibs, NuGet),
UnityEngine reference modules (NuGet) and BepInEx 5 (GitHub releases), lays
them out like a Valheim install under `vendor/valheim`, and writes
`Environment.props` so the Jötunn build props pick them up.

With a local Valheim install, the stock template workflow works unchanged:
delete `Environment.props`, set `VALHEIM_INSTALL`, and (optionally) flip
`DoPrebuild.props` to have Jötunn publicize your game assemblies.

## Project layout

```
VikingSettlements/
├── VikingSettlements.cs        # plugin entry point, config + manager wiring
├── ModConfig.cs                # BepInEx config entries (server-synced)
├── Npcs/
│   ├── SettlerPrefabs.cs       # clones vanilla humanoids into settler/trader prefabs
│   ├── SettlerIdentity.cs      # deterministic personal names
│   ├── SettlerChatter.cs       # proximity greetings
│   └── SettlerHome.cs          # pins AI patrol point to the settlement
├── World/
│   ├── SettlementLayout.cs     # data-driven blueprint DSL
│   ├── Layouts.cs              # the actual settlement blueprints
│   ├── LayoutBuilder.cs        # instantiates blueprints (locations & command)
│   └── SettlementLocations.cs  # ZoneManager registration
└── Commands/
    └── SpawnSettlementCommand.cs
```

All vanilla prefab references are resolved defensively — after a game update
a renamed prefab logs a warning and is skipped instead of breaking world
loading.

## Known limitations

- Settlement structures are placed on Valheim's build grid from code; exact
  piece pivots can only be fine-tuned in game, so expect some rustic
  imperfections in roofs and gables.
- Settlers use dvergr models (the closest vanilla friendly humanoids with
  full combat AI). Custom player-model settlers would need a Unity asset
  bundle, which this repo's Unity project supports as a follow-up.
- Killed settlers do not respawn — settlements can be wiped out by raids.

## Debugging

See the Wiki page [Debugging Plugins via IDE](https://github.com/Valheim-Modding/Wiki/wiki/Debugging-Plugins-via-IDE)
for more information.
