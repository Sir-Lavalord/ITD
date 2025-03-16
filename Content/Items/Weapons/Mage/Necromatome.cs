using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ITD.Content.Projectiles.Friendly.Mage;

namespace ITD.Content.Items.Weapons.Mage
{
	public class Necromatome : ModItem
	{
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults() {
            Item.DamageType = DamageClass.Magic;
            Item.damage = 18;
            Item.mana = 8;
            Item.width = 56;
            Item.height = 56;
            Item.useTime = 5;
            Item.useAnimation = 30;
			Item.reuseDelay = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 1f;
            Item.value = Item.buyPrice(silver: 10);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item103;
            Item.shoot = ModContent.ProjectileType<NecromatomeProjectile>();
            Item.shootSpeed = 14f;
		}
		
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
			Vector2 newVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(15));
				newVelocity *= 1.25f - Main.rand.NextFloat(0.5f);
			Projectile.NewProjectile(source, position, newVelocity, type, damage, knockback, player.whoAmI);
            return false;
        }
	}
}
