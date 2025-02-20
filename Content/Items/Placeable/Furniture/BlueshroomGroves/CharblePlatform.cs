using ITD.Content.Tiles.Furniture.BlueshroomGroves;

namespace ITD.Content.Items.Placeable.Furniture.BlueshroomGroves
{
    public class CharblePlatform : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<CharblePlatformTile>());
        }
    }
}
