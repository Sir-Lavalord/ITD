using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

using ITD.Content.Buffs.Debuffs;
using ITD.Content.Projectiles.Friendly.Summoner;
using ITD.Content.Items.Materials;
using ITD.Utilities;

namespace ITD.Content.Items.Weapons.Summoner
{
	public class RiteOfImmolation : ModItem
	{		
		public override void SetDefaults()
		{
			Item.damage = 15;
			Item.DamageType = DamageClass.SummonMeleeSpeed;
			Item.width = 30;
			Item.height = 34;
			Item.useTime = 10;
			Item.useAnimation = 10;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 5;
			Item.value = 10000;
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item20;
			Item.autoReuse = true;
			Item.shoot = ModContent.ProjectileType<RiteOfImmolationProj>();
			Item.shootSpeed = 128f;
		}
		
		public override void MeleeEffects (Player player, Rectangle hitbox)
        {
			Vector2 position;
			Vector2 spinningpoint;
			MiscHelpers.GetPointOnSwungItemPath(player, 30f, 30f, 0.2f + 0.8f * Main.rand.NextFloat(), player.GetAdjustedItemScale(Item), out position, out spinningpoint);
			Vector2 value = spinningpoint.RotatedBy((double)(1.57079637f * (float)player.direction * player.gravDir), default(Vector2));
			Dust.NewDustPerfect(position, 6, new Vector2?(value * 4f), 0, default(Color), 1.5f).noGravity = true;
        }
		
		public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(ModContent.BuffType<RiteOfImmolationTagDebuff>(), 300);
			player.MinionAttackTargetNPC = target.whoAmI;
		}
		
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
			float adjustedItemScale = player.GetAdjustedItemScale(player.inventory[player.selectedItem]);
			Main.projectile[Projectile.NewProjectile(source, position + velocity * adjustedItemScale, new Vector2(), type, damage, knockback, player.whoAmI, adjustedItemScale)].rotation = Main.rand.NextFloat(MathHelper.Pi);
            return false;
        }
		
		public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ModContent.ItemType<EmberlionMandible>(), 8)
				.AddIngredient(ModContent.ItemType<EmberlionSclerite>(), 6)
                .AddTile(TileID.Anvils)
                .Register();
        }
	}
}