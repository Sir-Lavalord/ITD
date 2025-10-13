using ITD.Common.Rarities;
using ITD.Systems.DataStructures;
using ITD.Systems.Extensions;
using System;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace ITD.Content.Rarities;

public static class RarityHelper // it came from the infernum
{
    public static void DrawBaseTooltipTextAndGlow(RarityModifier.DrawData data, Color glowColor, Color textOuterColor, Color? textInnerColor = null, Texture2D glowTexture = null, Vector2? glowScaleOffset = null)
    {
        Main.spriteBatch.End(out SpriteBatchData spriteBatchData); // restart spritebatch
        SamplerState oldSampler = spriteBatchData.SamplerState;
        spriteBatchData.SamplerState = null;
        Main.spriteBatch.Begin(spriteBatchData);

        textInnerColor ??= Color.Black;
        glowTexture ??= ITD.Instance.Assets.Request<Texture2D>("Content/Rarities/Textures/BaseRarityGlow").Value;
        glowScaleOffset ??= Vector2.One;

        string text = data.Text;
        Vector2 textSize = FontAssets.MouseText.Value.MeasureString(text);
        Vector2 textCenter = textSize * 0.5f;
        Vector2 textPosition = new(data.Position.X, data.Position.Y);
        Vector2 glowPosition = new Vector2(data.Position.X + textCenter.X, data.Position.Y + textCenter.Y / 1.5f) - data.Origin;
        Vector2 glowScale = new Vector2(textSize.X * 0.115f, 0.6f) * glowScaleOffset.Value * data.Scale;
        glowColor.A = 0;
        Main.spriteBatch.Draw(glowTexture, glowPosition, null, glowColor * 0.85f, 0f, glowTexture.Size() * 0.5f, glowScale, SpriteEffects.None, 0f);

        // Get an offset to the afterimageOffset based on a sine wave.
        float sine = (float)((1 + Math.Sin(Main.GlobalTimeWrappedHourly * 2.5f)) / 2);
        float sineOffset = MathHelper.Lerp(0.5f, 1f, sine);

        // Draw text backglow effects.
        for (int i = 0; i < 12; i++)
        {
            Vector2 afterimageOffset = (MathHelper.TwoPi * i / 12f).ToRotationVector2() * (2f * sineOffset);
            // Draw the text. Rotate the position based on i.
            ChatManager.DrawColorCodedString(Main.spriteBatch, FontAssets.MouseText.Value, text, (textPosition + afterimageOffset).RotatedBy(MathHelper.TwoPi * (i / 12)), textOuterColor * 0.9f, data.Rotation, data.Origin, data.Scale);
        }

        // Draw the main inner text.
        Color mainTextColor = Color.Lerp(glowColor, textInnerColor.Value, 0.9f);
        ChatManager.DrawColorCodedString(Main.spriteBatch, FontAssets.MouseText.Value, text, textPosition, mainTextColor, data.Rotation, data.Origin, data.Scale, data.MaxWidth);

        Main.spriteBatch.End();
        spriteBatchData.SamplerState = oldSampler;
        Main.spriteBatch.Begin(spriteBatchData);
    }
}
