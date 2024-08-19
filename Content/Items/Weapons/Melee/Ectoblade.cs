using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ITD.Content.Projectiles.Friendly.Misc;
using System;

namespace ITD.Content.Items.Weapons.Melee
{
	public class Ectoblade : ModItem
	{
		public int attackType = 0;
		
		public override void SetDefaults()
		{
			Item.damage = 28;
			Item.DamageType = DamageClass.Melee;
			Item.width = 50;
			Item.height = 50;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 5;
			Item.value = 10000;
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item15;
			Item.autoReuse = true;
			Item.shoot = ModContent.ProjectileType<GhostlyBlade>();
			Item.shootSpeed = 20;
		}
		
		public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
		
		private void GetPointOnSwungItemPath(Player player, float spriteWidth, float spriteHeight, float normalizedPointOnPath, float itemScale, out Vector2 location, out Vector2 outwardDirection)
		{
			float scaleFactor = (float)Math.Sqrt((double)(spriteWidth * spriteWidth + spriteHeight * spriteHeight));
			float num = (float)(player.direction == 1).ToInt() * 1.57079637f;
			if (player.gravDir == -1f)
			{
				num += 1.57079637f * (float)player.direction;
			}
			outwardDirection = player.itemRotation.ToRotationVector2().RotatedBy((double)(3.926991f + num), default(Vector2));
			location = player.RotatedRelativePoint(player.itemLocation + outwardDirection * scaleFactor * normalizedPointOnPath * itemScale, false, true);
		}
		
		public override void MeleeEffects (Player player, Rectangle hitbox)
        {
			Vector2 position;
			Vector2 spinningpoint;
			GetPointOnSwungItemPath(player, 60f, 60f, 0.2f + 0.8f * Main.rand.NextFloat(), player.GetAdjustedItemScale(Item), out position, out spinningpoint);
			Vector2 value = spinningpoint.RotatedBy((double)(1.57079637f * (float)player.direction * player.gravDir), default(Vector2));
			Dust.NewDustPerfect(position, 180, new Vector2?(value * 4f), 100, default(Color), 1.5f).noGravity = true;
        }
		
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			attackType = ++attackType % 2;
			return attackType == 1;
		}
	}
}