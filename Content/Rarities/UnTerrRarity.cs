using ITD.Common.Rarities;

namespace ITD.Content.Rarities
{
	[Autoload(false)]
    public class UnTerrRarityModifier : RarityModifier
    {
        public UnTerrRarity ModRarity { get; init; }
        public override int RarityType { get => ModRarity.Type; }

        public UnTerrRarityModifier(UnTerrRarity modRarity)
        {
            ModRarity = modRarity;
        }

        public override void Draw(DrawData data)
        {
			Color yellow = new(228, 202, 12);
            Color blue = new(83, 75, 164);
			Color darkBlue = new(17, 13, 53);
            RarityHelper.DrawBaseTooltipTextAndGlow(data, glowColor: blue, textOuterColor: darkBlue, yellow, glowScaleOffset: new(1.2f, 1f));
        }
    }
	
    public class UnTerrRarity : ModRarity
    {
        public override Color RarityColor => Color.Black;

		public sealed override void Load()
        {
            var modifier = new UnTerrRarityModifier(this);
            Mod.AddContent(modifier);
        }
    }
}
