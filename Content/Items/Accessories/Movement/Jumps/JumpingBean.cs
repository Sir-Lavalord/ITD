﻿namespace ITD.Content.Items.Accessories.Movement.Jumps
{
    public class JumpingBean : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;

            Item.value = Item.sellPrice(silver: 15);
            Item.rare = ItemRarityID.White;

            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.jumpSpeedBoost += 1;
            player.moveSpeed += 0.08f;
        }
    }
}