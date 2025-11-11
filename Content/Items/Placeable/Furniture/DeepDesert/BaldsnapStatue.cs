using ITD.Content.Tiles.Furniture.DeepDesert;

namespace ITD.Content.Items.Placeable.Furniture.DeepDesert;

public class BaldsnapStatue : ModItem
{
    public override void SetDefaults()
    {
        Item.DefaultToFurniture(ModContent.TileType<BaldsnapStatueTile>(), 34, 44);
    }
}
