using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.Audio;

using ITD.Content.Projectiles.Friendly.Melee;
using ITD.Systems;
using ITD.Utilities;
using ITD.Particles.Misc;
using ITD.Particles;

namespace ITD.Content.Items.Weapons.Melee
{
	public class Ectoblade : ModItem
	{
		//public int attackCycle = 0;
		public ParticleEmitter emitter;
		
		public override void SetStaticDefaults()
        {
            HeldItemLayer.RegisterData(Item.type, new DrawLayerData()
            {
                Texture = ModContent.Request<Texture2D>(Texture + "_Glow"),
                Color = () => Color.White
            });
            Item.ResearchUnlockCount = 1;
        }
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
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item15;
			Item.autoReuse = true;
			//Item.shoot = ModContent.ProjectileType<GhostlyBlade>();
			Item.shootSpeed = 20;
		}
		
		public override void MeleeEffects (Player player, Rectangle hitbox)
        {
			if (player.itemAnimation == player.itemAnimationMax)
			{
				emitter = ParticleSystem.NewEmitter<EctoCloud>(ParticleEmitterDrawCanvas.WorldUnderProjectiles);
				emitter.tag = Item;
				emitter.keptAlive = true;
			}
			
			if (emitter != null)
			{
                emitter.keptAlive = true;
				for (int j = 0; j < 2; j++) {
					Vector2 position;
					Vector2 spinningpoint;
					MiscHelpers.GetPointOnSwungItemPath(player, 60f, 60f, 0.5f + 0.6f * Main.rand.NextFloat(), player.GetAdjustedItemScale(Item), out position, out spinningpoint);
					Vector2 velocity = spinningpoint.RotatedBy((double)(1.57079637f * (float)player.direction * player.gravDir), default(Vector2));
					emitter?.Emit(position, velocity * 4f, 0f, 20);
				}
			}
        }
		
		//public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {			
		//	attackCycle = ++attackCycle % 2;
		//	return attackCycle == 1;
		//}
		
		public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers)
		{
			if (player.statMana >= player.statManaMax2 * 0.5f)
			{
				SoundEngine.PlaySound(SoundID.NPCDeath39, target.Center);
				ParticleEmitter hitEmitter = ParticleSystem.NewEmitter<EctoCloud>(ParticleEmitterDrawCanvas.WorldUnderProjectiles);
				hitEmitter.tag = Item;
				hitEmitter.keptAlive = true;
				
				for (int j = 0; j < 15; j++) {
					hitEmitter?.Emit(target.Center, Main.rand.NextVector2Circular(4f, 12f), 0f, 20);
				}
			}
			
			modifiers.FlatBonusDamage += player.GetTotalDamage(DamageClass.Magic).ApplyTo(player.statMana) * 0.5f;
			player.manaRegenDelay = player.maxRegenDelay;
			player.statMana = 0;

		}
		
		public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>(Texture + "_Glow");

            spriteBatch.Draw(texture, new Vector2(Item.position.X - Main.screenPosition.X + Item.width * 0.5f, Item.position.Y - Main.screenPosition.Y + Item.height - texture.Height * 0.5f),
                new Rectangle(0, 0, texture.Width, texture.Height), Color.White, rotation, texture.Size() * 0.5f, scale, SpriteEffects.None, 0f);
        }
	}
}