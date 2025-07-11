using ITD.Content.NPCs.Bosses;
using Terraria.GameContent.ItemDropRules;
using ITD.Content.Items.Accessories.Expert;

namespace ITD.Content.Items.Other
{
    public class GravekeeperBag : ModItem
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
            Item.expert = true;
        }

        public override bool CanRightClick()
        {
            return true;
        }

        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            itemLoot.Add(ItemDropRule.OneFromOptionsNotScalingWithLuck(1, Gravekeeper.oneFromOptionsDrops));
			itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<CharmOfTheAccursed>(), 1));
        }
    }
}