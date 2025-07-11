using ReLogic.Graphics;
using System;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace ITD.Common.ChatTags
{
    public class ShakyHandler : ITagHandler
    {
        TextSnippet ITagHandler.Parse(string text, Color baseColor, string options)
        {
            float strength = 1.5f;
            if (!string.IsNullOrEmpty(options))
                float.TryParse(options, out strength);
            return new ShakySnippet(text, baseColor, strength);
        }
    }
    public class ShakySnippet : TextSnippet
    {
        public float Strength;
        private Vector2[] shakes;
        private uint lastShakeGUC;
        public ShakySnippet(string text, Color baseColor, float strength)
        {
            Text = text;
            Color = baseColor;
            Strength = strength;
            shakes = new Vector2[Text.Length];
            lastShakeGUC = 0;
        }
        public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1)
        {
            if (spriteBatch is null || justCheckingString)
            {
                size = default;
                return false;
            }

            // to have consistent shakes for every character, even when drawn with shadows, we can pre-compute new shakes for each character every frame.
            if (lastShakeGUC != Main.GameUpdateCount)
            {
                for (int i = 0; i < Text.Length; i++)
                {
                    shakes[i] = Main.rand.NextVector2Circular(Strength, Strength);
                }
                lastShakeGUC = Main.GameUpdateCount;
            }

            Vector2 outSize = Vector2.Zero;
            DynamicSpriteFont font = FontAssets.MouseText.Value;
            Vector2 currentPosition = position;

            for (int i = 0; i < Text.Length; i++)
            {
                char c = Text[i];
                Vector2 shake = shakes[i];
                string str = c.ToString();
                Vector2 characterSize = font.MeasureString(str);
                Vector2 characterPosition = currentPosition + shake;

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
