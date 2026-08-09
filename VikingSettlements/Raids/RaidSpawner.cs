using Jotunn.Managers;
using UnityEngine;
using VikingSettlements.Npcs;
using VikingSettlements.Settlements;

namespace VikingSettlements.Raids
{
    /// <summary>
    /// Spawns "rival clan" raiding parties that assault a player settlement:
    /// a group of bandits appears at the edge of the settlement and hunts the
    /// inhabitants. Settlers (player faction) fight them natively.
    /// </summary>
    internal static class RaidSpawner
    {
        /// <summary>
        /// Rival raid chance after the world-wide reduction earned by
        /// clearing clanless camps (capped at 50% total reduction).
        /// </summary>
        public static float EffectiveRaidChance()
        {
            var reduction = ModConfig.CampClearRaidReduction.Value * CampTotem.ClearedCampCount();
            return ModConfig.RivalRaidChancePerDay.Value * Mathf.Max(0.5f, 1f - reduction);
        }

        public static void SpawnRivalRaid(PlayerSettlement settlement)
        {
            var raiderPrefab = PrefabManager.Instance.GetPrefab(SettlerPrefabs.Raider);
            if (raiderPrefab == null)
            {
                return;
            }

            // Raids come from the nearest camp's clan; a clan whose warlord
            // has fallen is broken and raids no more.
            var clanIndex = ClanNames.IndexNear(settlement.transform.position, out _);
            if (ClanNames.IsBroken(clanIndex))
            {
                return;
            }

            var center = settlement.transform.position;
            var count = Random.Range(3, 6);
            var maxLevel = 1;
            if (ModConfig.ScaleRaids.Value)
            {
                // Bigger settlements draw bigger war parties.
                count = Mathf.Clamp(3 + settlement.CountAssignedSettlers() / 3, 3, 8);
                // Raiders gain stars as the world's bosses fall.
                if (ZoneSystem.instance != null)
                {
                    if (ZoneSystem.instance.GetGlobalKey("defeated_bonemass"))
                    {
                        maxLevel = 3;
                    }
                    else if (ZoneSystem.instance.GetGlobalKey("defeated_gdking"))
                    {
                        maxLevel = 2;
                    }
                }
            }

            var angle = Random.value * 360f;
            var distance = ModConfig.SettlementRadius.Value + 12f;

            for (var i = 0; i < count; i++)
            {
                var offsetAngle = (angle + Random.Range(-20f, 20f)) * Mathf.Deg2Rad;
                var position = center + new Vector3(
                    Mathf.Sin(offsetAngle) * distance,
                    0f,
                    Mathf.Cos(offsetAngle) * distance);
                position.y = GroundHeight(position);

                var toCenter = center - position;
                toCenter.y = 0f;
                var raider = Object.Instantiate(raiderPrefab, position,
                    Quaternion.LookRotation(toCenter.normalized));

                var view = raider.GetComponent<ZNetView>();
                if (view != null && view.IsValid())
                {
                    view.GetZDO().Set(Npcs.RaiderDespawn.WarPartyKey, true);
                }
                var character = raider.GetComponent<Character>();
                if (character != null && maxLevel > 1 && Random.value < 0.2f)
                {
                    character.SetLevel(Random.Range(2, maxLevel + 1));
                }
                var ai = raider.GetComponent<MonsterAI>();
                if (ai != null)
                {
                    ai.SetHuntPlayer(true);
                    ai.Alert();
                }
            }

            // Raids leave a mark on the people, not just the walls.
            if (ModConfig.MoraleEnabled.Value)
            {
                foreach (var settler in settlement.GetSettlers())
                {
                    var morale = settler.GetComponent<Npcs.SettlerMorale>();
                    if (morale != null)
                    {
                        morale.AddMorale(-20);
                    }
                }
            }

            var player = Player.m_localPlayer;
            if (player != null
                && Vector3.Distance(player.transform.position, center) < 80f)
            {
                player.Message(MessageHud.MessageType.Center,
                    Localization.instance.Localize($"{ClanNames.Token(clanIndex)} $vs_clan_attack"));
            }

            // The counterweight to clearing camps: the clanless eventually
            // send a warlord. Kill him and the settlement earns real peace.
            if (ModConfig.WarlordEnabled.Value
                && CampTotem.ClearedCampCount() >= 3
                && Random.value < ModConfig.WarlordChance.Value)
            {
                SpawnWarlord(center, angle, distance, clanIndex);
            }

            // A raid can carry someone off - rescue them by breaking the
            // clan's camp before the deadline. Rolled last so its message
            // (the actionable one) isn't overwritten by the announcements.
            if (EnvMan.instance != null)
            {
                Abduction.TryAbduct(settlement, EnvMan.instance.GetCurrentDay());
            }

            Jotunn.Logger.LogInfo($"Rival clan raid: {count} raiders assault the settlement at {center}");
        }

        private static void SpawnWarlord(Vector3 center, float angle, float distance, int clanIndex)
        {
            var prefab = PrefabManager.Instance.GetPrefab(SettlerPrefabs.Warlord);
            if (prefab == null)
            {
                return;
            }
            var rad = angle * Mathf.Deg2Rad;
            var position = center + new Vector3(Mathf.Sin(rad) * distance, 0f, Mathf.Cos(rad) * distance);
            position.y = GroundHeight(position);
            var toCenter = center - position;
            toCenter.y = 0f;

            var warlord = Object.Instantiate(prefab, position,
                Quaternion.LookRotation(toCenter.normalized));

            var view = warlord.GetComponent<ZNetView>();
            if (view != null && view.IsValid())
            {
                view.GetZDO().Set(Npcs.RaiderDespawn.WarPartyKey, true);
                // He carries his clan: felling him breaks it (see WarlordFall).
                view.GetZDO().Set(ClanNames.ClanKey, clanIndex);
            }

            // Scale to boss progression, like starred raiders.
            var health = 300f;
            var level = 1;
            if (ZoneSystem.instance != null)
            {
                if (ZoneSystem.instance.GetGlobalKey("defeated_bonemass"))
                {
                    health = 800f;
                    level = 3;
                }
                else if (ZoneSystem.instance.GetGlobalKey("defeated_gdking"))
                {
                    health = 500f;
                    level = 2;
                }
            }
            var character = warlord.GetComponent<Character>();
            if (character != null)
            {
                character.SetLevel(level);
                character.SetMaxHealth(health);
                character.SetHealth(health);
            }
            var ai = warlord.GetComponent<MonsterAI>();
            if (ai != null)
            {
                ai.SetHuntPlayer(true);
                ai.Alert();
            }

            var player = Player.m_localPlayer;
            if (player != null
                && Vector3.Distance(player.transform.position, center) < 80f)
            {
                player.Message(MessageHud.MessageType.Center,
                    Localization.instance.Localize("$vs_warlord_comes"));
            }
            Jotunn.Logger.LogInfo($"A clanless warlord joins the raid at {center}");
        }

        private static float GroundHeight(Vector3 position)
        {
            if (ZoneSystem.instance != null)
            {
                return ZoneSystem.instance.GetGroundHeight(position);
            }
            return position.y;
        }
    }
}
