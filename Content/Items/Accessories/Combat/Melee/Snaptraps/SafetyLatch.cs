using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ITD.Systems;

namespace ITD.Content.Items.Accessories.Combat.Melee.Snaptraps
{
    public class SafetyLatch : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 58;
            Item.height = 22;
            Item.value = Item.buyPrice(10);
            Item.rare = ItemRarityID.Green;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<SnaptrapPlayer>().WarningModifer += 10;
        }
    }
}