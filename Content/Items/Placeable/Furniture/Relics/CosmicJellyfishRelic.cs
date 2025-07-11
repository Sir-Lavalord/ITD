using ITD.Content.Tiles.Furniture.Relics;

namespace ITD.Content.Items.Placeable.Furniture.Relics
{
    public class CosmicJellyfishRelic : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<CosmicJellyfishRelicTile>(), 0);

            Item.width = 30;
            Item.height = 40;
            Item.rare = ItemRarityID.Master;
            Item.master = true;
            Item.value = Item.buyPrice(0, 5);
        }
    }
}