using System.Collections.Generic;
using System.Globalization;
using Jotunn.Managers;
using UnityEngine;
using VikingSettlements.Npcs;

namespace VikingSettlements.Party
{
    internal enum PartyStance
    {
        Follow = 0,
        Hold = 1,
        Fallback = 2,
    }

    /// <summary>
    /// The local player's war party: up to MaxPartySize recruited villagers
    /// that fight at the player's side and survive every traversal system.
    ///
    /// The registry lives in Player.m_customData (keyed per world), so it
    /// travels with the character save. Members are either live creatures
    /// near the player, or serialized "stowed" blobs while the player is on
    /// a boat, in a portal, or logged out. That invariant - members are at
    /// your side or in your pocket - is what makes the permadeath contract
    /// enforceable: fate can only find them in a fight you are standing in.
    /// </summary>
    internal static class PartySystem
    {
        /// <summary>ZDO flag marking a settler as someone's party member.</summary>
        public const string PartyKey = "vs_party";

        /// <summary>ZDO int holding the member's current PartyStance.</summary>
        public const string StanceKey = "vs_stance";

        /// <summary>
        /// How close the owner must be for a member to be damageable at all
        /// (see PartyPatches). Inside the active area on every server setup.
        /// </summary>
        public const float GuardDistance = 60f;

        private const float TickInterval = 0.5f;
        private const float TetherDistance = 45f;
        private const float WoundedFraction = 0.5f;
        private const float GravelyWoundedFraction = 0.25f;

        private class Entry
        {
            public ZDOID Id = ZDOID.None;
            public string Stowed;
            public string LastName = "";
            public Vector3 LastPosition;
            public float LastHealthFraction = 1f;
        }

        private static readonly List<Entry> _entries = new List<Entry>();
        private static long _loadedFor;
        private static float _nextTick;

        public static void OnUpdate()
        {
            var player = Player.m_localPlayer;
            if (player == null || ZNet.instance == null)
            {
                _loadedFor = 0L;
                return;
            }
            if (_loadedFor != player.GetPlayerID())
            {
                Load(player);
                _loadedFor = player.GetPlayerID();
            }

            HandleHotkeys(player);

            if (Time.time < _nextTick)
            {
                return;
            }
            _nextTick = Time.time + TickInterval;

            AdoptFollowers(player);
            TickMembers(player);
            HandleTraversal(player);
        }

        public static bool HasRoom()
        {
            return _entries.Count < ModConfig.MaxPartySize.Value;
        }

        public static void AddMember(Player player, PartyMember member)
        {
            if (player == null || member == null || player != Player.m_localPlayer)
            {
                return;
            }
            member.MarkMember(player);
            var id = member.Id;
            if (id == ZDOID.None || FindEntry(id) != null)
            {
                return;
            }
            _entries.Add(new Entry
            {
                Id = id,
                LastName = member.MemberName,
                LastPosition = member.transform.position,
                LastHealthFraction = member.HealthFraction,
            });
            Save(player);
        }

        public static void RemoveMember(ZDOID id)
        {
            var entry = FindEntry(id);
            if (entry == null)
            {
                return;
            }
            _entries.Remove(entry);
            if (Player.m_localPlayer != null)
            {
                Save(Player.m_localPlayer);
            }
        }

        /// <summary>
        /// Called from the Game.Logout/Shutdown patches: pockets every
        /// travelling member into the character save before the profile is
        /// written, so logging out can never strand or endanger the party.
        /// Members on Hold stay posted where they were told to stand.
        /// </summary>
        public static void StowForExit()
        {
            var player = Player.m_localPlayer;
            if (player == null || _loadedFor != player.GetPlayerID())
            {
                return;
            }
            StowAll(player, silent: true);
        }

        internal static string StanceToken(PartyStance stance)
        {
            switch (stance)
            {
                case PartyStance.Hold: return "$vs_party_stance_hold";
                case PartyStance.Fallback: return "$vs_party_stance_fallback";
                default: return "$vs_party_stance_follow";
            }
        }

        // ---- hotkeys ------------------------------------------------------

        private static void HandleHotkeys(Player player)
        {
            if (_entries.Count == 0 || UiHasFocus())
            {
                return;
            }
            if (ModConfig.PartyStanceKey.Value.IsDown())
            {
                var anyNotHolding = AnyLiveMemberNotIn(PartyStance.Hold);
                CommandAll(player, anyNotHolding ? PartyStance.Hold : PartyStance.Follow);
            }
            if (ModConfig.PartyFallbackKey.Value.IsDown())
            {
                var anyNotFalling = AnyLiveMemberNotIn(PartyStance.Fallback);
                CommandAll(player, anyNotFalling ? PartyStance.Fallback : PartyStance.Follow);
            }
        }

        internal static bool UiHasFocus()
        {
            return Console.IsVisible()
                || TextInput.IsVisible()
                || Menu.IsVisible()
                || InventoryGui.IsVisible()
                || (Chat.instance != null && Chat.instance.HasFocus());
        }

        private static bool AnyLiveMemberNotIn(PartyStance stance)
        {
            foreach (var entry in _entries)
            {
                if (entry.Stowed != null)
                {
                    continue;
                }
                var member = PartyMember.FindById(entry.Id);
                if (member != null && member.Stance != stance)
                {
                    return true;
                }
            }
            return false;
        }

        private static void CommandAll(Player player, PartyStance stance)
        {
            var commanded = false;
            foreach (var entry in _entries)
            {
                if (entry.Stowed != null)
                {
                    continue;
                }
                var member = PartyMember.FindById(entry.Id);
                if (member == null || member.IsDead)
                {
                    continue;
                }
                member.SetStance(stance, player);
                commanded = true;
            }
            if (!commanded)
            {
                return;
            }
            string token;
            switch (stance)
            {
                case PartyStance.Hold: token = "$vs_party_cmd_hold"; break;
                case PartyStance.Fallback: token = "$vs_party_cmd_fallback"; break;
                default: token = "$vs_party_cmd_follow"; break;
            }
            player.Message(MessageHud.MessageType.Center, Localization.instance.Localize(token));
        }

        // ---- per-tick member upkeep ---------------------------------------

        // Old saves have followers from before the party system existed, and
        // a lost registry (crash before a save) leaves flagged members with
        // no entry. Both converge here: any settler following me becomes a
        // tracked party member while there is room.
        private static void AdoptFollowers(Player player)
        {
            foreach (var member in PartyMember.Instances)
            {
                if (member.IsDead || !member.IsActiveMember
                    || member.RecruiterId != player.GetPlayerID())
                {
                    continue;
                }
                var id = member.Id;
                if (FindEntry(id) != null)
                {
                    continue;
                }
                _entries.Add(new Entry
                {
                    Id = id,
                    LastName = member.MemberName,
                    LastPosition = member.transform.position,
                    LastHealthFraction = member.HealthFraction,
                });
                Save(player);
            }

            // Followers that are not flagged yet (recruited before 1.8).
            foreach (var settler in SettlerRecruitable.Instances)
            {
                if (settler.State != SettlerState.Following
                    || settler.RecruiterId != player.GetPlayerID())
                {
                    continue;
                }
                var member = settler.GetComponent<PartyMember>();
                if (member == null || member.IsActiveMember)
                {
                    continue;
                }
                if (!HasRoom())
                {
                    continue;
                }
                AddMember(player, member);
            }
        }

        private static void TickMembers(Player player)
        {
            for (var i = _entries.Count - 1; i >= 0; i--)
            {
                var entry = _entries[i];
                if (entry.Stowed != null)
                {
                    continue;
                }
                var member = PartyMember.FindById(entry.Id);
                if (member == null)
                {
                    // Out of sight is not dead: the damage contract makes
                    // unattended members untouchable, so they simply wait.
                    continue;
                }
                if (member.IsDead)
                {
                    _entries.RemoveAt(i);
                    Save(player);
                    player.Message(MessageHud.MessageType.Center,
                        Localization.instance.Localize($"{entry.LastName} $vs_party_fallen"));
                    continue;
                }
                if (!member.IsActiveMember)
                {
                    // Assigned to a settlement or dismissed through a path
                    // that missed the registry; stop tracking quietly.
                    _entries.RemoveAt(i);
                    Save(player);
                    continue;
                }

                entry.LastName = member.MemberName;
                entry.LastPosition = member.transform.position;
                WarnHealth(player, entry, member);
                Tether(player, member);
            }
        }

        private static void WarnHealth(Player player, Entry entry, PartyMember member)
        {
            var fraction = member.HealthFraction;
            var previous = entry.LastHealthFraction;
            entry.LastHealthFraction = fraction;
            if (fraction < GravelyWoundedFraction && previous >= GravelyWoundedFraction)
            {
                var token = ModConfig.PartyAutoFallback.Value ? "$vs_party_retreats" : "$vs_party_grave";
                player.Message(MessageHud.MessageType.Center,
                    Localization.instance.Localize($"{member.MemberName} {token}"));
            }
            else if (fraction < WoundedFraction && previous >= WoundedFraction)
            {
                player.Message(MessageHud.MessageType.TopLeft,
                    Localization.instance.Localize($"{member.MemberName} $vs_party_wounded"));
            }
        }

        // Keeps travelling members inside the active area even when pathing
        // fails or the player sprints across zone borders: past the tether
        // they are teleported to the player instead of being left behind.
        private static void Tether(Player player, PartyMember member)
        {
            if (member.Stance == PartyStance.Hold)
            {
                return;
            }
            if (Vector3.Distance(player.transform.position, member.transform.position) < TetherDistance)
            {
                return;
            }
            if (!player.IsOnGround() || player.IsSwimming() || player.IsTeleporting()
                || player.IsAttachedToShip() || player.GetStandingOnShip() != null)
            {
                return;
            }
            member.WarpTo(SpawnPointAround(player, _entries.IndexOf(FindEntry(member.Id))));
        }

        // ---- traversal: boats, portals, logout ----------------------------

        private static void HandleTraversal(Player player)
        {
            var traveling = player.IsTeleporting()
                || player.IsAttachedToShip()
                || player.GetStandingOnShip() != null;
            if (traveling)
            {
                StowAll(player, silent: false);
                return;
            }
            if (!HasStowed())
            {
                return;
            }
            if (player.IsDead() || player.IsSwimming() || !player.IsOnGround())
            {
                return;
            }
            UnstowAll(player);
        }

        private static bool HasStowed()
        {
            foreach (var entry in _entries)
            {
                if (entry.Stowed != null)
                {
                    return true;
                }
            }
            return false;
        }

        private static void StowAll(Player player, bool silent)
        {
            var any = false;
            foreach (var entry in _entries)
            {
                if (entry.Stowed != null)
                {
                    continue;
                }
                var member = PartyMember.FindById(entry.Id);
                if (member == null || member.IsDead || member.Stance == PartyStance.Hold)
                {
                    continue;
                }
                entry.LastName = member.MemberName;
                entry.Stowed = member.SerializeStow();
                entry.Id = ZDOID.None;
                member.DespawnStowed();
                any = true;
            }
            if (!any)
            {
                return;
            }
            Save(player);
            if (!silent)
            {
                player.Message(MessageHud.MessageType.TopLeft,
                    Localization.instance.Localize("$vs_party_aboard"));
            }
        }

        private static void UnstowAll(Player player)
        {
            var any = false;
            var index = 0;
            foreach (var entry in _entries)
            {
                if (entry.Stowed == null)
                {
                    continue;
                }
                var id = SpawnStowed(player, entry.Stowed, index++);
                if (id == ZDOID.None)
                {
                    continue;
                }
                entry.Id = id;
                entry.Stowed = null;
                entry.LastHealthFraction = 1f;
                any = true;
            }
            if (!any)
            {
                return;
            }
            Save(player);
            player.Message(MessageHud.MessageType.TopLeft,
                Localization.instance.Localize("$vs_party_ashore"));
        }

        private static ZDOID SpawnStowed(Player player, string stowed, int index)
        {
            var parts = stowed.Split('|');
            if (parts.Length < 6)
            {
                return ZDOID.None;
            }
            var prefab = PrefabManager.Instance.GetPrefab(parts[1])
                ?? PrefabManager.Instance.GetPrefab(SettlerPrefabs.Settler);
            if (prefab == null)
            {
                return ZDOID.None;
            }

            var spawned = Object.Instantiate(prefab,
                SpawnPointAround(player, index), player.transform.rotation);
            var view = spawned.GetComponent<ZNetView>();
            if (view == null || !view.IsValid())
            {
                Object.Destroy(spawned);
                return ZDOID.None;
            }

            var name = parts[2];
            float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var hp);
            int.TryParse(parts[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out var level);
            int.TryParse(parts[5], NumberStyles.Integer, CultureInfo.InvariantCulture, out var xp);

            var zdo = view.GetZDO();
            zdo.Set(SettlerRecruitable.StateKey, (int)SettlerState.Following);
            zdo.Set(SettlerRecruitable.OwnerKey, player.GetPlayerID());
            zdo.Set(PartyKey, true);
            zdo.Set(StanceKey, (int)PartyStance.Follow);
            zdo.Set(SettlerVeterancy.XpKey, xp);
            if (!string.IsNullOrEmpty(name))
            {
                zdo.Set(SettlerIdentity.NameKey, name);
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
                ai.SetFollowTarget(player.gameObject);
            }
            return zdo.m_uid;
        }

        private static Vector3 SpawnPointAround(Player player, int index)
        {
            var angle = (40f + Mathf.Max(0, index) * 72f) * Mathf.Deg2Rad;
            // Player height, not terrain height: inside dungeons the terrain
            // is hundreds of meters below the interior floor.
            return player.transform.position
                + new Vector3(Mathf.Sin(angle) * 2f, 0.2f, Mathf.Cos(angle) * 2f);
        }

        // ---- console command support --------------------------------------

        public static List<string> Describe(Player player)
        {
            var lines = new List<string>();
            if (_entries.Count == 0)
            {
                lines.Add($"vs_party: no companions - recruit villagers to form a party (up to {ModConfig.MaxPartySize.Value})");
                return lines;
            }
            foreach (var entry in _entries)
            {
                if (entry.Stowed != null)
                {
                    lines.Add($"vs_party: {StowedName(entry.Stowed)} - stowed, traveling with you");
                    continue;
                }
                var member = PartyMember.FindById(entry.Id);
                if (member == null)
                {
                    var distance = Vector3.Distance(player.transform.position, entry.LastPosition);
                    lines.Add($"vs_party: {entry.LastName} - away ({distance:0} m from here; safe until you return, or try 'vs_party recall')");
                    continue;
                }
                var stance = Localization.instance.Localize(StanceToken(member.Stance));
                var health = Mathf.RoundToInt(member.HealthFraction * 100f);
                var range = Vector3.Distance(player.transform.position, member.transform.position);
                lines.Add($"vs_party: {member.MemberName} - {stance} - {health}% - {range:0} m");
            }
            return lines;
        }

        public static string RecallStragglers(Player player)
        {
            var recalled = 0;
            var unreachable = 0;
            var index = 0;
            foreach (var entry in _entries)
            {
                index++;
                if (entry.Stowed != null || PartyMember.FindById(entry.Id) != null)
                {
                    continue;
                }
                var zdo = ZDOMan.instance != null ? ZDOMan.instance.GetZDO(entry.Id) : null;
                if (zdo == null)
                {
                    unreachable++;
                    continue;
                }
                zdo.SetPosition(SpawnPointAround(player, index));
                recalled++;
            }
            if (recalled == 0 && unreachable == 0)
            {
                return "vs_party: everyone is already at your side";
            }
            var result = $"vs_party: {recalled} recalled to you";
            if (unreachable > 0)
            {
                result += $"; {unreachable} out of reach from this client - they are safe, return to where you left them";
            }
            return result;
        }

        private static string StowedName(string stowed)
        {
            var parts = stowed.Split('|');
            return parts.Length > 2 && !string.IsNullOrEmpty(parts[2]) ? parts[2] : "Settler";
        }

        // ---- registry persistence -----------------------------------------

        private static string CustomDataKey => ZNet.instance != null
            ? $"vs_party_{ZNet.instance.GetWorldUID().ToString(CultureInfo.InvariantCulture)}"
            : null;

        private static Entry FindEntry(ZDOID id)
        {
            foreach (var entry in _entries)
            {
                if (entry.Stowed == null && entry.Id == id)
                {
                    return entry;
                }
            }
            return null;
        }

        private static void Save(Player player)
        {
            var key = CustomDataKey;
            if (key == null || player.m_customData == null)
            {
                return;
            }
            var parts = new List<string>();
            foreach (var entry in _entries)
            {
                if (entry.Stowed != null)
                {
                    parts.Add(entry.Stowed);
                }
                else
                {
                    parts.Add(string.Join("|", "L",
                        entry.Id.UserID.ToString(CultureInfo.InvariantCulture)
                        + ":" + entry.Id.ID.ToString(CultureInfo.InvariantCulture),
                        entry.LastName));
                }
            }
            if (parts.Count == 0)
            {
                player.m_customData.Remove(key);
            }
            else
            {
                player.m_customData[key] = string.Join(";", parts);
            }
        }

        private static void Load(Player player)
        {
            _entries.Clear();
            var key = CustomDataKey;
            if (key == null || player.m_customData == null
                || !player.m_customData.TryGetValue(key, out var data)
                || string.IsNullOrEmpty(data))
            {
                return;
            }
            foreach (var part in data.Split(';'))
            {
                var fields = part.Split('|');
                if (fields.Length >= 3 && fields[0] == "L")
                {
                    var idParts = fields[1].Split(':');
                    if (idParts.Length == 2
                        && long.TryParse(idParts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var user)
                        && uint.TryParse(idParts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
                    {
                        _entries.Add(new Entry
                        {
                            Id = new ZDOID(user, id),
                            LastName = fields[2],
                            LastPosition = player.transform.position,
                        });
                    }
                }
                else if (fields.Length >= 6 && fields[0] == "S")
                {
                    _entries.Add(new Entry { Stowed = part, LastName = fields[2] });
                }
            }
        }
    }
}
