using System;

namespace ITD.Content.Items.Accessories.Expert
{
    public class WindCape : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.value = Item.sellPrice(50000);
            Item.expert = true;
            Item.accessory = true;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.moveSpeed += 0.1f;
            Main.windSpeedCurrent = (Main.windSpeedCurrent * 9f + Math.Clamp(player.velocity.X, -6f, 6f) * 0.2f) * 0.1f;
        }
    }
}
