using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using VikingSettlements.Party;
using VikingSettlements.Settlements;

namespace VikingSettlements.Npcs
{
    /// <summary>
    /// The "talk to a settler" panel: opened with the talk hotkey while
    /// looking at (or standing next to) a settler. Shows who they are, their
    /// health and hunger, and - for assigned settlers - each thing their job
    /// needs before they will work, evaluated with the exact checks the work
    /// loop gates on, so the panel never disagrees with their behavior.
    /// </summary>
    internal static class SettlerTalkPanel
    {
        private const float PanelWidth = 480f;
        private const float LineHeight = 27f;
        private const float TargetRange = 5f;

        private static readonly string[] Greetings =
        {
            "$vs_talk_g1", "$vs_talk_g2", "$vs_talk_g3", "$vs_talk_g4",
        };

        private static GameObject _panel;
        private static SettlerRecruitable _settler;

        public static void OnUpdate()
        {
            var player = Player.m_localPlayer;
            if (player == null)
            {
                Close();
                return;
            }
            if (!ModConfig.TalkHotkey.Value.IsDown())
            {
                return;
            }
            if (_panel != null)
            {
                Close();
                return;
            }
            if (PartySystem.UiHasFocus() || SettlementPanel.IsOpen)
            {
                return;
            }
            var settler = FindTarget(player);
            if (settler != null)
            {
                Open(settler);
            }
        }

        private static SettlerRecruitable FindTarget(Player player)
        {
            var hovering = player.m_hoveringCreature;
            if (hovering != null)
            {
                var hovered = hovering.GetComponent<SettlerRecruitable>();
                if (hovered != null)
                {
                    return hovered;
                }
            }
            SettlerRecruitable best = null;
            var bestDistance = TargetRange;
            foreach (var settler in SettlerRecruitable.Instances)
            {
                var character = settler.GetComponent<Character>();
                if (character == null || character.IsDead())
                {
                    continue;
                }
                var distance = Vector3.Distance(player.transform.position, settler.transform.position);
                if (distance < bestDistance)
                {
                    best = settler;
                    bestDistance = distance;
                }
            }
            return best;
        }

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

        private static void Build()
        {
            var character = _settler.GetComponent<Character>();
            var name = _settler.GetHoverName();
            var lines = ComposeLines(character);

            var height = 118f + lines.Count * LineHeight + 64f;
            _panel = GUIManager.Instance.CreateWoodpanel(
                GUIManager.CustomGUIFront.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, PanelWidth, height);
            _panel.AddComponent<PanelBehaviour>();

            GUIManager.Instance.CreateText(
                name,
                _panel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -36f),
                GUIManager.Instance.AveriaSerifBold, 24, GUIManager.Instance.ValheimOrange,
                true, Color.black, 420f, 36f, false);

            var greeting = Greetings[(int)((uint)name.GetHashCode() % (uint)Greetings.Length)];
            GUIManager.Instance.CreateText(
                "“" + Localization.instance.Localize(greeting) + "”",
                _panel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -68f),
                GUIManager.Instance.AveriaSerif, 16, Color.gray,
                true, Color.black, 420f, 26f, false);

            for (var i = 0; i < lines.Count; i++)
            {
                GUIManager.Instance.CreateText(
                    lines[i].Text,
                    _panel.transform, new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(36f, -(110f + i * LineHeight)),
                    GUIManager.Instance.AveriaSerif, 17, lines[i].Color,
                    true, Color.black, PanelWidth - 72f, LineHeight - 2f, false);
            }

            var closeButton = GUIManager.Instance.CreateButton(
                Localization.instance.Localize("$vs_close"),
                _panel.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 36f), 140f, 38f);
            closeButton.GetComponent<Button>().onClick.AddListener(Close);
        }

        private struct PanelLine
        {
            public string Text;
            public Color Color;
        }

        private static List<PanelLine> ComposeLines(Character character)
        {
            var lines = new List<PanelLine>();
            var ok = new Color(0.65f, 1f, 0.65f);
            var bad = new Color(1f, 0.65f, 0.4f);

            // Who they are right now.
            string role;
            switch (_settler.State)
            {
                case SettlerState.Wild:
                    role = "$vs_talk_wild";
                    var heart = VillageHeart.FindNearest(_settler.transform.position);
                    if (heart != null && ModConfig.ReputationEnabled.Value)
                    {
                        role += $" — $vs_rep: {VillageHeart.TierToken(heart.Reputation)}";
                    }
                    break;
                case SettlerState.Following:
                    var member = _settler.GetComponent<PartyMember>();
                    var stance = member != null ? member.Stance : PartyStance.Follow;
                    role = $"$vs_talk_party — {PartySystem.StanceToken(stance)}";
                    break;
                default:
                    role = SettlerRecruitable.JobToken(_settler.Job);
                    break;
            }
            lines.Add(Line(role, Color.white));

            if (character != null)
            {
                var percent = Mathf.RoundToInt(character.GetHealthPercentage() * 100f);
                lines.Add(Line($"$vs_talk_health: {percent}%", percent < 50 ? bad : Color.white));
            }

            // Hunger, for settlers that are somebody's dependent.
            if (_settler.State == SettlerState.Assigned && ModConfig.FoodUpkeep.Value)
            {
                if (_settler.IsHungry)
                {
                    lines.Add(Line("$vs_talk_hungry", bad));
                }
                else
                {
                    var minutes = SettlerNeeds.MinutesToNextMeal(_settler);
                    lines.Add(Line(minutes >= 0
                        ? $"$vs_talk_fed ($vs_talk_nextmeal {minutes} min)"
                        : "$vs_talk_fed", ok));
                }
            }

            // What the job needs, live.
            var needs = SettlerNeeds.Evaluate(_settler);
            if (needs.Count > 0)
            {
                lines.Add(Line("$vs_talk_needs:", GUIManager.Instance.ValheimOrange));
                foreach (var need in needs)
                {
                    lines.Add(need.Met
                        ? Line($"  ✓ {need.Token}", ok)
                        : Line($"  ✗ {need.Token}", bad));
                }
            }

            switch (_settler.State)
            {
                case SettlerState.Following:
                    lines.Add(Line("$vs_talk_party_hint", Color.gray));
                    break;
                case SettlerState.Assigned when _settler.Job == SettlerJob.Villager:
                    lines.Add(Line("$vs_talk_villager_none", Color.gray));
                    break;
                case SettlerState.Assigned when _settler.Job == SettlerJob.Guard:
                    lines.Add(Line("$vs_talk_guard_none", Color.gray));
                    break;
            }
            return lines;
        }

        private static PanelLine Line(string text, Color color)
        {
            return new PanelLine
            {
                Text = Localization.instance.Localize(text),
                Color = color,
            };
        }

        /// <summary>Closes on Escape or when the settler is gone or far away.</summary>
        private class PanelBehaviour : MonoBehaviour
        {
            private void Update()
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    SettlerTalkPanel.Close();
                    return;
                }
                var player = Player.m_localPlayer;
                if (_settler == null || player == null
                    || Vector3.Distance(player.transform.position, _settler.transform.position) > 8f)
                {
                    SettlerTalkPanel.Close();
                }
            }
        }
    }
}
