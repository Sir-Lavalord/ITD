using ITD.Content.Tiles.Furniture.Catacombs;
using Terraria.ModLoader;

namespace ITD.Content.Items.Placeable.Furniture.Catacombs
{
    public class BlueCatacombDoor : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.GetInstance<CatacombDoor>().ClosedType);
            Item.width = 14;
            Item.height = 28;
        }
    }
    public class PinkCatacombDoor : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.GetInstance<CatacombDoor>().ClosedType, 1);
            Item.width = 14;
            Item.height = 28;
        }
    }
    public class GreenCatacombDoor : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.GetInstance<CatacombDoor>().ClosedType, 2);
            Item.width = 14;
            Item.height = 28;
        }
    }
}
