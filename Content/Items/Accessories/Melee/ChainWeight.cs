using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Items.Accessories.Melee
{
    public class ChainWeight : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.value = Item.buyPrice(10);
            Item.rare = ItemRarityID.Green;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<SnaptrapPlayer>().ChainWeightEquipped = true;
        }
    }

    public class SnaptrapPlayer : ModPlayer
    {
        public bool ChainWeightEquipped;

        public override void ResetEffects()
        {
            ChainWeightEquipped = false;
        }
    }
}