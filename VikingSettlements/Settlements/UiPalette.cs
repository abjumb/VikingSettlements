using UnityEngine;
using UnityEngine.UI;

namespace VikingSettlements.Settlements
{
    /// <summary>
    /// The mod's in-game UI palette and small GUI helpers, from the design
    /// system: titles and button labels Valheim orange (GUIManager), body
    /// white or beige, key hints yellow, warnings #FFA500, secondary text on
    /// wood #9A8465, status green #7FB5A4, badge gold #F5CD82.
    /// </summary>
    internal static class UiPalette
    {
        public static readonly Color Beige = new Color32(0xD9, 0xB9, 0x88, 0xFF);
        public static readonly Color Warning = new Color32(0xFF, 0xA5, 0x00, 0xFF);
        public static readonly Color SecondaryOnWood = new Color32(0x9A, 0x84, 0x65, 0xFF);
        public static readonly Color ColumnHeader = new Color32(0x8A, 0x71, 0x50, 0xFF);
        public static readonly Color WorkingGreen = new Color32(0x7F, 0xB5, 0xA4, 0xFF);
        public static readonly Color BadgeGold = new Color32(0xF5, 0xCD, 0x82, 0xFF);
        public static readonly Color NeedMet = new Color(0.65f, 1f, 0.65f);
        public static readonly Color BadgeWood = new Color32(0x4C, 0x36, 0x1E, 0xFF);
        public static readonly Color WellDark = new Color(0f, 0f, 0f, 0.4f);
        public static readonly Color BarTrack = new Color(0f, 0f, 0f, 0.55f);
        public static readonly Color BarFill = new Color32(0xDC, 0x9A, 0x37, 0xFF);

        /// <summary>A plain solid-color rectangle (badges, wells, bars).</summary>
        public static Image CreateRect(Transform parent, Vector2 anchor, Vector2 position, float width, float height, Color color)
        {
            var go = new GameObject("VS_Rect", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(width, height);
            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }
    }
}
