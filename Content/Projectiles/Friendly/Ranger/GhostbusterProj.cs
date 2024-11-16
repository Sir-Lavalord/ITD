using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

using ITD.Players;
using ITD.Utilities;
using ITD.Systems.DataStructures;
using ITD.Systems.Extensions;

namespace ITD.Content.Projectiles.Friendly.Ranger
{
    public class GhostbusterProj: ModProjectile
    {
		public VertexStrip TrailStrip = new VertexStrip();
		public NPC TargetLock;
		public int hitDelay = 0;
		
        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.width = 8; Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
			Player player = Main.player[Projectile.owner];
			ITDPlayer modPlayer = player.GetITDPlayer();
            Vector2 mouse = modPlayer.MousePosition;

			Projectile.timeLeft = 60;

			Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter);
			if (Main.myPlayer == Projectile.owner) {
				if (player.channel) {
					float holdoutDistance = 36f;
					Vector2 holdoutOffset = holdoutDistance * Vector2.Normalize(mouse - playerCenter).RotatedBy(-0.075f*player.direction);
					if (holdoutOffset.X != Projectile.velocity.X || holdoutOffset.Y != Projectile.velocity.Y) {
						Projectile.netUpdate = true;
					}
					Projectile.velocity = holdoutOffset;
				}
				else {
					Projectile.Kill();
				}
			}
			player.heldProj = Projectile.whoAmI;
			player.SetDummyItemTime(2);
			
			Vector2 position = new Vector2((float)((int)(player.position.X - (float)(player.bodyFrame.Width / 2) + (float)(player.width / 2))), (float)((int)(player.position.Y + (float)player.height - (float)player.bodyFrame.Height + 4f))) + player.bodyPosition + new Vector2((float)(player.bodyFrame.Width / 2), (float)(player.bodyFrame.Height / 2)) - new Vector2(player.direction*5f, 0f);
			
			Vector2 value = Main.OffsetsPlayerHeadgear[player.bodyFrame.Y / player.bodyFrame.Height];
			value.Y -= 2f;
			position += value * player.gravDir;
			
			Projectile.Center = position;
			
            NPC closestNPC = null;

            float MaxDistance = 2560000;

            foreach (var target in Main.ActiveNPCs)
            {
				float idktbh = 0f;
				Rectangle targetHitbox = target.Hitbox;
                if (!target.friendly && Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), playerCenter, playerCenter + Vector2.Normalize(mouse - playerCenter) * 320f, 100f, ref idktbh))
                {
					float MouseToTarget = Vector2.DistanceSquared(target.Center, mouse);

                    if (MouseToTarget < MaxDistance)
                    {
                        MaxDistance = MouseToTarget;
                        closestNPC = target;
                    }
                }
            }
			TargetLock = closestNPC;
			
			if (TargetLock != null)
			{
				if (hitDelay == 0)
				{
					int damage = Projectile.damage;
					bool crit = false;
					if (Main.rand.Next(1, 101) <= player.HeldItem.crit + player.GetCritChance(player.HeldItem.DamageType))
					{
						crit = true;
						damage *= 2;
					}
					TargetLock.StrikeNPC(new NPC.HitInfo
					{
						Damage = Main.DamageVar(damage, player.luck),
						Knockback = Projectile.knockBack,
						HitDirection = TargetLock.Center.X < player.Center.X ? -1 : 1,
						Crit = crit
					});
					hitDelay = 10;
				}
			}
			if (hitDelay > 0)
				hitDelay--;
			
			modPlayer.recoilFront = modPlayer.recoilBack = Main.rand.NextFloat(0.15f);
        }
		
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return false;
		}
		
		private Color StripColorBlue(float progressOnStrip)
		{
			return new Color(200, 200, 255);
		}
		private Color StripColorOrange(float progressOnStrip)
		{
			return new Color(255, 255, 200);
		}
		private float StripWidth1(float progressOnStrip)
		{
			return 64f;
		}
		private float StripWidth2(float progressOnStrip)
		{
			return 16f;
		}
		public override bool PreDraw(ref Color lightColor)
		{
			if (TargetLock != null)
			{
				float toTarget = (TargetLock.Center - Projectile.Center).ToRotation();
				Vector2[] positions = {TargetLock.Center, Projectile.Center};
				float[] rotations = {toTarget, toTarget};

				MiscShaderData BlueShader = GameShaders.Misc["MagicMissile"];
				BlueShader.UseSaturation(-2.8f);
				BlueShader.UseOpacity(2f);
				BlueShader.Apply(null);
				
				TrailStrip.PrepareStrip(positions, rotations, StripColorBlue, StripWidth1, - Main.screenPosition, positions.Length, true);
				TrailStrip.DrawTrail();
				
				//MiscShaderData OrangeShader = GameShaders.Misc["FlameLash"];
				//OrangeShader.UseSaturation(-2f);
				//OrangeShader.UseOpacity(4f);
				//OrangeShader.Apply(null);
				
				//TrailStrip.PrepareStrip(positions, rotations, StripColorOrange, StripWidth2, - Main.screenPosition, positions.Length, true);
				//TrailStrip.DrawTrail();
				
				Main.spriteBatch.End(out SpriteBatchData spriteBatchData); // unapply shaders
				Main.spriteBatch.Begin(spriteBatchData);
			}			
			return false;
		}
    }
}
