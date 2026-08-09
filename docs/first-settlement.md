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

> Deeper detail on everything in this step — standing tiers, donation math,
> veteran shopping, follower safety — lives in the
> [complete recruiting guide](recruiting.md).


Walk up to any villager. You'll see **"Recruit (50 Coins)"** in the hover text.

- **Press `E`** to hire them. The coins come out of your inventory and they'll
  start following you.
- **`Shift+E`** dismisses a follower if you change your mind — they'll settle
  back down where they are.

Villages keep track of how you treat them — the **standing** on a settler's
hover text. Donate 10 coins with `Shift+E` to raise it, defend the village
when monsters attack, and recruits get cheaper (half price at Honored).
Attack or kill villagers and prices climb until they refuse you entirely.

The fastest honest coin-and-standing combo is the **village bounty board**
by the plaza: press `E` for the day's posting — bring the village goods, or
break a named clan's camp — and get paid on the spot, standing included.

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

Press `E` on a settled villager again to cycle their job (or use the banner's
management panel to do it from one screen). Keep pressing to scroll through
all fifteen:

| Job | Every work tick (60 s) | Needs before they start |
|---|---|---|
| **Villager** | Nothing. The default — just lives there | — |
| **Lumberjack** | **2–4 wood** into the nearest chest with room | A chest with space |
| **Farmer** | **1–2 carrots or turnips** (50/50); **20% chance of 1 honey** | A chest with space; a **beehive** in the radius for the honey |
| **Builder** | Repairs up to **3 damaged structures** — free, no materials consumed | A **workbench** in the radius, and something actually damaged |
| **Blacksmith** | One smelt: 1 copper ore → copper, else 1 tin ore → tin, else 1 iron scrap → iron, else **1 wood → coal** | A **forge** in the radius; the ore (or wood) **and room for the result in the same chest** |
| **Guard** | No production — patrols with **60% wider awareness** for threats | — |
| **Cook** | One cook: raw meat, deer meat, neck tail, raw fish, wolf or lox meat → its cooked version | A **cooking station** in the radius; the raw food **and room in the same chest** |
| **Miner** | **2–4 stone**; **15% chance** of 1 copper or tin ore | A chest with space |
| **Hunter** | **1–2 raw meat**; **40% chance** of a deer hide; **20% chance** of 2 feathers | A chest with space |
| **Brewer** | One brew: **2 honey → minor healing mead**, else **2 barley → barley wine** | A **fermenter** in the radius; the ingredients **and room in the same chest** |
| **Courier** | Hauls up to **8 surplus goods** to another settlement and walks back | A second settlement within **300 m**; more than 10 of something in your chests |
| **Herder** | Feeds pen animals, **culls the herd above 4**, tidies loose drops into chests | Tamed animals in the radius; carrots/turnips for feed |
| **Engineer** | Loads the emptiest **ballista tower** with up to 5 bolts from your chests; fletches **4 bolts from 2 wood** when all are full | A **workbench**; a ballista in the radius; wood or bolts in a chest |
| **Innkeeper** | Once per day, pours a round: 1 mead/barley wine from your chests → **morale for every settler present**, Rested refreshed for players | A **mead hall** in the radius; a mead in a chest |
| **Fisher** | **1–2 raw fish** per tick | **Open water** at the settlement's edge; a chest with space |

They work roughly **once a minute**, whether or not you're watching, as long as
the area is loaded — and only while **fed** (see Step 7). Note the smelting,
cooking and brewing jobs are *converters*: they need their input **and** space
for the output **in one chest**, so don't scatter one-of-everything across ten
full boxes.

**Not sure why someone's idle? Ask them.** Press **`T`** while looking at any
settler (or standing next to one) to talk: a panel shows their health, their
hunger and next mealtime, and a live ✓/✗ checklist of everything their job
still needs before they'll work.

**Builders can construct whole buildings.** Build a **Builders' Supply
Chest** (hammer → Misc, 10 wood) and fill it with wood. Then stand exactly
where the new building should go, talk to a builder (`T`), and pick a
blueprint: **cabin** (40 wood), **watchtower** (30 wood) and **fishing
dock** (40 wood) from the start; **longhouse** (100 wood + 10 stone),
**livestock pen** (30 wood), **palisade ring** (80 wood), **ballista
tower** (60 wood + 20 stone) and **mead hall** (120 wood + 20 stone)
once the settlement reaches Village tier; the **stone great-hall** (60 wood
+ 40 stone) at Town tier. A construction site appears at your feet; each work
tick your builders carry materials from the supply chest into it, and the
finished building — beds, door, chest and all — goes up when the cost is
paid. If the supply chest runs dry you'll get a warning, and any
lumberjacks and miners you have will automatically top it back up.
`Shift+E` on the site cancels the project.

**Give everyone a home.** Press **`T` on a door** inside the settlement to
choose who lives behind it — one settler per door, and blueprint cabins
come with a door and a bed. A settler with a home works at full speed; a
homeless one works at **half speed** and will tell you so when you talk to
them. Cabins first, then jobs.

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

Well-run settlements become homes in the fullest sense: two **housed,
happy settlers can marry**. Couples keep each other's spirits up,
settlements with couples grow **50% faster**, and sometimes the newcomer
is a **child of the settlement come of age**. Careful, though — if a
settler's partner dies, the widow grieves hard. An **Innkeeper** with a
**mead hall** helps everyone's mood along, one poured round per day —
and refreshes *your* Rested buff while you're in the settlement.

## Step 8 — Expect trouble

A settlement is a target. Two things can come for it:

- **Bandit raids** — once you've killed Eikthyr, your settlement counts as a
  base for the game's own raid system. You'll get the message *"The clanless are
  raiding!"* the same way you'd get "The forest is moving."
- **Rival clans** — each night there's a **15% chance** a war party of three to
  five bandits attacks a settlement directly. The attackers have names: every
  bandit camp belongs to one of **eight clans**, and your settlement is raided
  by the clan of its nearest camp — the raid message tells you who came.

Raids can also **carry someone off**: about one raid in five abducts an
assigned settler to the clan's camp. The banner shows who was taken and how
many days you have (7 by default) — destroy that camp's **war totem** in time
and they walk home with their name, stars and gear; wait too long and they're
gone forever, and the whole settlement's morale takes the blow. Party members
travelling with you are never abducted.

Your settlers fight back on your side automatically. A couple of **Guards**, a
palisade, and a few workbench-repairable walls go a long way. Builders will
patch up the damage afterwards — and a fed settlement regrows lost settlers
over time.

From Village tier your builders can raise real **defense works**: the
**palisade ring** blueprint plants a wall of stakes around wherever you
stand, and the **ballista tower** mounts a Settlement Ballista that shoots
only enemies — it can't hit you, your settlers or your animals. Give someone
the **Engineer** job and the towers stay loaded without you touching them.

A **Village Seer** earns their keep in war too: assigned to your settlement,
a seer heals hurt settlers every work tick and **senses raids a night before
they land**, turning ambushes into appointments.

None of it is forgotten: the banner keeps a **Settlement Saga** (Saga
button in the management panel) — every raid and who sent it, every
warlord slain, wedding, loss and rescue. Settlers who stand through
**three raids** earn a permanent epithet like *the Unbroken* next to
their name. Fighting alongside them, plant a **Rally Standard** (hammer →
Misc, 6 wood + 2 leather scraps) where the line must hold — `E` sends
your war party to fight at the standard, and `Y` focus-fires the enemy
under your crosshair.

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
Post / bring a follower ....... E          (away from any banner)
Party: follow / hold all ...... G
Party: fall back! ............. H          (they disengage and run to you)
Party: focus-fire ............. Y          (strike the enemy under your crosshair)
Rally the party ............... E          (on a built Rally Standard)
Change a settler's job ........ E          (cycles all fifteen)
Unassign a settler ............ Shift + E
Talk to a settler ............. T          (health, hunger, job needs)
Order a building .............. T on a builder, stand where it goes
Assign a home ................. T on a door
Cancel a construction site .... Shift + E on the site

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
| Follower "waits here" instead of settling | You pressed E away from any banner, which posts a party member. Walk them inside the 32 m radius and press E again |
| Settlers work but nothing appears | No chest inside the radius, or every chest is full |
| A settler stopped working | Probably hungry — check the hover text, stock food in a chest |
| Blacksmith or builder does nothing | They need a forge / workbench inside the radius |
| Wood keeps turning into coal | A blacksmith with a forge but no ore to smelt. See Step 6 |
| No newcomers ever arrive | Needs a spare unclaimed bed, ~3 food in chests, and room below the cap |
| Friends can't join my server | Everyone — server included — needs the mod at the same version |

Every number above is a default. All of them are adjustable in
`BepInEx/config/com.abjumb.vikingsettlements.cfg` — see the
[configuration table](../README.md#configuration).
