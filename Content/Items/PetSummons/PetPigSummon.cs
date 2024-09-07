using ITD.Content.Buffs.PetBuffs;
using ITD.Content.Projectiles.Friendly.Pets;
using ITD.Utilities.Placeholders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace ITD.Content.Items.PetSummons
{
    public class PetPigSummon : ModItem
    {
        public override string Texture => Placeholder.PHBottle;
        public override void SetDefaults()
        {
            Item.damage = 0;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item2;
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.rare = ItemRarityID.Yellow;
            Item.noMelee = true;
            Item.height = 32;
            Item.width = 32;
            Item.shoot = ModContent.ProjectileType<PetPigPet>();
            Item.buffType = ModContent.BuffType<PetPigBuff>();
        }
        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
            {
                player.AddBuff(Item.buffType, 3600);
            }
        }
    }
}
