using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using Terraria;
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
        public ShakySnippet(string text, Color baseColor, float strength)
        {
            Text = text;
            Color = baseColor;
            Strength = strength;
        }
        public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1)
        {
            if (spriteBatch is null || justCheckingString)
            {
                size = default;
                return false;
            }

            Vector2 outSize = Vector2.Zero;
            DynamicSpriteFont font = FontAssets.MouseText.Value;
            Vector2 currentPosition = position;

            for (int i = 0; i < Text.Length; i++)
            {
                char c = Text[i];
                Vector2 shake = Main.rand.NextVector2Circular(Strength, Strength);
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
