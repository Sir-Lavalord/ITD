using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ITD.Content.Tiles;

namespace ITD.Content.Items
{
    public class Bluesoil : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<BluesoilTile>());
            Item.width = 12;
            Item.height = 12;
        }
    }
}