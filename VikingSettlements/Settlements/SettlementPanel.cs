using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using VikingSettlements.Npcs;

namespace VikingSettlements.Settlements
{
    /// <summary>
    /// The settlement management panel, opened by interacting with the
    /// banner, laid out per the design system's redesign: header with
    /// population bar and hungry count, column headers, one row per settler
    /// (level badge, name with rank stars, job stepper well, status column),
    /// and a footer with the storage hint and Close. Job changes go through
    /// the normal ZDO ownership path so they sync like any other assignment.
    /// </summary>
    internal static class SettlementPanel
    {
        private const float PanelWidth = 720f;
        private const float HeaderHeight = 96f;
        private const float ColumnHeaderHeight = 26f;
        private const float RowHeight = 48f;
        private const float FooterHeight = 72f;
        private const int MaxRows = 10;

        private static GameObject _panel;
        private static PlayerSettlement _settlement;

        internal static bool IsOpen => _panel != null;

        public static void Toggle(PlayerSettlement settlement)
        {
            if (_panel != null && _settlement == settlement)
            {
                Close();
                return;
            }
            Open(settlement);
        }

        public static void Open(PlayerSettlement settlement)
        {
            Close();
            if (settlement == null || GUIManager.Instance == null || GUIManager.CustomGUIFront == null)
            {
                return;
            }
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
            _settlement = null;
        }

        private static void Rebuild()
        {
            var settlement = _settlement;
            Close();
            Open(settlement);
        }

        private static void Build()
        {
            var settlers = _settlement.GetSettlers();
            var rows = Mathf.Min(settlers.Count, MaxRows);
            var height = HeaderHeight + ColumnHeaderHeight
                + Mathf.Max(1, rows) * RowHeight
                + (settlers.Count > MaxRows ? 22f : 0f)
                + FooterHeight;

            _panel = GUIManager.Instance.CreateWoodpanel(
                GUIManager.CustomGUIFront.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, PanelWidth, height);
            _panel.AddComponent<PanelBehaviour>();

            BuildHeader(settlers);
            BuildColumnHeaders();

            if (settlers.Count == 0)
            {
                var empty = Text(
                    Localization.instance.Localize("$vs_nosettlers"),
                    new Vector2(0.5f, 1f), new Vector2(0f, -(HeaderHeight + ColumnHeaderHeight + RowHeight / 2f)),
                    17, UiPalette.SecondaryOnWood, 600f, 30f);
                empty.alignment = TextAnchor.MiddleCenter;
            }
            for (var i = 0; i < rows; i++)
            {
                BuildRow(settlers[i], -(HeaderHeight + ColumnHeaderHeight + RowHeight * i + RowHeight / 2f));
            }
            if (settlers.Count > MaxRows)
            {
                var more = Text($"+{settlers.Count - MaxRows}",
                    new Vector2(0.5f, 1f),
                    new Vector2(0f, -(HeaderHeight + ColumnHeaderHeight + RowHeight * MaxRows + 10f)),
                    14, UiPalette.SecondaryOnWood, 200f, 20f);
                more.alignment = TextAnchor.MiddleCenter;
            }

            BuildFooter();
        }

        private static void BuildHeader(System.Collections.Generic.List<SettlerRecruitable> settlers)
        {
            // Title, left-aligned over the content column.
            var title = Text(_settlement.DisplayName,
                new Vector2(0f, 1f), new Vector2(26f + 190f, -34f),
                26, GUIManager.Instance.ValheimOrange, 380f, 38f,
                GUIManager.Instance.AveriaSerifBold);
            title.alignment = TextAnchor.MiddleLeft;

            // Population line: tier, count, bar, hungry warning.
            var count = settlers.Count;
            var capacity = _settlement.SettlerCap;
            var countText = Text(
                Localization.instance.Localize(
                    $"{PlayerSettlement.TierToken(_settlement.Tier)} — $vs_settlers {count}/{capacity}"),
                new Vector2(0f, 1f), new Vector2(26f + 115f, -68f),
                15, UiPalette.Beige, 230f, 22f);
            countText.alignment = TextAnchor.MiddleLeft;

            var barAnchor = new Vector2(0f, 1f);
            var barLeft = 26f + 230f + 16f;
            UiPalette.CreateRect(_panel.transform, barAnchor, new Vector2(barLeft + 75f, -68f), 150f, 8f, UiPalette.BarTrack);
            var fillWidth = capacity > 0 ? 150f * Mathf.Clamp01((float)count / capacity) : 0f;
            if (fillWidth > 0f)
            {
                UiPalette.CreateRect(_panel.transform, barAnchor,
                    new Vector2(barLeft + fillWidth / 2f, -68f), fillWidth, 8f, UiPalette.BarFill);
            }

            var hungryCount = 0;
            foreach (var settler in settlers)
            {
                if (settler.IsHungry)
                {
                    hungryCount++;
                }
            }
            if (hungryCount > 0)
            {
                var hungry = Text(
                    Localization.instance.Localize($"{hungryCount} $vs_hungry"),
                    new Vector2(0f, 1f), new Vector2(barLeft + 150f + 14f + 60f, -68f),
                    15, UiPalette.Warning, 120f, 22f);
                hungry.alignment = TextAnchor.MiddleLeft;
            }

            var renameButton = GUIManager.Instance.CreateButton(
                Localization.instance.Localize("$vs_rename"),
                _panel.transform, new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-26f - 55f, -42f), 110f, 36f);
            renameButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                var settlement = _settlement;
                Close();
                if (settlement != null && TextInput.instance != null)
                {
                    TextInput.instance.RequestText(settlement, "$vs_rename_topic", 30);
                }
            });
        }

        private static void BuildColumnHeaders()
        {
            var y = -(HeaderHeight + ColumnHeaderHeight / 2f);
            var settlerHeader = Text(Localization.instance.Localize("$vs_col_settler"),
                new Vector2(0f, 1f), new Vector2(26f + 80f, y), 12, UiPalette.ColumnHeader, 160f, 18f);
            settlerHeader.alignment = TextAnchor.MiddleLeft;
            var jobHeader = Text(Localization.instance.Localize("$vs_col_job"),
                new Vector2(1f, 1f), new Vector2(-108f - 105f, y), 12, UiPalette.ColumnHeader, 210f, 18f);
            jobHeader.alignment = TextAnchor.MiddleLeft;
            var statusHeader = Text(Localization.instance.Localize("$vs_col_status"),
                new Vector2(1f, 1f), new Vector2(-26f - 35f, y), 12, UiPalette.ColumnHeader, 70f, 18f);
            statusHeader.alignment = TextAnchor.MiddleRight;
        }

        private static void BuildRow(SettlerRecruitable settler, float y)
        {
            var anchorLeft = new Vector2(0f, 1f);
            var anchorRight = new Vector2(1f, 1f);
            var character = settler.GetComponent<Character>();
            var level = character != null ? character.GetLevel() : 1;

            // Level badge on button wood.
            UiPalette.CreateRect(_panel.transform, anchorLeft, new Vector2(26f + 26f, y), 52f, 34f, UiPalette.BadgeWood);
            var badge = Text($"Lvl. {level}", anchorLeft, new Vector2(26f + 26f, y), 13, UiPalette.BadgeGold, 52f, 20f);
            badge.alignment = TextAnchor.MiddleCenter;

            // Name with rank stars, rank line beneath (the rank has its own
            // line here, so use the bare name rather than the hover name).
            var stars = level >= 3 ? " <color=#FFE300>★★</color>" : level == 2 ? " <color=#FFE300>★</color>" : "";
            var bareName = character != null ? character.m_name : settler.GetHoverName();
            var name = Text(Localization.instance.Localize(bareName) + stars,
                anchorLeft, new Vector2(96f + 120f, y - 8f), 17, Color.white, 240f, 24f);
            name.alignment = TextAnchor.MiddleLeft;
            var rankToken = level >= 3 ? "$vs_elite" : level == 2 ? "$vs_veteran" : "$vs_rank_settler";
            var rank = Text(Localization.instance.Localize(rankToken),
                anchorLeft, new Vector2(96f + 120f, y + 11f), 13, UiPalette.SecondaryOnWood, 240f, 18f);
            rank.alignment = TextAnchor.MiddleLeft;

            // Job stepper: < [well] >
            var prevButton = GUIManager.Instance.CreateButton(
                "<", _panel.transform, anchorRight, anchorRight, new Vector2(-304f, y), 28f, 28f);
            prevButton.GetComponent<Button>().onClick.AddListener(() => ChangeJob(settler, -1));

            UiPalette.CreateRect(_panel.transform, anchorRight, new Vector2(-208f, y), 148f, 28f, UiPalette.WellDark);
            var job = Text(Localization.instance.Localize(SettlerRecruitable.JobToken(settler.Job)),
                anchorRight, new Vector2(-208f, y), 15, UiPalette.Beige, 148f, 22f);
            job.alignment = TextAnchor.MiddleCenter;

            var nextButton = GUIManager.Instance.CreateButton(
                ">", _panel.transform, anchorRight, anchorRight, new Vector2(-112f, y), 28f, 28f);
            nextButton.GetComponent<Button>().onClick.AddListener(() => ChangeJob(settler, 1));

            // Status column.
            var hungry = settler.IsHungry;
            var status = Text(
                Localization.instance.Localize(hungry ? "$vs_status_hungry" : "$vs_status_working"),
                anchorRight, new Vector2(-26f - 35f, y), 14,
                hungry ? UiPalette.Warning : UiPalette.WorkingGreen, 70f, 20f);
            status.alignment = TextAnchor.MiddleRight;
        }

        private static void BuildFooter()
        {
            var hint = Text(Localization.instance.Localize("$vs_panel_hint"),
                new Vector2(0f, 0f), new Vector2(26f + 210f, 36f),
                13, UiPalette.SecondaryOnWood, 420f, 20f);
            hint.alignment = TextAnchor.MiddleLeft;

            var sagaButton = GUIManager.Instance.CreateButton(
                Localization.instance.Localize("$vs_saga"),
                _panel.transform, new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-26f - 140f - 12f - 55f, 38f), 110f, 38f);
            sagaButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                var settlement = _settlement;
                Close();
                SagaPanel.Open(settlement);
            });

            var closeButton = GUIManager.Instance.CreateButton(
                Localization.instance.Localize("$vs_close"),
                _panel.transform, new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-26f - 70f, 38f), 140f, 38f);
            closeButton.GetComponent<Button>().onClick.AddListener(Close);
        }

        private static Text Text(string value, Vector2 anchor, Vector2 position,
            int size, Color color, float width, float height, Font font = null)
        {
            var go = GUIManager.Instance.CreateText(
                value, _panel.transform, anchor, anchor, position,
                font != null ? font : GUIManager.Instance.AveriaSerif,
                size, color, true, Color.black, width, height, false);
            return go.GetComponent<Text>();
        }

        private static void ChangeJob(SettlerRecruitable settler, int direction)
        {
            if (settler == null)
            {
                Rebuild();
                return;
            }
            var count = SettlerRecruitable.JobCount;
            var next = (SettlerJob)((((int)settler.Job + direction) % count + count) % count);
            settler.SetJob(next);
            Rebuild();
        }

        /// <summary>Closes the panel on Escape or when the player walks away.</summary>
        private class PanelBehaviour : MonoBehaviour
        {
            private void Update()
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    SettlementPanel.Close();
                    return;
                }
                var player = Player.m_localPlayer;
                if (_settlement == null || player == null
                    || Vector3.Distance(player.transform.position, _settlement.transform.position) > 12f)
                {
                    SettlementPanel.Close();
                }
            }
        }
    }
}
