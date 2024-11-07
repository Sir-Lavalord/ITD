using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace ITD.Content.Rarities
{
    public static class RarityHelper // it came from the infernum
    {
		Texture2D baseRarityGlow = ModContent.Request<Texture2D>("ITD/Content/Rarities/Textures/BaseRarityGlow").Value;
		
        public static void DrawBaseTooltipTextAndGlow(DrawableTooltipLine tooltipLine, Color glowColor, Color textOuterColor, Color? textInnerColor = null, Texture2D glowTexture = null, Vector2? glowScaleOffset = null)
        {
            textInnerColor ??= Color.Black;
            glowTexture ??= baseRarityGlow;
            glowScaleOffset ??= Vector2.One;
            // Get the text of the tooltip line.
            string text = tooltipLine.Text;
            // Get the size of the text in its font.
            Vector2 textSize = tooltipLine.Font.MeasureString(text);
            // Get the center of the text.
            Vector2 textCenter = textSize * 0.5f;
            // The position to draw the text.
            Vector2 textPosition = new(tooltipLine.X, tooltipLine.Y);
            // Get the position to draw the glow behind the text.
            Vector2 glowPosition = new(tooltipLine.X + textCenter.X, tooltipLine.Y + textCenter.Y / 1.5f);
            // Get the scale of the glow texture based off of the text size.
            Vector2 glowScale = new Vector2(textSize.X * 0.115f, 0.6f) * glowScaleOffset.Value;
            glowColor.A = 0;
            // Draw the glow texture.
            Main.spriteBatch.Draw(glowTexture, glowPosition, null, glowColor * 0.85f, 0f, glowTexture.Size() * 0.5f, glowScale, SpriteEffects.None, 0f);

            // Get an offset to the afterimageOffset based on a sine wave.
            float sine = (float)((1 + Math.Sin(Main.GlobalTimeWrappedHourly * 2.5f)) / 2);
            float sineOffset = MathHelper.Lerp(0.5f, 1f, sine);

            // Draw text backglow effects.
            for (int i = 0; i < 12; i++)
            {
                Vector2 afterimageOffset = (MathHelper.TwoPi * i / 12f).ToRotationVector2() * (2f * sineOffset);
                // Draw the text. Rotate the position based on i.
                ChatManager.DrawColorCodedString(Main.spriteBatch, tooltipLine.Font, text, (textPosition + afterimageOffset).RotatedBy(MathHelper.TwoPi * (i / 12)), textOuterColor * 0.9f, tooltipLine.Rotation, tooltipLine.Origin, tooltipLine.BaseScale);
            }

            // Draw the main inner text.
            Color mainTextColor = Color.Lerp(glowColor, textInnerColor.Value, 0.9f);
            ChatManager.DrawColorCodedString(Main.spriteBatch, tooltipLine.Font, text, textPosition, mainTextColor, tooltipLine.Rotation, tooltipLine.Origin, tooltipLine.BaseScale);
        }
	}
}
