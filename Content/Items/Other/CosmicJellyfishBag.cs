using ITD.Content.NPCs;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Items
{
    public class CosmicJellyfishBag : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.BossBag[Type] = true;
            ItemID.Sets.PreHardmodeLikeBossBag[Type] = true;
            Item.ResearchUnlockCount = 3;
        }

        public override void SetDefaults()
        {
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Purple;
            Item.expert = true;
        }

        public override bool CanRightClick()
        {
            return true;
        }

        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            //itemLoot.Add(ItemDropRule.NotScalingWithLuck(ModContent.ItemType<CosmicJellyfishMask>(), 7));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<VoidShard>(), 1, 7, 15));
            itemLoot.Add(ItemDropRule.CoinsBasedOnNPCValue(ModContent.NPCType<CosmicJellyfish>()));
        }
    }
}