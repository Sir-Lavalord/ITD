﻿using Terraria.GameContent.ItemDropRules;
using ITD.Content.Items.Weapons.Ranger;
using ITD.Content.Items.Accessories.Defensive.Defense;
using ITD.Content.Items.Accessories.Expert;

namespace ITD.Content.Items.Other
{
    public class SandberusBag : ModItem
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
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Skullmet>(), 1));
			itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Dunebarrel>(), 5));
            itemLoot.Add(ItemDropRule.Common(ItemID.SandBlock, 1, 20, 30));
			itemLoot.Add(ItemDropRule.Common(ItemID.DesertFossil, 1, 5, 15));
            itemLoot.Add(ItemDropRule.Common(ItemID.FossilOre, 1, 5, 8));
			itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<WindCape>(), 1));
        }
    }
}