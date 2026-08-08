# Console Command Reference

Every console command that works with VikingSettlements — the mod's own two
commands, its spawnable prefabs, and the vanilla dev commands that exercise
each of its systems.

## Setup

The console requires the `-console` launch option (Steam → Valheim →
*Properties* → *Launch Options*). Open it in game with **F5**.

Most commands below are cheats and need dev commands enabled first:

```
devcommands
```

It must reply **"Dev commands: True"**. This resets every session, and on a
dedicated server it only works if you're in `adminlist.txt`. With it enabled,
`help` lists everything available.

## The mod's commands

| Command | Cheat? | What it does |
|---|---|---|
| `vs_find [village\|outpost\|steading\|camp]` | **No** | Marks the nearest settlement or camp on your map with distance and compass direction. Works without `devcommands` — it's a legitimate player tool, like Hugin's boss hints. Finds locations anywhere in generated terrain, explored or not |
| `vs_spawn [village\|outpost\|steading\|camp]` | Yes | Builds the chosen settlement 15 m in front of you, settlers/raiders included. The way to get settlements into already-explored terrain |

Both default to `village` when no argument is given.

## The mod's spawnable prefabs

Usable with vanilla `spawn <prefab> [count] [stars]` (cheat):

| Prefab | What you get |
|---|---|
| `VS_Settler` | A wild settler — recruitable as normal. `spawn VS_Settler 1 3` spawns one already at Elite (2 stars) |
| `VS_Seer` | The support-mage settler variant |
| `VS_Raider` | A hostile clanless bandit — instant raid practice |
| `VS_Trader` | The village trader (static NPC; opens his store on interact) |
| `VS_CampTotem` | A destructible war totem — smashing it counts as clearing a camp |
| `VS_SettlementBanner` | The settlement banner piece (normally built with the hammer) |
| `VS_Flatten` | ⚠ A one-shot terrain op: instantly levels a 13 m radius and paints it dirt **at your feet, permanently**. Handy for prepping a build site, but there is no undo |

## Testing each system with vanilla commands

### Recruiting & settlement

```
spawn Coins 200          -- recruiting money
spawn Wood 50            -- banner materials...
spawn FineWood 20
debugmode                -- then press B in-hammer for free, instant building
```

`debugmode` also gives you **Z** (fly — the fastest way to scout for villages)
and **K** (kill all nearby enemies — ends a raid instantly).

### Economy, growth & veterancy

```
spawn CookedMeat 20      -- stock the pantry
skiptime 1800            -- advance ~one in-game day (meals, service XP)
tod 0.9                  -- jump to night (growth + rival raid rolls happen then)
tod -1                   -- release the clock afterwards
```

Nightly rolls (newcomer growth, rival raids) happen once per settlement per
night **while the area is loaded** — you must be nearby.

### Raids

```
setkey defeated_eikthyr  -- satisfies the raid gate without killing the boss
event vs_banditraid      -- force the native bandit raid event
stopevent                -- end it
randomevent              -- let the game roll a random event naturally
```

Raid scaling reads these too: `setkey defeated_gdking` makes rival raiders
eligible for 1 star, `setkey defeated_bonemass` for 2.

### Clanless camps

The mod tracks cleared camps with global keys `vs_camp_cleared_1` through
`vs_camp_cleared_10` — each reduces rival raid chance 5%, and setting all ten
disables the native bandit event:

```
listkeys                     -- see which are set
setkey vs_camp_cleared_1     -- fake a cleared camp
resetkeys                    -- ⚠ wipes ALL global keys, including boss kills
```

### Cleanup after testing

```
removedrops              -- clears dropped items
killall                  -- kills nearby hostiles (not your settlers - they're friendly)
```

## Two things commands can't do

- **`exploremap` does not reveal settlements** — it uncovers terrain on the
  map, but locations don't carry map markers. Use `vs_find`.
- **No command adds settlements to already-generated terrain** except
  `vs_spawn` — world-gen locations (including clanless camps) only exist in
  chunks generated after the mod was installed.

For per-feature testing walkthroughs with config tweaks (fast work ticks,
guaranteed raids), see the [README's Testing section](../README.md#testing-in-game);
for the player-facing flow, the [first settlement guide](first-settlement.md).
