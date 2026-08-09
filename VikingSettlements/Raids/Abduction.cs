using System.Globalization;
using Jotunn.Managers;
using UnityEngine;
using VikingSettlements.Npcs;
using VikingSettlements.Settlements;

namespace VikingSettlements.Raids
{
    /// <summary>
    /// Abductions and rescues: a rival raid can carry one assigned settler
    /// off to the clan's camp. The settler is serialized onto the banner in
    /// the same record format the party system stows travellers with, and
    /// comes home - name, stars and gear intact - if the camp's war totem
    /// falls before the deadline. Party members are exempt: their fate is
    /// governed by the party's permadeath contract, not by raid dice.
    /// </summary>
    internal static class Abduction
    {
        public const string CaptiveKey = "vs_captive";
        public const string CaptiveNameKey = "vs_captive_name";
        public const string CaptiveCampKey = "vs_captive_camp";
        public const string CaptiveDayKey = "vs_captive_day";

        /// <summary>
        /// Prefix of the position-stamped global keys camp totems set on
        /// destruction, so a settlement can later tell that its captive's
        /// camp fell even if the settlement was unloaded at the time.
        /// </summary>
        public const string CampClearedAtPrefix = "vs_camp_cleared_at_";

        /// <summary>How far a stamped clear position may sit from the recorded camp center.</summary>
        private const float CampMatchRadius = 32f;

        public static string CampClearedKeyAt(Vector3 position)
        {
            return CampClearedAtPrefix
                + Mathf.RoundToInt(position.x).ToString(CultureInfo.InvariantCulture)
                + "_"
                + Mathf.RoundToInt(position.z).ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Rolled once per rival raid, owner-side. One captive per settlement;
        /// only assigned settlers qualify (party members are Following, so
        /// they never appear in the candidate list).
        /// </summary>
        public static void TryAbduct(PlayerSettlement settlement, int day)
        {
            var view = settlement.View;
            if (view == null || !view.IsValid())
            {
                return;
            }
            var zdo = view.GetZDO();
            if (!string.IsNullOrEmpty(zdo.GetString(CaptiveKey)))
            {
                return;
            }
            if (Random.value >= ModConfig.AbductionChance.Value)
            {
                return;
            }
            var clanIndex = ClanNames.IndexNear(settlement.transform.position, out var campPosition);
            if (clanIndex < 0)
            {
                return; // no camp anywhere: nowhere to take them
            }
            var settlers = settlement.GetSettlers();
            if (settlers.Count == 0)
            {
                return;
            }
            var victim = settlers[Random.Range(0, settlers.Count)];
            var record = Serialize(victim);
            if (record == null)
            {
                return;
            }
            var character = victim.GetComponent<Character>();
            var name = character != null ? character.m_name : "";

            zdo.Set(CaptiveKey, record);
            zdo.Set(CaptiveNameKey, name);
            zdo.Set(CaptiveCampKey, campPosition);
            zdo.Set(CaptiveDayKey, day);

            var victimView = victim.GetComponent<ZNetView>();
            if (victimView != null && victimView.IsValid() && ZNetScene.instance != null)
            {
                victimView.ClaimOwnership();
                ZNetScene.instance.Destroy(victim.gameObject);
            }
            settlement.RecordSaga($"{name} $vs_saga_taken");
            Message(settlement, $"{name} $vs_abducted");
            Jotunn.Logger.LogInfo($"Raiders abducted {name} to the camp at {campPosition}");
        }

        /// <summary>
        /// Ran periodically by the banner owner: frees the captive once their
        /// camp's totem has fallen, or gives them up once the deadline passes.
        /// The rescue check runs first, so a camp cleared while the settlement
        /// was unloaded still counts even past the deadline.
        /// </summary>
        public static void CheckCaptive(PlayerSettlement settlement, int day)
        {
            var view = settlement.View;
            if (view == null || !view.IsValid())
            {
                return;
            }
            var zdo = view.GetZDO();
            var record = zdo.GetString(CaptiveKey);
            if (string.IsNullOrEmpty(record))
            {
                return;
            }
            var name = zdo.GetString(CaptiveNameKey);

            if (CampClearedNear(zdo.GetVec3(CaptiveCampKey, settlement.transform.position)))
            {
                if (SpawnCaptive(record, settlement))
                {
                    ClearCaptive(zdo);
                    settlement.RecordSaga($"{name} $vs_saga_rescued");
                    Message(settlement, $"{name} $vs_rescued");
                }
                return;
            }

            var taken = zdo.GetInt(CaptiveDayKey, day);
            if (day > taken + ModConfig.AbductionDeadlineDays.Value)
            {
                ClearCaptive(zdo);
                Message(settlement, $"{name} $vs_captive_lost");
                if (ModConfig.MoraleEnabled.Value)
                {
                    foreach (var settler in settlement.GetSettlers())
                    {
                        var morale = settler.GetComponent<SettlerMorale>();
                        if (morale != null)
                        {
                            morale.AddMorale(-20);
                        }
                    }
                }
                // A spouse lost to the clanless is a confirmed loss: the
                // widowed partner grieves on top of the settlement's sorrow.
                SettlerFamily.GrieveFor(name, settlement.transform.position);
                settlement.RecordSaga($"{name} $vs_saga_lost");
            }
        }

        /// <summary>Banner hover line for an active abduction, or "" without one.</summary>
        public static string HoverLine(PlayerSettlement settlement)
        {
            var view = settlement.View;
            if (view == null || !view.IsValid() || EnvMan.instance == null)
            {
                return "";
            }
            var zdo = view.GetZDO();
            if (string.IsNullOrEmpty(zdo.GetString(CaptiveKey)))
            {
                return "";
            }
            var name = zdo.GetString(CaptiveNameKey);
            var taken = zdo.GetInt(CaptiveDayKey);
            var left = Mathf.Max(0,
                taken + ModConfig.AbductionDeadlineDays.Value - EnvMan.instance.GetCurrentDay());
            return $"\n<color=orange>$vs_captive: {name} — {left} $vs_captive_days</color>";
        }

        private static void ClearCaptive(ZDO zdo)
        {
            zdo.Set(CaptiveKey, "");
            zdo.Set(CaptiveNameKey, "");
        }

        // Matching is by proximity, not exact key: the stamped position is
        // the totem's, a few meters off the camp center the banner recorded.
        // Also used by bounty boards to check camp-breaking bounties.
        internal static bool CampClearedNear(Vector3 campPosition)
        {
            if (ZoneSystem.instance == null)
            {
                return false;
            }
            foreach (var key in ZoneSystem.instance.GetGlobalKeys())
            {
                if (key == null || !key.StartsWith(CampClearedAtPrefix))
                {
                    continue;
                }
                var parts = key.Substring(CampClearedAtPrefix.Length).Split('_');
                if (parts.Length != 2
                    || !int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var x)
                    || !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var z))
                {
                    continue;
                }
                var dx = campPosition.x - x;
                var dz = campPosition.z - z;
                if (dx * dx + dz * dz <= CampMatchRadius * CampMatchRadius)
                {
                    return true;
                }
            }
            return false;
        }

        private static string Serialize(SettlerRecruitable settler)
        {
            var view = settler.GetComponent<ZNetView>();
            var character = settler.GetComponent<Character>();
            if (view == null || !view.IsValid() || character == null || character.IsDead())
            {
                return null;
            }
            var zdo = view.GetZDO();
            var fields = new[]
            {
                "S",
                settler.gameObject.name.Replace("(Clone)", ""),
                character.m_name,
                character.GetHealth().ToString("F1", CultureInfo.InvariantCulture),
                character.GetLevel().ToString(CultureInfo.InvariantCulture),
                zdo.GetInt(SettlerVeterancy.XpKey).ToString(CultureInfo.InvariantCulture),
                zdo.GetString(SettlerEquipment.SlotKeys[0]),
                zdo.GetString(SettlerEquipment.SlotKeys[1]),
                zdo.GetString(SettlerEquipment.SlotKeys[2]),
                zdo.GetString(SettlerEquipment.SlotKeys[3]),
                zdo.GetString(SettlerEquipment.SlotKeys[4]),
            };
            return string.Join("|", fields);
        }

        private static bool SpawnCaptive(string record, PlayerSettlement settlement)
        {
            var parts = record.Split('|');
            if (parts.Length < 6)
            {
                return false;
            }
            var prefab = PrefabManager.Instance.GetPrefab(parts[1])
                         ?? PrefabManager.Instance.GetPrefab(SettlerPrefabs.Settler);
            if (prefab == null)
            {
                return false;
            }

            var center = settlement.transform.position;
            var angle = Random.value * 360f * Mathf.Deg2Rad;
            var distance = ModConfig.SettlementRadius.Value + 4f;
            var position = center + new Vector3(
                Mathf.Sin(angle) * distance, 0f, Mathf.Cos(angle) * distance);
            if (ZoneSystem.instance != null)
            {
                position.y = ZoneSystem.instance.GetGroundHeight(position);
            }
            var toCenter = center - position;
            toCenter.y = 0f;

            var spawned = Object.Instantiate(prefab, position,
                Quaternion.LookRotation(toCenter.normalized));
            var view = spawned.GetComponent<ZNetView>();
            if (view == null || !view.IsValid())
            {
                Object.Destroy(spawned);
                return false;
            }

            var name = parts[2];
            float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var hp);
            int.TryParse(parts[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out var level);
            int.TryParse(parts[5], NumberStyles.Integer, CultureInfo.InvariantCulture, out var xp);

            var zdo = view.GetZDO();
            zdo.Set(SettlerRecruitable.StateKey, (int)SettlerState.Assigned);
            zdo.Set(SettlerRecruitable.JobKey, (int)SettlerJob.Villager);
            zdo.Set(SettlerRecruitable.HomeKey, center);
            zdo.Set(SettlerVeterancy.XpKey, xp);
            if (!string.IsNullOrEmpty(name))
            {
                zdo.Set(SettlerIdentity.NameKey, name);
            }
            for (var slot = 0; slot < SettlerEquipment.SlotCount && 6 + slot < parts.Length; slot++)
            {
                zdo.Set(SettlerEquipment.SlotKeys[slot], parts[6 + slot]);
            }

            var character = spawned.GetComponent<Character>();
            if (character != null)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    character.m_name = name;
                }
                if (level > 1)
                {
                    character.SetLevel(level);
                }
                if (hp > 0f)
                {
                    character.SetHealth(hp);
                }
            }
            var ai = spawned.GetComponent<MonsterAI>();
            if (ai != null)
            {
                // Walk in from the edge, like a newcomer.
                ai.SetPatrolPoint(center);
            }
            return true;
        }

        private static void Message(PlayerSettlement settlement, string text)
        {
            var player = Player.m_localPlayer;
            if (player != null
                && Vector3.Distance(player.transform.position, settlement.transform.position) < 80f)
            {
                player.Message(MessageHud.MessageType.Center,
                    Localization.instance.Localize(text));
            }
        }
    }
}
