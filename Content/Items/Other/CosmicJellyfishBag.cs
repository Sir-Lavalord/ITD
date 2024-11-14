using ITD.Content.NPCs.Bosses;
using ITD.Content.Items.Materials;
using ITD.Content.Items.Placeable;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using ITD.Content.Items.Armor.Vanity.Masks;
using ITD.Content.Items.Accessories.Expert;
using ITD.Content.Items.Accessories.Movement.Boots;

namespace ITD.Content.Items.Other
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
            itemLoot.Add(ItemDropRule.NotScalingWithLuck(ModContent.ItemType<GravityBoots>(), 10));
            itemLoot.Add(ItemDropRule.NotScalingWithLuck(ModContent.ItemType<CosmicJellyfishMask>(), 7));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<GalacticJellyBean>(),1));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<StarlitOre>(), 1, 25, 45));
            itemLoot.Add(ItemDropRule.Common(ItemID.Star, 1, 10, 20));
            itemLoot.Add(ItemDropRule.CoinsBasedOnNPCValue(ModContent.NPCType<CosmicJellyfish>()));
        }
    }
}