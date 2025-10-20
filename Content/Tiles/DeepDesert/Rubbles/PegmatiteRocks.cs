using ITD.Content.Items.Placeable.Biomes.DeepDesert;
using ITD.Systems.DataStructures;
using Terraria.GameContent;

namespace ITD.Content.Tiles.DeepDesert.Rubbles;

public class PegmatiteRubble3x2 : ITDRubble
{
    public override Point8 Dimensions => new(3, 2);
    public override void SetStaticDefaultsSafe()
    {
        FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<Pegmatite>(), Type, 0, 1);
    }
}
public class PegmatiteRubble4x2 : ITDRubble
{
    public override Point8 Dimensions => new(4, 2);
    public override void SetStaticDefaultsSafe()
    {
        FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<Pegmatite>(), Type, 0);
    }
}
