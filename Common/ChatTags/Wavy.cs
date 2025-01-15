using ITD.Content.UI;
using ITD.Systems.WorldNPCs;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using SteelSeries.GameSense;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace ITD.Common.ChatTags
{
    public class WavyHandler : ITagHandler
    {
        TextSnippet ITagHandler.Parse(string text, Color baseColor, string options)
        {
            string[] paramsStrings = options.Split('/');
            float ampl = 4f;
            float freq = 1f;
            if (!string.IsNullOrEmpty(options))
            {
                float.TryParse(paramsStrings[0], out ampl);
                if (paramsStrings.Length > 1)
                    float.TryParse(paramsStrings[1], out freq);
            }
            return new WavySnippet(text, baseColor, ampl, freq);
        }
    }
    public class WavySnippet : TextSnippet
    {
        public float Amplitude;
        public float Frequency;
        public WavySnippet(string text, Color baseColor, float amplitude, float frequency)
        {
            Text = text;
            Color = baseColor;
            Amplitude = amplitude;
            Frequency = frequency;
        }
        public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1)
        {
            // NOTE FOR FUTURE CHATTAGS:
            // UniqueDraw is ran once for each time a string with said tag is drawn.
            // This means you have to use ChatManager.DrawColorCodedString here, and not ChatManager.DrawColorCodedStringWithShadow,
            // because the latter will draw 5x the amount of shadows if the string is drawn using ChatManager.DrawColorCodedStringWithShadow outside of the TextSnippet.

            if (spriteBatch is null || justCheckingString)
            {
                size = default;
                return false;
            }

            Vector2 outSize = Vector2.Zero;
            DynamicSpriteFont font = FontAssets.MouseText.Value;
            double time = Main.timeForVisualEffects / 32d;
            Vector2 currentPosition = position;

            //Main.NewText(Main.GameUpdateCount);

            for (int i = 0; i < Text.Length; i++)
            {
                char c = Text[i];
                float posOffset = currentPosition.X / 16f;
                float yOffset = MathF.Sin(((float)time + posOffset) * Frequency) * Amplitude;
                string str = c.ToString();
                Vector2 characterSize = font.MeasureString(str);
                Vector2 characterPosition = currentPosition + Vector2.UnitY * yOffset;

                ChatManager.DrawColorCodedString(spriteBatch, font, str, characterPosition, color, 0f, Vector2.Zero, new Vector2(scale));

                currentPosition.X += characterSize.X;
                outSize.X += characterSize.X;
                outSize.Y = Math.Max(outSize.Y, characterSize.Y);
            }

            size = outSize;

            return true;
        }
    }
}
