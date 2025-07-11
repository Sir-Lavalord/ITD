namespace ITD.Content.Rarities
{
    using Microsoft.Xna.Framework;
    using System;
    using Terraria;
    using Terraria.ModLoader;

    public class CursedFavorRarity : ModRarity
    {
        public override Color RarityColor
        {
            get
            {
                float t = (float)Main.GameUpdateCount / 120f;
                t = (float)(0.5f * (Math.Sin(t * MathHelper.TwoPi) + 1));

                Color startColor = new Color(128, 0, 0); // Maroon
                Color endColor = new Color(128, 0, 128); // Blue

                return Color.Lerp(startColor, endColor, t);
            }
        }
    }
}
