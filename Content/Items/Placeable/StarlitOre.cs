using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
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
            Item.width = 16;
            Item.height = 16;

            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.buyPrice(silver: 1);
        }
    }
}