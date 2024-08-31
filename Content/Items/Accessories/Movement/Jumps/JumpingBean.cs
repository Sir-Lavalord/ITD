using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using System;
using ITD.Utilities;
using ITD.Utilities.Placeholders;

namespace ITD.Content.Items.Accessories.Movement.Jumps
{
    public class JumpingBean : ModItem
    {
        public override string Texture => Placeholder.PHGeneric;
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
            player.moveSpeed *= 1.08f;
        }
    }
}