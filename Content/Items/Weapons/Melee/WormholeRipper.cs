using ITD.Content.Projectiles.Friendly.Misc;
using ITD.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using ITD.Utilities;
using Terraria.Audio;

namespace ITD.Content.Items.Weapons.Melee
{
    public class WormholeRipper : ModItem
    {
        public override void SetDefaults()
        {
            Item.autoReuse = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useAnimation = 15;
            Item.useTime = 15;
            Item.knockBack = 6f;
            Item.width = 30;
            Item.height = 10;
            Item.damage = 24;
            Item.shoot = ModContent.ProjectileType<WRipperSlash>();
			Item.shootSpeed = 4;
            Item.UseSound = SoundID.Item1;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(gold: 1);
            Item.DamageType = DamageClass.Melee;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
        }

        public override bool AltFunctionUse(Player player)
        {
			ITDPlayer modPlayer = player.GetModPlayer<ITDPlayer>();
            return modPlayer.itemVar[0] == 3;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            { //wormhole dash
				player.GetITDPlayer().itemVar[0] = 0;
                Item.useStyle = ItemUseStyleID.Shoot;
                Item.shoot = ModContent.ProjectileType<WRipperRift>();
				player.armorEffectDrawShadow = true;
				player.jump = 0;
				player.velocity = Vector2.Normalize(Main.MouseWorld - player.Center) * 14f;
                SoundStyle wRipperRip = new SoundStyle("ITD/Content/Sounds/WRipperRip");
                SoundEngine.PlaySound(wRipperRip, player.Center);
                for (int index1 = 0; index1 < 12; ++index1)
				{
					int index2 = Dust.NewDust(player.position, player.width, player.height, DustID.TreasureSparkle, 0.0f, 0.0f, 100, new Color(), 1f);
					Main.dust[index2].velocity = player.velocity*Main.rand.Next(10)*0.1f;
					Main.dust[index2].scale *= 1f + Main.rand.Next(40) * 0.01f;
				}
				
                return true;
            }
            else
            { //regular swing
                Item.useStyle = ItemUseStyleID.Swing;
                Item.shoot = ModContent.ProjectileType<WRipperSlash>();
                return true;
            }
        }
		
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
			if (Item.shoot == ModContent.ProjectileType<WRipperRift>())
			{
				velocity *=0;
				knockback *= 0.5f;
			}
			Vector2 randomSpread = velocity.RotatedByRandom(MathHelper.ToRadians(10));
            Projectile proj = Projectile.NewProjectileDirect(source, position, randomSpread, Item.shoot, damage, knockback, player.whoAmI);
			proj.spriteDirection = player.direction;
            return false;
        }
    }
}