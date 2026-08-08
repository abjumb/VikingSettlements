using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using VikingSettlements.Npcs;

namespace VikingSettlements.Settlements
{
    /// <summary>
    /// The settlement management panel, opened by interacting with the
    /// banner: lists every assigned settler with name, rank, job and hunger,
    /// and lets the player reassign jobs without walking around pressing E
    /// on each villager. Client-side UI; job changes go through the normal
    /// ZDO ownership path so they sync like any other assignment.
    /// </summary>
    internal static class SettlementPanel
    {
        private const float PanelWidth = 640f;
        private const float RowHeight = 34f;
        private const float HeaderHeight = 100f;
        private const float FooterHeight = 64f;
        private const int MaxRows = 14;

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
            var height = HeaderHeight + FooterHeight + Mathf.Max(1, rows) * RowHeight + 16f;

            _panel = GUIManager.Instance.CreateWoodpanel(
                GUIManager.CustomGUIFront.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, PanelWidth, height);
            _panel.AddComponent<PanelBehaviour>();

            // Header: settlement name, population, rename button.
            GUIManager.Instance.CreateText(
                _settlement.DisplayName,
                _panel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -36f),
                GUIManager.Instance.AveriaSerifBold, 26, GUIManager.Instance.ValheimOrange,
                true, Color.black, 460f, 40f, false);

            var hungryCount = 0;
            foreach (var settler in settlers)
            {
                if (settler.IsHungry)
                {
                    hungryCount++;
                }
            }
            var subtitle = Localization.instance.Localize(
                $"$vs_settlers: {settlers.Count}/{ModConfig.MaxSettlersPerSettlement.Value}"
                + (hungryCount > 0 ? $"   $vs_hungry: {hungryCount}" : ""));
            GUIManager.Instance.CreateText(
                subtitle,
                _panel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -68f),
                GUIManager.Instance.AveriaSerif, 17, Color.white,
                true, Color.black, 460f, 26f, false);

            var renameButton = GUIManager.Instance.CreateButton(
                Localization.instance.Localize("$vs_rename"),
                _panel.transform, new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-72f, -42f), 110f, 34f);
            renameButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                var settlement = _settlement;
                Close();
                if (settlement != null && TextInput.instance != null)
                {
                    TextInput.instance.RequestText(settlement, "$vs_rename_topic", 30);
                }
            });

            // Settler rows.
            if (settlers.Count == 0)
            {
                GUIManager.Instance.CreateText(
                    Localization.instance.Localize("$vs_nosettlers"),
                    _panel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(0f, -(HeaderHeight + RowHeight / 2f)),
                    GUIManager.Instance.AveriaSerif, 17, Color.gray,
                    true, Color.black, 460f, 28f, false);
            }
            for (var i = 0; i < rows; i++)
            {
                BuildRow(settlers[i], -(HeaderHeight + RowHeight * i + RowHeight / 2f));
            }
            if (settlers.Count > MaxRows)
            {
                GUIManager.Instance.CreateText(
                    $"+{settlers.Count - MaxRows}",
                    _panel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(0f, -(HeaderHeight + RowHeight * MaxRows + 10f)),
                    GUIManager.Instance.AveriaSerif, 15, Color.gray,
                    true, Color.black, 460f, 24f, false);
            }

            // Footer.
            var closeButton = GUIManager.Instance.CreateButton(
                Localization.instance.Localize("$vs_close"),
                _panel.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 38f), 140f, 38f);
            closeButton.GetComponent<Button>().onClick.AddListener(Close);
        }

        private static void BuildRow(SettlerRecruitable settler, float y)
        {
            var label = settler.GetHoverName()
                        + " — " + Localization.instance.Localize(SettlerRecruitable.JobToken(settler.Job))
                        + (settler.IsHungry ? Localization.instance.Localize(" <color=orange>$vs_hungry</color>") : "");
            GUIManager.Instance.CreateText(
                label,
                _panel.transform, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(230f, y),
                GUIManager.Instance.AveriaSerif, 17, Color.white,
                true, Color.black, 400f, 28f, false);

            var previousButton = GUIManager.Instance.CreateButton(
                "<", _panel.transform, new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-118f, y), 40f, 28f);
            previousButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                ChangeJob(settler, -1);
            });

            var nextButton = GUIManager.Instance.CreateButton(
                ">", _panel.transform, new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-68f, y), 40f, 28f);
            nextButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                ChangeJob(settler, 1);
            });
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
