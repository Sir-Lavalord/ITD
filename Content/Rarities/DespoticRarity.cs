using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using ITD.Common.Rarities;

namespace ITD.Content.Rarities
{
	public abstract class DespoticRarityHack : ModRarity // idk to be honest
    {
        [Autoload(false)]
        private class DespoticRarityModifier : RarityModifier
        {
            public DespoticRarityHack ModRarity { get; init; }
            public override int RarityType { get => ModRarity.Type; }

            public DespoticRarityModifier(DespoticRarityHack modRarity)
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
            var modifier = new DespoticRarityModifier(this);

            Mod.AddContent(modifier);

            Load(modifier);
        }

        public virtual void Load(RarityModifier modifier) { }
        public abstract void Draw(RarityModifier.DrawData data);
    }
	
    public class DespoticRarity : DespoticRarityHack
    {
        public override Color RarityColor => Color.Black;

        public override void Draw(RarityModifier.DrawData data)
        {
            // Draw the base tooltip text and glow.
            Color blue = new(51, 191, 255);
            RarityHelper.DrawBaseTooltipTextAndGlow(data, glowColor: blue, textOuterColor: blue, Color.Black, glowScaleOffset: new(1.2f, 1f));
        }
    }
}
