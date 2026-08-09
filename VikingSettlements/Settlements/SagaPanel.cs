using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace VikingSettlements.Settlements
{
    /// <summary>
    /// The settlement's chronicle: a simple scroll of the saga entries the
    /// banner has recorded - raids weathered, warlords slain, weddings,
    /// rescues and losses - newest first. Opened from the management panel.
    /// </summary>
    internal static class SagaPanel
    {
        private const float PanelWidth = 560f;
        private const float LineHeight = 27f;
        private const float LineLeftMargin = 36f;
        private const float LineWidth = PanelWidth - 2f * LineLeftMargin;
        // CreateText positions the CENTER of the text rect, not its left edge.
        private const float LineCenterX = LineLeftMargin + LineWidth / 2f;

        private static GameObject _panel;
        private static PlayerSettlement _settlement;

        public static bool IsOpen => _panel != null;

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

        private static void Build()
        {
            var entries = _settlement.SagaEntries();
            entries.Reverse(); // newest first
            var lines = Mathf.Max(1, entries.Count);
            var height = 118f + lines * LineHeight + 64f;

            _panel = GUIManager.Instance.CreateWoodpanel(
                GUIManager.CustomGUIFront.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, PanelWidth, height);
            _panel.AddComponent<PanelBehaviour>();

            GUIManager.Instance.CreateText(
                Localization.instance.Localize("$vs_saga_title"),
                _panel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -36f),
                GUIManager.Instance.AveriaSerifBold, 24, GUIManager.Instance.ValheimOrange,
                true, Color.black, 500f, 36f, false);

            GUIManager.Instance.CreateText(
                _settlement.DisplayName,
                _panel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -68f),
                GUIManager.Instance.AveriaSerif, 16, UiPalette.SecondaryOnWood,
                true, Color.black, 500f, 26f, false);

            if (entries.Count == 0)
            {
                GUIManager.Instance.CreateText(
                    Localization.instance.Localize("$vs_saga_empty"),
                    _panel.transform, new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(LineCenterX, -110f),
                    GUIManager.Instance.AveriaSerif, 16, UiPalette.SecondaryOnWood,
                    true, Color.black, LineWidth, LineHeight - 2f, false);
            }
            for (var i = 0; i < entries.Count; i++)
            {
                var (day, text) = entries[i];
                GUIManager.Instance.CreateText(
                    Localization.instance.Localize($"$vs_saga_day {day} — {text}"),
                    _panel.transform, new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(LineCenterX, -(110f + i * LineHeight)),
                    GUIManager.Instance.AveriaSerif, 16, UiPalette.Beige,
                    true, Color.black, LineWidth, LineHeight - 2f, false);
            }

            var closeButton = GUIManager.Instance.CreateButton(
                Localization.instance.Localize("$vs_close"),
                _panel.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 36f), 140f, 38f);
            closeButton.GetComponent<Button>().onClick.AddListener(Close);
        }

        private class PanelBehaviour : MonoBehaviour
        {
            private void Update()
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    SagaPanel.Close();
                    return;
                }
                var player = Player.m_localPlayer;
                if (_settlement == null || player == null
                    || Vector3.Distance(player.transform.position, _settlement.transform.position) > 12f)
                {
                    SagaPanel.Close();
                }
            }
        }
    }
}
