using ITD.Content.Dusts;
using static Terraria.ModLoader.ModContent;

namespace ITD.Content.Tiles.BlueshroomGroves;

public class BlueshroomSapling : ITDSapling
{
    public override void SetStaticSaplingDefaults()
    {
        DustType = DustType<BlueshroomSporesDust>();
        MapColor = Color.Aquamarine;
        GrowSlow = 20;
        GrowsIntoTreeType = TileType<BlueshroomTree>();
        MinGrowHeight = 8;
        MaxGrowHeight = 14;
    }
}