using ITD.Content.NPCs.Bosses;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using ITD.Content.Items.Weapons.Ranger;
using ITD.Content.Items.Accessories.Defensive.Defense;
using ITD.Content.Items.Accessories.Expert;

namespace ITD.Content.Items.Other
{
    public class HardmodeGravekeeperBag : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.BossBag[Type] = true;
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
			itemLoot.Add(ItemDropRule.Common(ItemID.Ectoplasm, 1, 5, 10));
        }
    }
}