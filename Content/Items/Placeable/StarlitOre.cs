using Terraria.DataStructures;
using ITD.Content.Tiles;

namespace ITD.Content.Items.Placeable
{
    public class StarlitOre : ModItem
    {
        public override void SetStaticDefaults()
        { 
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 5));
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
			Item.DefaultToPlaceableTile(ModContent.TileType<StarlitOreTile>());
            Item.width = 12;
            Item.height = 12;
			Item.rare = ItemRarityID.Blue;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(silver: 2);
        }
    }
}