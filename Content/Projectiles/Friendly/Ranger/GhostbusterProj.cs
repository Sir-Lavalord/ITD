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
		public Vector2 VacuumCleaner;
		
		public ref float FadeIn => ref Projectile.ai[0];
		
        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.width = 8; Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
        }

		public override bool? CanHitNPC(NPC target)
		{
			if (target != TargetLock)
				return false;
			return base.CanHitNPC(target);
		}

        public override void AI()
        {
			Player player = Main.player[Projectile.owner];
			ITDPlayer modPlayer = player.GetITDPlayer();
            Vector2 mouse = modPlayer.MousePosition;

			if (FadeIn < 2f)
				FadeIn += 0.1f;

			Projectile.timeLeft = 60;

			Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter);
			
			if (!player.channel)
				Projectile.Kill();
			
			player.heldProj = Projectile.whoAmI;
			player.SetDummyItemTime(2);
			
			Vector2 position = new Vector2((float)((int)(player.position.X - (float)(player.bodyFrame.Width / 2) + (float)(player.width / 2))), (float)((int)(player.position.Y + (float)player.height - (float)player.bodyFrame.Height + 4f))) + player.bodyPosition + new Vector2((float)(player.bodyFrame.Width / 2), (float)(player.bodyFrame.Height / 2)) - new Vector2(player.direction*5f, 0f);
			
			Vector2 value = Main.OffsetsPlayerHeadgear[player.bodyFrame.Y / player.bodyFrame.Height];
			value.Y -= 2f;
			position += value * player.gravDir;
			
			float holdoutDistance = 36f;
			Vector2 holdoutOffset = holdoutDistance * Vector2.Normalize(mouse - playerCenter).RotatedBy(-0.075f*player.direction);
			position += holdoutOffset;
			
			VacuumCleaner = position;
			
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
				Projectile.Center = TargetLock.Center;
			else
				Projectile.Center = VacuumCleaner;
			
			modPlayer.recoilFront = modPlayer.recoilBack = Main.rand.NextFloat(0.15f);
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
				//float toTarget = (TargetLock.Center - Projectile.Center).ToRotation();

				// you can increase this as much as you want
				// a value this low looks weird in certain positions but that's why i'm limiting the midpoint's position
				int res = 20;

				// <>AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
                float yRange = 100f;
                Vector2 mouse = Main.player[Projectile.owner].GetITDPlayer().MousePosition;
                Vector2 realMidPoint = Vector2.Lerp(VacuumCleaner, TargetLock.Center, 0.5f);

				// clamp to avoid long noodle
                mouse.Y = MathHelper.Clamp(mouse.Y, VacuumCleaner.Y - yRange, TargetLock.Center.Y + yRange);
                mouse.X = MathHelper.Clamp(mouse.X,
                    Projectile.direction == 1 ? VacuumCleaner.X : TargetLock.Center.X,
                    Projectile.direction == 1 ? TargetLock.Center.X : VacuumCleaner.X);

				// lerp to avoid weird segments
				float angleDiffRange = 0.5f;
				float angleDiff = MiscHelpers.AngleDiff(VacuumCleaner, mouse, TargetLock.Center);
				float lerp0 = MathHelper.Clamp(angleDiff / angleDiffRange, 0f, 1f);
				mouse = Vector2.Lerp(mouse, realMidPoint, lerp0);

				// lerp to avoid more weird segments
                float closenessRange = 100f;
                float lerp = MathHelper.Clamp(VacuumCleaner.DistanceSQ(TargetLock.Center) / (closenessRange * closenessRange), 0f, 1f);
				mouse = Vector2.Lerp(mouse, realMidPoint, 1f - lerp);
				// AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA</>

                Vector2[] positions = Bezier.Quadratic(TargetLock.Center, mouse, VacuumCleaner, res);
				float[] rotations = Bezier.Rotations(positions);

                MiscShaderData BlueShader = GameShaders.Misc["MagicMissile"];
				BlueShader.UseSaturation(-2.8f);
				BlueShader.UseOpacity(FadeIn);
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
