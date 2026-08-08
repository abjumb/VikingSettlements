using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using VikingSettlements.Npcs;

namespace VikingSettlements.Settlements
{
    /// <summary>
    /// "Who lives here?" - opened with the talk key while looking at a door
    /// inside a settlement. Lists the settlement's settlers; one click moves
    /// a settler into this door (displacing the previous occupant, and any
    /// previous home the settler had). Settlers with a home work at full
    /// speed - homeless ones at half.
    /// </summary>
    internal static class HomeAssignPanel
    {
        private const float PanelWidth = 520f;
        private const float RowHeight = 36f;
        private const int MaxRows = 10;

        private static GameObject _panel;
        private static Door _door;
        private static PlayerSettlement _settlement;

        internal static bool IsOpen => _panel != null;

        public static void Open(Door door, PlayerSettlement settlement)
        {
            Close();
            if (door == null || settlement == null
                || GUIManager.Instance == null || GUIManager.CustomGUIFront == null)
            {
                return;
            }
            _door = door;
            _settlement = settlement;
            Build();
            GUIManager.BlockInput(true);
        }

        public static void Close()
        {
            if (_panel != null)
            {
                Object.Destroy(_panel);
                _panel = null;
                GUIManager.BlockInput(false);
            }
            _door = null;
            _settlement = null;
        }

        private static void Rebuild()
        {
            var door = _door;
            var settlement = _settlement;
            Close();
            Open(door, settlement);
        }

        private static void Build()
        {
            var settlers = _settlement.GetSettlers();
            var rows = Mathf.Min(settlers.Count, MaxRows);
            var height = 96f + Mathf.Max(1, rows) * RowHeight + 64f;

            _panel = GUIManager.Instance.CreateWoodpanel(
                GUIManager.CustomGUIFront.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, PanelWidth, height);
            _panel.AddComponent<PanelBehaviour>();

            GUIManager.Instance.CreateText(
                Localization.instance.Localize("$vs_home_title"),
                _panel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -36f),
                GUIManager.Instance.AveriaSerifBold, 24, GUIManager.Instance.ValheimOrange,
                true, Color.black, 460f, 36f, false);

            GUIManager.Instance.CreateText(
                _settlement.DisplayName,
                _panel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -66f),
                GUIManager.Instance.AveriaSerif, 16, Color.gray,
                true, Color.black, 460f, 26f, false);

            if (settlers.Count == 0)
            {
                GUIManager.Instance.CreateText(
                    Localization.instance.Localize("$vs_nosettlers"),
                    _panel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(0f, -(96f + RowHeight / 2f)),
                    GUIManager.Instance.AveriaSerif, 17, Color.gray,
                    true, Color.black, 460f, 28f, false);
            }
            for (var i = 0; i < rows; i++)
            {
                BuildRow(settlers[i], -(96f + RowHeight * i + RowHeight / 2f));
            }

            var closeButton = GUIManager.Instance.CreateButton(
                Localization.instance.Localize("$vs_close"),
                _panel.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 36f), 140f, 38f);
            closeButton.GetComponent<Button>().onClick.AddListener(Close);
        }

        private static void BuildRow(SettlerRecruitable settler, float y)
        {
            var doorPosition = _door.transform.position;
            var livesHere = SettlerHousing.LivesAt(settler, doorPosition);
            var housedElsewhere = !livesHere && SettlerHousing.HasHome(settler);

            var label = settler.GetHoverName()
                + " — " + Localization.instance.Localize(SettlerRecruitable.JobToken(settler.Job))
                + (livesHere
                    ? Localization.instance.Localize(" <color=#a6ffa6>($vs_home_occupant)</color>")
                    : housedElsewhere
                        ? Localization.instance.Localize(" <color=grey>($vs_talk_home)</color>")
                        : "");
            GUIManager.Instance.CreateText(
                label,
                _panel.transform, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(36f, y),
                GUIManager.Instance.AveriaSerif, 17, Color.white,
                true, Color.black, 340f, 30f, false);

            var button = GUIManager.Instance.CreateButton(
                Localization.instance.Localize(livesHere ? "$vs_home_unassign" : "$vs_home_assign"),
                _panel.transform, new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-84f, y), 110f, 30f);
            button.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (livesHere)
                {
                    SettlerHousing.ClearHome(settler);
                }
                else
                {
                    // One settler per door: whoever lived here moves out.
                    foreach (var other in _settlement.GetSettlers())
                    {
                        if (SettlerHousing.LivesAt(other, doorPosition))
                        {
                            SettlerHousing.ClearHome(other);
                        }
                    }
                    SettlerHousing.AssignHome(settler, doorPosition);
                }
                Rebuild();
            });
        }

        /// <summary>Closes on Escape or when the player walks away from the door.</summary>
        private class PanelBehaviour : MonoBehaviour
        {
            private void Update()
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    HomeAssignPanel.Close();
                    return;
                }
                var player = Player.m_localPlayer;
                if (_door == null || player == null
                    || Vector3.Distance(player.transform.position, _door.transform.position) > 8f)
                {
                    HomeAssignPanel.Close();
                }
            }
        }
    }
}
