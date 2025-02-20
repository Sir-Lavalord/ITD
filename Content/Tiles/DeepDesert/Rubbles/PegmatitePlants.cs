using Terraria.GameContent;
using ITD.Systems.DataStructures;
using ITD.Content.Items.Placeable.Biomes.DeepDesert;

namespace ITD.Content.Tiles.DeepDesert.Rubbles
{
    public class PegmatitePlant4x4 : ITDRubble
    {
        public override Point8 Dimensions => new(4);
        public override void SetStaticDefaultsSafe()
        {
            FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<LightPyracotta>(), Type, 0);
        }
    }
    public class PegmatitePlant3x2 : ITDRubble
    {
        public override Point8 Dimensions => new(3, 2);
        public override void SetStaticDefaultsSafe()
        {
            FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<LightPyracotta>(), Type, 0);
        }
    }
    public class PegmatitePlant2x2 : ITDRubble
    {
        public override Point8 Dimensions => new(2);
        public override void SetStaticDefaultsSafe()
        {
            FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<LightPyracotta>(), Type, 0);
        }
    }
}
