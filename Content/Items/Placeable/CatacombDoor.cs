using ITD.Content.Tiles.Furniture.Catacombs;
using Terraria.ModLoader;

namespace ITD.Content.Items.Placeable
{
    public class BlueCatacombDoor : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<CatacombDoorClosed>());
            Item.width = 14;
            Item.height = 28;
        }
    }
	public class PinkCatacombDoor : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<CatacombDoorClosed>(), 1);
            Item.width = 14;
            Item.height = 28;
        }
    }
    public class GreenCatacombDoor : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<CatacombDoorClosed>(), 2);
            Item.width = 14;
            Item.height = 28;
        }
    }
}
