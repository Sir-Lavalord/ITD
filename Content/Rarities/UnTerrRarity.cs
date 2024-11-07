using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
	
namespace ITD.Content.Rarities
{
    public class UnTerrRarity : ModRarity
    {
        public override Color RarityColor => new Color(83, 75, 164);

        public static void DrawCustomTooltipLine(DrawableTooltipLine tooltipLine)
        {
            // Draw the base tooltip text and glow.
			Color yellow = new(228, 202, 12);
            Color blue = new(83, 75, 164);
			Color darkBlue = new(17, 13, 53);
            RarityHelper.DrawBaseTooltipTextAndGlow(tooltipLine, glowColor: blue, textOuterColor: darkBlue, yellow, glowScaleOffset: new(1.2f, 1f));
        }
    }
}
