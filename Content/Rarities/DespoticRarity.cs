using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
	
namespace ITD.Content.Rarities
{
    public class DespoticRarity : ModRarity
    {
        public override Color RarityColor => Color.Black;

        public static void DrawCustomTooltipLine(DrawableTooltipLine tooltipLine)
        {
            // Draw the base tooltip text and glow.
            Color blue = new(51, 191, 255);
            RarityHelper.DrawBaseTooltipTextAndGlow(tooltipLine, glowColor: blue, textOuterColor: blue, Color.Black, glowScaleOffset: new(1.2f, 1f));
        }
    }
}
