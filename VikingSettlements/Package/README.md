# VikingSettlements

![VikingSettlements — a pixel-art village of moss-roofed cabins at the forest's edge](https://raw.githubusercontent.com/abjumb/VikingSettlements/master/docs/brand/banner-800x296.png)

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
- **Fifteen jobs**: press E on an assigned settler to cycle — Villager,
  Lumberjack, Farmer, Builder, Blacksmith, Guard, Cook, Miner, Hunter,
  Brewer, Courier, Herder, Engineer, Innkeeper, Fisher. Producers fill your settlement's chests, cooks and brewers refine
  what they find in them, builders repair damage, guards keep watch.
  The jobs chain: hunters bring raw meat, cooks turn it into the food that
  keeps the whole settlement fed. Shift+E unassigns/dismisses.
- **Talk to your settlers**: press T while looking at any settler for a
  panel with their health, hunger and next mealtime, plus a live checklist
  of everything their job still needs — the workstation, the ingredients,
  the chest space — so an idle settler is never a mystery.
- **Builder projects**: order construction through a builder's talk menu —
  stand where the building should go, talk to a builder, pick a blueprint:
  cabins, watchtowers, fishing docks and longhouses up to livestock pens,
  palisade rings, ballista towers, the mead hall and the Town-only stone
  great-hall. Builders raise it
  from materials in the new
  **Builders' Supply Chest**; you get warned when supplies run dry, and
  your lumberjacks and miners automatically redirect their haul to the
  supply chest while a project needs it.
- **Housing**: press T on any door inside your settlement to choose who
  lives there. Settlers with a home work at full speed; homeless settlers
  work at half speed and say so when you talk to them.
- **Settler equipment**: arm your people. The talk panel's Equipment
  button hands a settler a weapon, shield or armor from your inventory —
  their AI fights with the weapon, the armor genuinely reduces damage,
  and everything they wear drops at their corpse if they fall.
- **Settlement tiers**: Hamlet → Village → Town. Growing population and
  workstations promote the settlement, raising the settler cap and
  unlocking bigger builder blueprints, up to the Town-only Stone
  Great-Hall.
- **The clanless warlord**: clear enough camps and the clanless answer —
  rival raids can bring a scaled mini-boss. Kill him and your settlement
  earns days of guaranteed peace.
- **Morale**: settlers have moods — housing, food, company and raids all
  count. Cheerful settlers work extra, miserable ones drag their feet,
  and a settler neglected for days walks out on you.
- **The mead hall**: a Village-tier blueprint with seating, a fermenter
  and hall banners. An **Innkeeper** pours a daily round from the
  brewer's stock — morale for every settler present, and players in the
  settlement get their Rested buff refreshed.
- **Families**: two housed, happy settlers can marry. Couples gain morale
  together, settlements with couples grow faster — sometimes the
  newcomer is a child of the settlement come of age — and a partner's
  confirmed death leaves real grief. Absence never fakes a widowing.
- **Seer ward-keeping**: an assigned seer heals hurt settlers every work
  tick, and foresees rival raids one night before they land.
- **Caravans**: give a settler the Courier job and they haul surplus
  goods to your other settlement on foot — and can be ambushed by the
  clanless on the way.
- **Livestock pens**: a builder blueprint with two tame boars; a Herder
  keeps them fed (vanilla breeding included), culls the herd for meat,
  and tidies loose drops into chests.
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
- **Named rival clans**: every bandit camp belongs to one of eight named
  clans — your settlement is raided by the clan of its nearest camp, raid
  messages name your enemy, and each war totem shows whose camp it is.
  A raiding warlord carries his clan's banner: fell him and that clan is
  **broken for good** — its raids on your lands end permanently.
- **Abductions & rescues**: a rival raid can carry one of your settlers
  off to the clan's camp. The banner shows who was taken and the days
  remaining — destroy that camp's war totem before the deadline and they
  walk home with their name, stars and gear intact. Fail, and they are
  lost forever. Party members are never abducted.
- **Defense works**: two Village-tier blueprints — the Palisade Ring
  (a stake ring with a gate, raised around wherever you stand) and the
  Ballista Tower, crowned with a Settlement Ballista that targets only
  enemies and can never hit players, tamed animals or your people. The
  new **Engineer** job keeps every ballista loaded and fletches turret
  bolts from wood.
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
| Raids / AbductionChance | 0.2 | Chance a rival raid carries one settler off to the raiders' camp |
| Raids / AbductionDeadlineDays | 7 | Days to break the camp's totem before a captive is lost forever |
| Economy / FoodUpkeep | true | Settlers eat from settlement chests; hungry settlers stop working |
| Economy / MealIntervalSeconds | 1800 | In-game seconds between settler meals (~1 per game day) |
| Economy / GrowthEnabled | true | Settlements attract newcomers when beds and food allow |
| Economy / GrowthChancePerDay | 0.35 | Nightly chance of a newcomer when conditions are met |
| Economy / GrowthFoodCost | 3 | Food consumed when a newcomer arrives |
| Economy / RequireWorkstations | true | Blacksmith needs a forge, builder a workbench, honey a beehive |
| Economy / HomesMatter | true | Settlers without an assigned home (talk key on a door) work at half speed |
| Economy / MoraleEnabled | true | Settler moods affect output; rock-bottom morale makes settlers leave |
| Economy / FamiliesEnabled | true | Settlers can marry: morale together, faster growth, grief on a confirmed loss |
| Trade / CourierRange | 300 | Max distance to a partner settlement for the Courier job |
| Trade / CourierAmbushChance | 0.02 | Chance a travelling courier draws a clanless ambush |
| Progression / TiersEnabled | true | Settlements grow Hamlet -> Village -> Town with tier caps and blueprint gates |
| Progression / WarlordEnabled | true | Rival raids can bring a warlord after 3+ camps cleared |
| Progression / WarlordChance | 0.25 | Chance a rival raid includes the warlord |
| Progression / WarlordPeaceDays | 10 | Days without rival raids after felling a warlord |
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

The full version history lives in the **Changelog tab** above (CHANGELOG.md
in the package), or on
[GitHub](https://github.com/abjumb/VikingSettlements/blob/master/VikingSettlements/CHANGELOG.md).

## Links & source

- **Source code, issues and guides:** https://github.com/abjumb/VikingSettlements
- **New player guide:** [Building Your First Settlement](https://github.com/abjumb/VikingSettlements/blob/master/docs/first-settlement.md)
- **Recruiting guide:** [standing, prices and follower care](https://github.com/abjumb/VikingSettlements/blob/master/docs/recruiting.md)
- **Console reference:** [commands and testing recipes](https://github.com/abjumb/VikingSettlements/blob/master/docs/commands.md)
