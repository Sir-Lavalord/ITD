using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using ITD.Common.Rarities;

namespace ITD.Content.Rarities
{
	[Autoload(false)]
    public class DespoticRarityModifier : RarityModifier
    {
        public DespoticRarity ModRarity { get; init; }
        public override int RarityType { get => ModRarity.Type; }

        public DespoticRarityModifier(DespoticRarity modRarity)
        {
            ModRarity = modRarity;
        }

        public override void Draw(DrawData data)
        {
            Color blue = new(51, 191, 255);
            RarityHelper.DrawBaseTooltipTextAndGlow(data, glowColor: blue, textOuterColor: blue, Color.Black, glowScaleOffset: new(1.2f, 1f));
        }
    }
	
    public class DespoticRarity : ModRarity
    {
        public override Color RarityColor => Color.Black;

		public sealed override void Load()
        {
            var modifier = new DespoticRarityModifier(this);
            Mod.AddContent(modifier);
        }
    }
}
