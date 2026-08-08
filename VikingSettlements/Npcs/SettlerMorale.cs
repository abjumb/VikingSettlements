using UnityEngine;

namespace VikingSettlements.Npcs
{
    /// <summary>
    /// Settler morale (0..100, ZDO): adjusted once per in-game day for
    /// assigned settlers from things the mod already tracks - a home, a full
    /// belly, company nearby - and knocked down when their settlement is
    /// raided. Cheerful settlers produce a little extra, miserable ones drag
    /// their feet (see SettlerWork), and a settler left at rock bottom packs
    /// up and leaves the settlement for good.
    /// </summary>
    public class SettlerMorale : MonoBehaviour
    {
        public const string MoraleKey = "vs_morale";
        private const string LastDayKey = "vs_moraleday";

        public const int CheerfulAt = 70;
        public const int MiserableBelow = 30;
        private const int LeaveBelow = 6;
        private const float CompanyRange = 12f;
        private const float TickInterval = 5f;

        private ZNetView _nview;
        private SettlerRecruitable _settler;
        private float _nextTick;

        private void Awake()
        {
            _nview = GetComponent<ZNetView>();
            _settler = GetComponent<SettlerRecruitable>();
        }

        internal int Morale => _nview != null && _nview.IsValid()
            ? Mathf.Clamp(_nview.GetZDO().GetInt(MoraleKey, 50), 0, 100)
            : 50;

        internal static string MoodToken(int morale)
        {
            if (morale >= CheerfulAt)
            {
                return "$vs_mood_cheerful";
            }
            if (morale >= 40)
            {
                return "$vs_mood_content";
            }
            if (morale >= MiserableBelow)
            {
                return "$vs_mood_unhappy";
            }
            return "$vs_mood_miserable";
        }

        /// <summary>Applies a morale change (e.g. the raid shock), owner-side.</summary>
        internal void AddMorale(int delta)
        {
            if (_nview == null || !_nview.IsValid())
            {
                return;
            }
            _nview.ClaimOwnership();
            _nview.GetZDO().Set(MoraleKey,
                Mathf.Clamp(Morale + delta, 0, 100));
        }

        private void Update()
        {
            if (!ModConfig.MoraleEnabled.Value
                || _nview == null || !_nview.IsValid() || !_nview.IsOwner()
                || _settler == null || _settler.State != SettlerState.Assigned
                || EnvMan.instance == null)
            {
                return;
            }
            if (Time.time < _nextTick)
            {
                return;
            }
            _nextTick = Time.time + TickInterval;

            var day = EnvMan.instance.GetCurrentDay();
            var zdo = _nview.GetZDO();
            var lastDay = zdo.GetInt(LastDayKey, -1);
            if (lastDay < 0)
            {
                zdo.Set(LastDayKey, day);
                return;
            }
            if (day <= lastDay)
            {
                return;
            }
            zdo.Set(LastDayKey, day);

            var delta = 0;
            delta += SettlerHousing.HasHome(_settler) ? 10 : -10;
            delta += _settler.IsHungry ? -15 : 10;
            delta += HasCompany() ? 5 : -5;
            var morale = Mathf.Clamp(Morale + delta, 0, 100);
            zdo.Set(MoraleKey, morale);

            if (morale < LeaveBelow && Random.value < 0.5f)
            {
                Leave();
            }
        }

        private bool HasCompany()
        {
            foreach (var other in SettlerRecruitable.Instances)
            {
                if (other != _settler
                    && Vector3.Distance(other.transform.position, transform.position) <= CompanyRange)
                {
                    return true;
                }
            }
            return false;
        }

        // Sustained neglect has a real cost: the settler quits the
        // settlement, keeps their name and levels, and stands where they
        // are as a wild villager - recruitable again by anyone.
        private void Leave()
        {
            var zdo = _nview.GetZDO();
            zdo.Set(SettlerRecruitable.StateKey, (int)SettlerState.Wild);
            zdo.Set(SettlerRecruitable.OwnerKey, 0L);
            zdo.Set(MoraleKey, 40); // a fresh start elsewhere
            var ai = GetComponent<MonsterAI>();
            if (ai != null)
            {
                ai.SetFollowTarget(null);
                ai.SetPatrolPoint();
            }

            var character = GetComponent<Character>();
            var player = Player.m_localPlayer;
            if (character != null && player != null
                && Vector3.Distance(player.transform.position, transform.position) < 60f)
            {
                player.Message(MessageHud.MessageType.Center,
                    Localization.instance.Localize($"{character.m_name} $vs_mood_left"));
            }
            Jotunn.Logger.LogInfo("A miserable settler left their settlement");
        }
    }
}
