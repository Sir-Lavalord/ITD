using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace ITD.Content.Items
{
    public class VoidShard : ModItem
    {
        public override void SetStaticDefaults()
        { 
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 24;

            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.buyPrice(silver: 1);
        }
    }
}