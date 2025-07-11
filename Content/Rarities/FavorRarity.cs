namespace ITD.Content.Rarities
{
    using Microsoft.Xna.Framework;
    using System;
    using Terraria;
    using Terraria.ModLoader;

    public class FavorRarity : ModRarity
    {
        public override Color RarityColor
        {
            get
            {
                float t = (float)Main.GameUpdateCount / 120f;
                t = (float)(0.5f * (Math.Sin(t * MathHelper.TwoPi) + 1));

                Color startColor = new Color(255, 255, 0); // Yellow
                Color endColor = new Color(0, 255, 255); // Blue

                return Color.Lerp(startColor, endColor, t);
            }
        }
    }
}
