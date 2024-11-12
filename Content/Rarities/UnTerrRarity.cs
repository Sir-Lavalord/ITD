using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using ITD.Common.Rarities;

namespace ITD.Content.Rarities
{
	public abstract class UnTerrRarityHack : ModRarity // idk to be honest
    {
        [Autoload(false)]
        private class UnTerrRarityModifier : RarityModifier
        {
            public UnTerrRarityHack ModRarity { get; init; }
            public override int RarityType { get => ModRarity.Type; }

            public UnTerrRarityModifier(UnTerrRarityHack modRarity)
            {
                ModRarity = modRarity;
            }

            public override void Draw(DrawData data)
            {
                ModRarity.Draw(data);
            }
        }

        public sealed override void Load()
        {
            var modifier = new UnTerrRarityModifier(this);

            Mod.AddContent(modifier);

            Load(modifier);
        }

        public virtual void Load(RarityModifier modifier) { }
        public abstract void Draw(RarityModifier.DrawData data);
    }
	
    public class UnTerrRarity : UnTerrRarityHack
    {
        public override Color RarityColor => Color.Black;

        public override void Draw(RarityModifier.DrawData data)
        {
            // Draw the base tooltip text and glow.
			Color yellow = new(228, 202, 12);
            Color blue = new(83, 75, 164);
			Color darkBlue = new(17, 13, 53);
            RarityHelper.DrawBaseTooltipTextAndGlow(data, glowColor: blue, textOuterColor: darkBlue, yellow, glowScaleOffset: new(1.2f, 1f));
        }
    }
}
