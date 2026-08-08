using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using VikingSettlements.Npcs;

namespace VikingSettlements.Settlements
{
    /// <summary>
    /// The equipment panel for a recruited settler, opened from their talk
    /// panel: the five gear slots with take-back buttons, and every
    /// equippable item in the player's inventory with a give button.
    /// </summary>
    internal static class SettlerGearPanel
    {
        private const float PanelWidth = 600f;
        private const float RowHeight = 34f;
        private const int MaxGiveRows = 8;

        private static GameObject _panel;
        private static SettlerRecruitable _settler;

        internal static bool IsOpen => _panel != null;

        public static void Open(SettlerRecruitable settler)
        {
            Close();
            if (settler == null || GUIManager.Instance == null || GUIManager.CustomGUIFront == null)
            {
                return;
            }
            _settler = settler;
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
            _settler = null;
        }

        private static void Rebuild()
        {
            var settler = _settler;
            Close();
            Open(settler);
        }

        private static void Build()
        {
            var equipment = _settler.GetComponent<SettlerEquipment>();
            var player = Player.m_localPlayer;
            var giveable = FindGiveable(player);
            var rows = SettlerEquipment.SlotCount + 1 + Mathf.Max(1, Mathf.Min(giveable.Count, MaxGiveRows));
            var height = 108f + rows * RowHeight + 64f;

            _panel = GUIManager.Instance.CreateWoodpanel(
                GUIManager.CustomGUIFront.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, PanelWidth, height);
            _panel.AddComponent<PanelBehaviour>();

            GUIManager.Instance.CreateText(
                _settler.GetHoverName() + " — " + Localization.instance.Localize("$vs_gear"),
                _panel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -36f),
                GUIManager.Instance.AveriaSerifBold, 24, GUIManager.Instance.ValheimOrange,
                true, Color.black, 540f, 36f, false);

            var y = -76f;
            for (var slot = 0; slot < SettlerEquipment.SlotCount; slot++)
            {
                BuildSlotRow(equipment, slot, y);
                y -= RowHeight;
            }

            var header = GUIManager.Instance.CreateText(
                Localization.instance.Localize("$vs_gear_give") + ":",
                _panel.transform, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(36f + 264f, y - 4f),
                GUIManager.Instance.AveriaSerifBold, 17, GUIManager.Instance.ValheimOrange,
                true, Color.black, 528f, 26f, false);
            header.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
            y -= RowHeight;

            if (giveable.Count == 0)
            {
                var none = GUIManager.Instance.CreateText(
                    Localization.instance.Localize("$vs_gear_nothing"),
                    _panel.transform, new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(36f + 264f, y),
                    GUIManager.Instance.AveriaSerif, 16, UiPalette.SecondaryOnWood,
                    true, Color.black, 528f, 26f, false);
                none.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
            }
            for (var i = 0; i < giveable.Count && i < MaxGiveRows; i++)
            {
                BuildGiveRow(equipment, giveable[i], y);
                y -= RowHeight;
            }

            var closeButton = GUIManager.Instance.CreateButton(
                Localization.instance.Localize("$vs_close"),
                _panel.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 36f), 140f, 38f);
            closeButton.GetComponent<Button>().onClick.AddListener(Close);
        }

        private static void BuildSlotRow(SettlerEquipment equipment, int slot, float y)
        {
            var itemName = equipment != null ? equipment.SlotDisplayName(slot) : null;
            var label = Localization.instance.Localize(SettlerEquipment.SlotTokens[slot])
                + ": " + (itemName ?? Localization.instance.Localize("$vs_gear_none"));
            var text = GUIManager.Instance.CreateText(
                label,
                _panel.transform, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(36f + 200f, y),
                GUIManager.Instance.AveriaSerif, 17,
                itemName != null ? Color.white : UiPalette.SecondaryOnWood,
                true, Color.black, 400f, 28f, false);
            text.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

            if (itemName != null)
            {
                var button = GUIManager.Instance.CreateButton(
                    Localization.instance.Localize("$vs_gear_return"),
                    _panel.transform, new Vector2(1f, 1f), new Vector2(1f, 1f),
                    new Vector2(-90f, y), 120f, 28f);
                button.GetComponent<Button>().onClick.AddListener(() =>
                {
                    equipment.TakeBack(Player.m_localPlayer, slot);
                    Rebuild();
                });
            }
        }

        private static void BuildGiveRow(SettlerEquipment equipment, ItemDrop.ItemData item, float y)
        {
            var name = Localization.instance.Localize(item.m_shared.m_name)
                + (item.m_quality > 1 ? $" ({item.m_quality}★)" : "");
            var text = GUIManager.Instance.CreateText(
                name,
                _panel.transform, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(36f + 200f, y),
                GUIManager.Instance.AveriaSerif, 16, UiPalette.Beige,
                true, Color.black, 400f, 28f, false);
            text.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

            var button = GUIManager.Instance.CreateButton(
                Localization.instance.Localize("$vs_gear_givebtn"),
                _panel.transform, new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-90f, y), 120f, 28f);
            button.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (equipment != null)
                {
                    equipment.Give(Player.m_localPlayer, item);
                }
                Rebuild();
            });
        }

        private static List<ItemDrop.ItemData> FindGiveable(Player player)
        {
            var result = new List<ItemDrop.ItemData>();
            if (player == null)
            {
                return result;
            }
            foreach (var item in player.GetInventory().GetAllItems())
            {
                if (SettlerEquipment.SlotFor(item) >= 0 && item.m_dropPrefab != null)
                {
                    result.Add(item);
                }
            }
            // Best gear first: quality, then armor+damage as a rough power sort.
            result.Sort((a, b) => b.m_quality.CompareTo(a.m_quality));
            return result;
        }

        /// <summary>Closes on Escape or when the settler is gone or far away.</summary>
        private class PanelBehaviour : MonoBehaviour
        {
            private void Update()
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    SettlerGearPanel.Close();
                    return;
                }
                var player = Player.m_localPlayer;
                if (_settler == null || player == null
                    || Vector3.Distance(player.transform.position, _settler.transform.position) > 8f)
                {
                    SettlerGearPanel.Close();
                }
            }
        }
    }
}
