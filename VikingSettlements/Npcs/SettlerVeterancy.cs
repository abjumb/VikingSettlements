using UnityEngine;

namespace VikingSettlements.Npcs
{
    /// <summary>
    /// Settlers earn experience and rise through the game's native star
    /// levels: 1 XP per in-game day of service while assigned, 2 XP per
    /// battle survived (taking damage from an attacker and living, with a
    /// cooldown so one long fight is a single battle). Star levels carry
    /// vanilla stat scaling, so veterans genuinely hold the line in raids.
    /// XP and level both live in the ZDO and persist with the world.
    /// </summary>
    public class SettlerVeterancy : MonoBehaviour
    {
        public const string XpKey = "vs_xp";

        /// <summary>Raids this settler's settlement has weathered with them in it.</summary>
        public const string RaidsKey = "vs_raids";
        private const int EpithetAtRaids = 3;

        private const string LastServiceDayKey = "vs_xpday";

        private static readonly string[] Epithets =
        {
            "$vs_ep1", "$vs_ep2", "$vs_ep3", "$vs_ep4", "$vs_ep5", "$vs_ep6",
        };

        private const int MaxLevel = 3; // vanilla displays two stars
        private const int ServiceXp = 1;
        private const int BattleXp = 2;
        private const float BattleCooldownSeconds = 120f;

        private ZNetView _nview;
        private Character _character;
        private SettlerRecruitable _settler;
        private float _battleCooldown;

        private void Awake()
        {
            _nview = GetComponent<ZNetView>();
            _character = GetComponent<Character>();
            _settler = GetComponent<SettlerRecruitable>();
            if (_character != null)
            {
                _character.m_onDamaged += OnDamaged;
            }
        }

        private void OnDestroy()
        {
            if (_character != null)
            {
                _character.m_onDamaged -= OnDamaged;
            }
        }

        private void Update()
        {
            if (_battleCooldown > 0f)
            {
                _battleCooldown -= Time.deltaTime;
            }
            if (_nview == null || !_nview.IsValid() || !_nview.IsOwner())
            {
                return;
            }
            if (!ModConfig.VeterancyEnabled.Value || _character == null || _character.IsDead())
            {
                return;
            }
            if (_settler == null || _settler.State != SettlerState.Assigned || EnvMan.instance == null)
            {
                return;
            }

            var day = EnvMan.instance.GetCurrentDay();
            var lastServiceDay = _nview.GetZDO().GetInt(LastServiceDayKey, -1);
            if (lastServiceDay < 0)
            {
                _nview.GetZDO().Set(LastServiceDayKey, day);
            }
            else if (day > lastServiceDay)
            {
                _nview.GetZDO().Set(LastServiceDayKey, day);
                AddXp(ServiceXp);
            }
        }

        // Combat experience: any settler that gets hurt and lives learns from
        // it - wild villages harden over time too.
        private void OnDamaged(float damage, Character attacker)
        {
            if (_nview == null || !_nview.IsValid() || !_nview.IsOwner())
            {
                return;
            }
            if (!ModConfig.VeterancyEnabled.Value || attacker == null || damage <= 0f)
            {
                return;
            }
            if (_character.IsDead() || _battleCooldown > 0f)
            {
                return;
            }
            _battleCooldown = BattleCooldownSeconds;
            AddXp(BattleXp);
        }

        private void AddXp(int amount)
        {
            var zdo = _nview.GetZDO();
            var xp = zdo.GetInt(XpKey) + amount;
            zdo.Set(XpKey, xp);

            var level = _character.GetLevel();
            if (level >= MaxLevel)
            {
                return;
            }
            // First star at XpPerStar, second at three times that.
            var threshold = level <= 1
                ? ModConfig.XpPerStar.Value
                : ModConfig.XpPerStar.Value * 3;
            if (xp < threshold)
            {
                return;
            }

            _character.SetLevel(level + 1);

            var player = Player.m_localPlayer;
            if (player != null
                && Vector3.Distance(player.transform.position, transform.position) < 30f)
            {
                player.Message(MessageHud.MessageType.TopLeft,
                    Localization.instance.Localize($"{_character.m_name} $vs_levelup"));
            }
        }

        /// <summary>
        /// The saga epithet of a settler who has stood through three raids:
        /// " the Unbroken" and kin, chosen deterministically from the name so
        /// it never changes once earned. Empty until earned.
        /// </summary>
        internal static string EpithetToken(ZNetView view, Character character)
        {
            if (view == null || !view.IsValid() || character == null)
            {
                return "";
            }
            if (view.GetZDO().GetInt(RaidsKey) < EpithetAtRaids)
            {
                return "";
            }
            var name = character.m_name ?? "";
            var index = (int)((uint)name.GetHashCode() % (uint)Epithets.Length);
            return " " + Epithets[index];
        }

        /// <summary>Rank tag for hover texts, empty for unproven settlers.</summary>
        internal static string RankToken(Character character)
        {
            if (character == null)
            {
                return "";
            }
            switch (character.GetLevel())
            {
                case 2: return " ($vs_veteran)";
                case 3: return " ($vs_elite)";
                default: return "";
            }
        }
    }
}
