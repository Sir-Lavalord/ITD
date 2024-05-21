using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ITD.Content.Tiles;

namespace ITD.Content.Items
{
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
}