using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;


namespace ITD.Content.Projectiles.Friendly.Misc
{
    public class StarlightProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            Main.projFrames[Projectile.type] = 5;
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
        public bool bCollapse;
        public ref float DelayTimer => ref Projectile.ai[1];

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
            if (Main.rand.NextBool(3))
            {
                if (Main.rand.NextBool(2))
                {
                    //Yellow D
                    int dust = Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height,
                    DustID.TintableDustLighted, Projectile.velocity.X / 2, Projectile.velocity.Y / 2, 80, new Color(249, 203, 151), Main.rand.NextFloat(1.6f, 2f));
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 2f;
                }
                else
                {
                    //Blue D
                    int dust = Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height,
                    DustID.TintableDustLighted, Projectile.velocity.X / 2, Projectile.velocity.Y / 2, 80, new Color(168, 241, 255), Main.rand.NextFloat(1.2f, 1.8f));
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 1f;
                }
            }
            float maxDetectRadius = 200f;

            if (DelayTimer < 10)
            {
                DelayTimer += 1;
                return;
            }

            if (!bCollapse)
            {
                
                if (HomingTarget == null)
                {
                    HomingTarget = FindClosestNPC(maxDetectRadius);
                }

                if (HomingTarget != null && !IsValidTarget(HomingTarget))
                {
                    HomingTarget = null;
                }

                if (HomingTarget == null)
                    return;


                float length = Projectile.velocity.Length();
                float targetAngle = Projectile.AngleTo(HomingTarget.Center);
                Projectile.velocity = Projectile.velocity.ToRotation().AngleTowards(targetAngle, MathHelper.ToRadians(6)).ToRotationVector2() * length;
                Projectile.Center += Main.rand.NextVector2Circular(1, 1);
                Projectile.alpha = 60;//The sprite is not good enough to be fully seen rn
            }
            else
            {
                Projectile.velocity *= 0f;
                int frameSpeed = 6;
                Projectile.frameCounter++;
                if (Projectile.frameCounter >= frameSpeed)
                {
                    Projectile.frameCounter = 0;
                    Projectile.frame++;
                    if (Projectile.frame >= Main.projFrames[Projectile.type])
                    {

                        Projectile.timeLeft = 3;
                    }
                }
                if (Projectile.owner == Main.myPlayer && Projectile.timeLeft <= 3)
                {
                    Projectile.alpha = 255;
                    Projectile.Resize(200, 200);

                    Projectile.tileCollide = false;
                   
                    Projectile.position = Projectile.Center;
                    Projectile.Center = Projectile.position;
                }
            }
        }
        public override bool? CanDamage()
        {
            if (Projectile.timeLeft <= 3 || !bCollapse)
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
            ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.StardustPunch,
    new ParticleOrchestraSettings { PositionInWorld = (target.Center) },
    Projectile.owner);
            if (!bCollapse)
            {

                Projectile.timeLeft = 99999;
                Projectile.frame = 1;
                bCollapse = true;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.StardustPunch,
new ParticleOrchestraSettings { PositionInWorld = (Projectile.Center) },
Projectile.owner);
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);

            if (!bCollapse)
            {
                Projectile.timeLeft = 99999;
                Projectile.frame = 1;
                bCollapse = true;
            }
            return false;
        }
        public override void OnKill(int timeLeft)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                for (int i = 0; i < 20; i++)
                {
                    ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.StardustPunch,
                        new ParticleOrchestraSettings { PositionInWorld = Main.rand.NextVector2Circular(Projectile.width, Projectile.height) },Projectile.owner);
                    //Yellow D
                    int dustY = Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height,
                        DustID.TintableDustLighted, Projectile.velocity.X / 2, Projectile.velocity.Y / 2, 80, new Color(249, 203, 151), Main.rand.NextFloat(.8f, 1.2f));
                        Main.dust[dustY].noGravity = true;
                        Main.dust[dustY].velocity *= 2f;

                        //Blue D
                        int dustB = Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height,
                        DustID.TintableDustLighted, Projectile.velocity.X / 2, Projectile.velocity.Y / 2, 80, new Color(168, 241, 255), Main.rand.NextFloat(.8f, 1.2f));
                        Main.dust[dustB].noGravity = true;
                        Main.dust[dustB].velocity *= 1f;

                    //Magenta D
                    int dustM = Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height,
                    DustID.TintableDustLighted, Projectile.velocity.X / 2, Projectile.velocity.Y / 2, 80, new Color(192, 59, 166), Main.rand.NextFloat(.8f, 1.2f));
                    Main.dust[dustM].noGravity = true;
                    Main.dust[dustM].velocity *= 0.8f;

                }

            }
        }
        
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * Projectile.Opacity;
        }
        public NPC FindClosestNPC(float maxDetectDistance)
        {
            NPC closestNPC = null;

            float sqrMaxDetectDistance = maxDetectDistance * maxDetectDistance;

            foreach (var target in Main.ActiveNPCs)
            {
                if (IsValidTarget(target))
                {
                    float sqrDistanceToTarget = Vector2.DistanceSquared(target.Center, Projectile.Center);

                    if (sqrDistanceToTarget < sqrMaxDetectDistance)
                    {
                        sqrMaxDetectDistance = sqrDistanceToTarget;
                        closestNPC = target;
                    }
                }
            }

            return closestNPC;
        }
        //Make the invul boss part untargetable please
        public bool IsValidTarget(NPC target)
        {
            return target.CanBeChasedBy() && Collision.CanHit(Projectile.Center, 1, 1, target.position, target.width, target.height);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            int num156 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type]; //ypos of lower right corner of sprite to draw
            int y3 = num156 * Projectile.frame; //ypos of upper left corner of sprite to draw
            Rectangle rectangle = new Rectangle(0, y3, texture2D13.Width, num156);
            Vector2 origin2 = rectangle.Size() / 2f;

            Color color26 = lightColor;
            color26 = Projectile.GetAlpha(color26);

            SpriteEffects effects = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Projectile.type]; i++)
            {
                Color color27 = Color.White * Projectile.Opacity * 0.75f * 0.5f;
                color27 *= (float)(ProjectileID.Sets.TrailCacheLength[Projectile.type] - i) / ProjectileID.Sets.TrailCacheLength[Projectile.type];
                Vector2 value4 = Projectile.oldPos[i];
                float num165 = Projectile.oldRot[i];
                Main.EntitySpriteDraw(texture2D13, value4 + Projectile.Size / 2f - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), color27, num165, origin2, Projectile.scale, SpriteEffects.FlipVertically, 0);
            }

            Main.EntitySpriteDraw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), Projectile.GetAlpha(lightColor), Projectile.rotation, origin2, Projectile.scale, SpriteEffects.FlipVertically, 0);
            return false;
        }
    }
}
