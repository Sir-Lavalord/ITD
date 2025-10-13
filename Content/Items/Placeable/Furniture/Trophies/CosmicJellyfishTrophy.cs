using ITD.Content.Tiles.Furniture;

namespace ITD.Content.Items.Placeable.Furniture.Trophies;

public class CosmicJellyfishTrophy : ModItem
{
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<CosmicJellyfishTrophyTile>());

        Item.width = 32;
        Item.height = 32;
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.buyPrice(0, 1);
    }
}