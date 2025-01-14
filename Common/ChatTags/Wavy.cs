using Microsoft.Xna.Framework.Graphics;
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
            Vector2 outSize = Vector2.Zero;
            DynamicSpriteFont font = FontAssets.MouseText.Value;
            float time = Main.GameUpdateCount / 32f;
            Vector2 currentPosition = position;

            for (int i = 0; i < Text.Length; i++)
            {
                char c = Text[i];
                float posOffset = currentPosition.X / 16f;
                float yOffset = MathF.Sin((time + posOffset) * Frequency) * Amplitude;
                string str = c.ToString();
                Vector2 characterSize = font.MeasureString(str);

                if (spriteBatch != null)
                {
                    ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, str, currentPosition + Vector2.UnitY * yOffset, color, 0f, Vector2.Zero, Vector2.One, spread: 0);
                }

                currentPosition.X += characterSize.X;
                outSize.X += characterSize.X;
                outSize.Y = Math.Max(outSize.Y, characterSize.Y);
            }

            size = outSize;
            return true;
        }
    }
}
