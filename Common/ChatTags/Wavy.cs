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
        // we need to do something similar to shakysnippet bc else the shadows are all bad
        private float[] yOffsets;
        private uint lastpreComputeGUC;
        public WavySnippet(string text, Color baseColor, float amplitude, float frequency)
        {
            Text = text;
            Color = baseColor;
            Amplitude = amplitude;
            Frequency = frequency;
            yOffsets = new float[Text.Length];
            lastpreComputeGUC = 0;
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

            DynamicSpriteFont font = FontAssets.MouseText.Value;

            // to have consistent yOffsets for every character, even when drawn with shadows, we can pre-compute new yOffsets for each character every frame.
            if (lastpreComputeGUC != Main.GameUpdateCount)
            {
                Vector2 tempPos = Vector2.Zero;
                double time = Main.timeForVisualEffects / 32d;

                for (int i = 0; i < Text.Length; i++)
                {
                    char c = Text[i];
                    string str = c.ToString();
                    Vector2 charSize = font.MeasureString(str);

                    tempPos.X += charSize.X;

                    if (char.IsWhiteSpace(c))
                    {
                        yOffsets[i] = 0;
                        continue;
                    }

                    float posOffset = tempPos.X / 16f;
                    yOffsets[i] = MathF.Sin(((float)time + posOffset) * Frequency) * Amplitude;
                }
                lastpreComputeGUC = Main.GameUpdateCount;
            }

            Vector2 outSize = Vector2.Zero;
            Vector2 currentPosition = position;

            //Main.NewText(Main.GameUpdateCount);

            for (int i = 0; i < Text.Length; i++)
            {
                char c = Text[i];
                string str = c.ToString();
                Vector2 characterSize = font.MeasureString(str);

                currentPosition.X += characterSize.X;
                outSize.X += characterSize.X;
                outSize.Y = Math.Max(outSize.Y, characterSize.Y);

                // avoid calculating stuff if this char is whitespace
                // i love microoptimizations
                if (char.IsWhiteSpace(c))
                    continue;

                Vector2 characterPosition = currentPosition + Vector2.UnitY * yOffsets[i];

                ChatManager.DrawColorCodedString(spriteBatch, font, str, characterPosition, color, 0f, Vector2.Zero, new Vector2(scale));

            }

            size = outSize;

            return true;
        }
    }
}
