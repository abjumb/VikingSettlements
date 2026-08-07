# Building Your First Settlement

A step-by-step guide to going from wandering Viking to village chieftain.
Should take about one in-game day once you have the materials.

## Before you start

You need three things:

| What | Where it comes from |
|---|---|
| **20 coins** for the banner, **50 per settler** you recruit | Burial chambers, Fuling villages, Haldor, and the villagers' own chests |
| **10 wood, 4 fine wood** | Fine wood needs birch or oak — which means a **bronze axe** |
| **A wild village to recruit from** | Meadows, Black Forest or Plains |

That bronze axe requirement is the real gate here: you'll want to be past the
Black Forest before founding a settlement. Budget around **170 coins** for a
banner plus three settlers.

> **Villages only appear in land the game has never loaded before.** If you
> installed the mod on an existing save, your explored territory won't have any.
> Sail somewhere new, start a fresh world, or spawn one in with `vs_spawn`
> (needs `devcommands`).

## Step 1 — Find a village

The quick way: open the console (F5) and type `vs_find village` — it marks the
nearest village on your map with distance and direction, no cheats needed.
Otherwise, explore until you find one. There are three kinds:

- **Meadows village** — the big one. A longhouse, cabins, a farm, a trader, and
  seven villagers. This is the best place to recruit from.
- **Black Forest outpost** — a watchtower and cabin behind a ring of stakes,
  three settlers.
- **Plains steading** — a stone hall and barley fields, four settlers.

The villagers are peaceful. They'll greet you as you walk past and they won't
attack unless you attack them first.

## Step 2 — Recruit your first settlers

Walk up to any villager. You'll see **"Recruit (50 Coins)"** in the hover text.

- **Press `E`** to hire them. The coins come out of your inventory and they'll
  start following you.
- **`Shift+E`** dismisses a follower if you change your mind — they'll settle
  back down where they are.

Recruit two or three to start. They follow you like a tamed wolf, so you can
walk them home. Don't march them through a swamp at night — they fight, but
they can die, and **dead settlers don't come back**.

## Step 3 — Build the banner

Pick your spot. Anywhere works, but flat ground near your base is ideal.

Stand near a **workbench**, open the hammer, and find **Settlement Banner**
under the **Misc** tab.

```
Settlement Banner — 10 Wood, 4 Fine Wood, 20 Coins
```

Place it. That's your settlement founded. Everything within **32 metres** of the
banner is now part of it — that's roughly a 15-floor-tile radius, comfortably
big enough for a starter base.

Hover the banner any time to see who lives there and what they're doing.
**Press `E` on the banner** to open the management panel — every settler with
their rank, job and hunger, and `<` / `>` buttons to reassign jobs from one
screen. **`Shift+E` names your settlement** (signs-style text box); the name
shows on the banner and for everyone on your server.

## Step 4 — Move your settlers in

With a follower standing inside that 32 m radius, **press `E` on them**. You'll
get "*settles here!*" and they'll stop following you and stay put.

A settlement holds **10 settlers** by default.

## Step 5 — Give them jobs

Press `E` on a settled villager again to cycle their job. Keep pressing to
scroll through all six:

| Job | What they do | Needs |
|---|---|---|
| **Villager** | Nothing. The default — just lives there | — |
| **Lumberjack** | Drops **2–4 wood** into a settlement chest | — |
| **Farmer** | Drops **1–2 carrots or turnips**, with a 1-in-5 chance of honey | A **beehive** for the honey |
| **Builder** | Repairs up to **3 damaged structures** in the settlement | A **workbench** in the radius |
| **Blacksmith** | Smelts ore sitting in your chests | A **forge** in the radius |
| **Guard** | No production — patrols with **60% wider awareness** for threats | — |
| **Cook** | Cooks raw meat and fish found in your chests | A **cooking station** in the radius |
| **Miner** | Drops **2–4 stone**, with a rare piece of copper or tin ore | — |
| **Hunter** | Drops **1–2 raw meat**, often deer hide, sometimes feathers | — |
| **Brewer** | Turns 2 honey into a minor healing mead, 2 barley into barley wine | A **fermenter** in the radius |

They work roughly **once a minute**, whether or not you're watching, as long as
the area is loaded.

The jobs feed each other: a **hunter** fills chests with raw meat, a **cook**
turns it into proper food, and that food is what keeps everyone fed (Step 7) —
a two-settler food chain that makes the settlement self-sufficient.

`Shift+E` on a settled villager pulls them back out to follow you again.

## Step 6 — Put down chests (don't skip this)

**Jobs need somewhere to put things.** Place at least one chest inside the
settlement radius. Without one, your lumberjacks and farmers work and produce
nothing, silently.

Three things worth knowing:

- **A full chest stops production.** They won't find another one if the nearest
  one with room is gone. Keep space free.
- **A blacksmith with ore missing burns wood into coal.** With a forge built,
  they try copper ore, tin ore, then iron scrap — and if none of those are in
  your chests, they fall back to converting wood into coal. Keep ore stocked,
  or park the firewood in a chest outside the radius.
- **Settlers eat from these chests too.** See the next step.

## Step 7 — Feed your people

Settlers eat **one food item roughly once per in-game day**, taken from your
settlement chests — always the **cheapest food first**, so nobody touches your
serpent stew while there are berries in the box.

A settler that finds nothing to eat goes **hungry and stops working** until
their next meal. You'll see it on their hover text and on the banner.

Keep the pantry stocked and the settlement takes care of its own future:
each night, a settlement below its settler cap has a chance to **attract a
newcomer** — as long as there's a **spare unclaimed bed** and about **3 food**
in the chests to spare (consumed when they arrive). Rarely, the newcomer is a
seer. Build beds ahead of your population and the village grows on its own,
which also means raid losses heal with time instead of being forever.

## Step 8 — Expect trouble

A settlement is a target. Two things can come for it:

- **Bandit raids** — once you've killed Eikthyr, your settlement counts as a
  base for the game's own raid system. You'll get the message *"The clanless are
  raiding!"* the same way you'd get "The forest is moving."
- **Rival clans** — each night there's a **15% chance** a war party of three to
  five bandits attacks a settlement directly.

Your settlers fight back on your side automatically. A couple of **Guards**, a
palisade, and a few workbench-repairable walls go a long way. Builders will
patch up the damage afterwards — and a fed settlement regrows lost settlers
over time.

Settlers who stick around get better at this: a day of service earns 1 XP and
every battle survived earns 2, promoting them to **Veteran** (1 star) at 20 XP
and **Elite** (2 stars) at 60. Stars mean vanilla stat scaling — an Elite guard
is a genuinely dangerous opponent for a war party. One more reason to keep
your people alive and fed.

Want to fight back at the source? The raiders live in **clanless camps**
scattered through the world (`vs_find camp` points you at the nearest).
Destroy the **war totem** at a camp's center and rival raids get permanently
5% less likely — clear ten camps and the native raid event stops entirely.
Raids also scale: bigger settlements draw bigger war parties, and raiders
come starred once The Elder and Bonemass are dead.

If raids aren't your thing, you can turn them off entirely in the config.

## Quick reference

```
Recruit a villager ............ E          (50 coins)
Dismiss a follower ............ Shift + E
Settle a follower ............. E          (inside banner radius)
Change a settler's job ........ E          (cycles through six)
Unassign a settler ............ Shift + E

Settlement Banner ............. Hammer -> Misc  (10 wood, 4 fine wood, 20 coins)
Settlement radius ............. 32 m
Max settlers .................. 10
Work tick ..................... every 60 seconds
Meals ......................... 1 food per settler per ~game day, cheapest first
Growth ........................ spare unclaimed bed + 3 food + below cap
```

## If something isn't working

| Problem | Why |
|---|---|
| No villages anywhere | You're in terrain generated before you installed the mod. Explore somewhere new |
| No banner in the hammer menu | You need to be near a workbench, and it's under **Misc** |
| "No settlement banner nearby" | Your follower is outside the 32 m radius — walk them closer |
| Settlers work but nothing appears | No chest inside the radius, or every chest is full |
| A settler stopped working | Probably hungry — check the hover text, stock food in a chest |
| Blacksmith or builder does nothing | They need a forge / workbench inside the radius |
| Wood keeps turning into coal | A blacksmith with a forge but no ore to smelt. See Step 6 |
| No newcomers ever arrive | Needs a spare unclaimed bed, ~3 food in chests, and room below the cap |
| Friends can't join my server | Everyone — server included — needs the mod at the same version |

Every number above is a default. All of them are adjustable in
`BepInEx/config/com.abjumb.vikingsettlements.cfg` — see the
[configuration table](../README.md#configuration).
