﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using ITD.Content.Items.Materials;
using ITD.Content.Projectiles.Friendly.Ranger.Ammo;
using Microsoft.Xna.Framework;
using ITD.Content.Projectiles;

namespace ITD.Content.Items.Weapons.Ranger
{
    public class Skyshot : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 9;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 30;
            Item.height = 64;
            Item.useTime = 36;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 2;
            Item.value = Item.sellPrice(silver: 50);
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item5;
            Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.shootSpeed = 8f;
            Item.useAmmo = AmmoID.Arrow;
            Item.autoReuse = false;
        }
		
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
			if (Main.rand.NextBool(4))
            {
                Vector2 newVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(8));

                newVelocity *= 1f - Main.rand.NextFloat(0.3f);
                Item.shootSpeed = 6f;
                int proj = Projectile.NewProjectile(source, position, newVelocity, type, damage, knockback, player.whoAmI);
                Main.projectile[proj].GetGlobalProjectile<ITDInstancedGlobalProjectile>().isFromSkyProjectileBow = true;
                return false;
            } 
            else
            {
                Item.shootSpeed = 8f;
                return true;
            }
        }
		
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<ElectrumBar>(), 10);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}
