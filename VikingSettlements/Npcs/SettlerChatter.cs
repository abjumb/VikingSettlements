using UnityEngine;

namespace VikingSettlements.Npcs
{
    /// <summary>
    /// Cosmetic, client-side idle chatter: settlers occasionally greet
    /// players who come close.
    /// </summary>
    public class SettlerChatter : MonoBehaviour
    {
        private const float TalkRange = 10f;

        private static readonly string[] Lines =
        {
            "Welcome, traveller.",
            "Odin watch over you.",
            "The forest has been restless lately...",
            "Fine weather for a raid, eh?",
            "We built all this with our own hands.",
            "Stay the night, the wolves howl after dark.",
            "Have you seen a greydwarf? Ugly things.",
            "May your mead never run dry.",
            "The soil here is good. We will stay.",
            "Skål!",
            "Keep your axe close, friend.",
            "I hear great beasts stir beyond the mist.",
        };

        private float _timer;

        private void Update()
        {
            if (ModConfig.ChatterEnabled == null || !ModConfig.ChatterEnabled.Value)
            {
                return;
            }

            _timer += Time.deltaTime;
            if (_timer < ModConfig.ChatterInterval.Value)
            {
                return;
            }

            var player = Player.m_localPlayer;
            if (player == null || Chat.instance == null)
            {
                return;
            }
            if (Vector3.Distance(player.transform.position, transform.position) > TalkRange)
            {
                return;
            }

            var character = GetComponent<Character>();
            if (character == null || character.IsDead())
            {
                return;
            }

            _timer = 0f;
            if (Random.value < 0.4f)
            {
                // Not every opportunity is taken, so villages don't feel scripted.
                return;
            }

            var line = Lines[Random.Range(0, Lines.Length)];
            Chat.instance.SetNpcText(gameObject, Vector3.up * 2f, 20f, 8f, "", line, false);
        }
    }
}
