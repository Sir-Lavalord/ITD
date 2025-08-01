﻿using ITD.Content.Items.Accessories.Movement.Dashes;
using ITD.Content.Items.Accessories.Movement.Jumps;
using ITD.Content.Items.Accessories.Defensive.Buffs;

namespace ITD.Content.Items.Accessories.Misc
{
    internal class RooksBrew : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;

            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(gold: 5);

            Item.accessory = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddTile(TileID.TinkerersWorkbench)
                .AddIngredient<DashingFeather>()
                .AddIngredient<JumpingBean>()
                .AddIngredient<CupOJoe>()
                .Register();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.jumpSpeedBoost += 1;
            player.moveSpeed += 0.08f;

            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Frozen] = true;

            player.GetModPlayer<FeatherDashPlayer>().DashAccessoryEquipped = true;
        }
    }
}
