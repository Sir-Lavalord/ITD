using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using ITD.Utilities;

namespace ITD.Content.Projectiles.Friendly.Mage
{
    public class StarlightStaffProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
			ProjectileID.Sets.TrailCacheLength [Type] = 6;
			ProjectileID.Sets.TrailingMode [Type] = 0;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }
        private NPC HomingTarget
        {
            get => Projectile.ai[2] == 0 ? null : Main.npc[(int)Projectile.ai[2] - 1];
            set
            {
                Projectile.ai[2] = value == null ? 0 : value.whoAmI + 1;
            }
        }
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.aiStyle = 0;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.light = 1f;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 600;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;

        }
        public override void AI()
        {
            float maxDetectRadius = 200f;

            if (Projectile.timeLeft > 36)
            {				
                HomingTarget ??= Projectile.FindClosestNPCDirect(maxDetectRadius);

                if (HomingTarget != null && !Projectile.IsValidTarget(HomingTarget))
                {
                    HomingTarget = null;
                }

                if (HomingTarget == null)
                    return;


                float length = Projectile.velocity.Length();
                float targetAngle = Projectile.AngleTo(HomingTarget.Center);
                Projectile.velocity = Projectile.velocity.ToRotation().AngleTowards(targetAngle, MathHelper.ToRadians(6)).ToRotationVector2() * length;
                Projectile.Center += Main.rand.NextVector2Circular(1, 1);
            }
            else
            {
                Projectile.velocity *= 0f;
                int frameSpeed = 6;
                Projectile.frameCounter++;
                if (Projectile.frameCounter >= frameSpeed && Projectile.timeLeft > 10)
                {
                    Projectile.frameCounter = 0;
                    Projectile.frame++;
                    if (Projectile.frame >= Main.projFrames[Projectile.type])
                    {
						SoundEngine.PlaySound(SoundID.Item62, Projectile.Center);
                    }
                }
                if (Projectile.owner == Main.myPlayer && Projectile.timeLeft <= 10)
                {
                    Projectile.Resize(200, 200);

                    Projectile.tileCollide = false;
                   
                    Projectile.position = Projectile.Center;
                    Projectile.Center = Projectile.position;
                }
            }
        }
        public override bool? CanDamage()
        {
            if (Projectile.timeLeft <= 10 || Projectile.timeLeft > 36)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.timeLeft > 36)
            {
                Projectile.timeLeft = 36;
                Projectile.frame = 1;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
			if (Projectile.oldPos[0] != new Vector2())
				Projectile.position = Projectile.oldPos[0];
			
            if (Projectile.timeLeft > 36)
            {
                Projectile.timeLeft = 36;
                Projectile.frame = 1;
            }
            return false;
        }
        
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
        public override bool PreDraw(ref Color lightColor)
        {
			if (Projectile.timeLeft > 10)
			{
				Texture2D trailTexture = ModContent.Request<Texture2D>("ITD/Content/Projectiles/Friendly/Mage/StarlightStaffProj_Trail").Value;
				Vector2 origin = trailTexture.Frame(1, 1).Size() / 2f;
				for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Projectile.type]; i++)
				{
					Vector2 oldPos = Projectile.oldPos[i];
					Main.EntitySpriteDraw(trailTexture, oldPos + Projectile.Size / 2f - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(trailTexture.Frame(1, 1)), Color.White, 0, origin, Projectile.scale*1.25f-(Projectile.scale*0.2f*i), SpriteEffects.None, 0);
				}
			}
			else
			{
				Vector2 position = Projectile.Center - Main.screenPosition;
				Texture2D texture = ModContent.Request<Texture2D>("ITD/Content/Projectiles/Friendly/Melee/WRipperRift").Value;
				Rectangle sourceRectangle = texture.Frame(1, 1);
				Vector2 origin = sourceRectangle.Size() / 2f;
				
				float scaleMultipler = (20f-Projectile.timeLeft)*0.1f;
				float colorMultiplier = Math.Min(1, Projectile.timeLeft*0.2f);
				
				Main.EntitySpriteDraw(texture, position, sourceRectangle, new Color(36, 12, 34)*colorMultiplier, scaleMultipler*2f, origin, scaleMultipler*2f, SpriteEffects.None, 0f);
				Main.EntitySpriteDraw(texture, position, sourceRectangle, new Color(133, 50, 88)*colorMultiplier, scaleMultipler*1.5f, origin, scaleMultipler*1.5f, SpriteEffects.None, 0f);
				Main.EntitySpriteDraw(texture, position, sourceRectangle, new Color(255, 244, 191)*colorMultiplier, scaleMultipler, origin, scaleMultipler, SpriteEffects.None, 0f);

			}

            return Projectile.timeLeft > 10;
        }
    }
}
