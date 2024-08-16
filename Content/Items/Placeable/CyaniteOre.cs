using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using ITD.Content.Tiles.BlueshroomGroves;

namespace ITD.Content.Items.Placeable
{
    public class CyaniteOre : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<CyaniteOreTile>());
            Item.width = 14;
            Item.height = 18;
			Item.rare = ItemRarityID.Cyan;
        }
    }
}