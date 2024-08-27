using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ITD.Content.Projectiles.Friendly.Misc;
using ITD.Systems;
using ITD.Utilities;
using System;

namespace ITD.Content.Items.Weapons.Melee
{
	public class Ectoblade : ModItem
	{
		public int attackType = 0;
		
		public override void SetStaticDefaults()
        {
            HeldItemLayer.RegisterData(Item.type, new DrawLayerData()
            {
                Texture = ModContent.Request<Texture2D>(Texture + "_Glow"),
                Color = () => new Color(255, 255, 255, 50) * 0.7f
            });
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
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item15;
			Item.autoReuse = true;
			Item.shoot = ModContent.ProjectileType<GhostlyBlade>();
			Item.shootSpeed = 20;
		}
		
		public override void MeleeEffects (Player player, Rectangle hitbox)
        {
			Vector2 position;
			Vector2 spinningpoint;
			MiscHelpers.GetPointOnSwungItemPath(player, 60f, 60f, 0.2f + 0.8f * Main.rand.NextFloat(), player.GetAdjustedItemScale(Item), out position, out spinningpoint);
			Vector2 value = spinningpoint.RotatedBy((double)(1.57079637f * (float)player.direction * player.gravDir), default(Vector2));
			Dust.NewDustPerfect(position, 180, new Vector2?(value * 4f), 100, default(Color), 1.5f).noGravity = true;
        }
		
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			attackType = ++attackType % 2;
			return attackType == 1;
		}
		
		public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>(Texture + "_Glow");

            spriteBatch.Draw(texture, new Vector2(Item.position.X - Main.screenPosition.X + Item.width * 0.5f, Item.position.Y - Main.screenPosition.Y + Item.height - texture.Height * 0.5f),
                new Rectangle(0, 0, texture.Width, texture.Height), new Color(255, 255, 255, 50) * 0.7f, rotation, texture.Size() * 0.5f, scale, SpriteEffects.None, 0f);
        }
	}
}