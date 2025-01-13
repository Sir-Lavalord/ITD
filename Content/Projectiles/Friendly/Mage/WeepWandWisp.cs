using ITD.Content.Dusts;
using ITD.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Friendly.Mage
{
    public class WeepWandWisp : ModProjectile
    {
        private int DelayTimer = 0;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }
        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Magic;
            Projectile.width = 20;
            Projectile.height = 72;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = (15);

            Projectile.frame = 1;
        }
        private NPC HomingTarget
        {
            get => Projectile.ai[0] == 0 ? null : Main.npc[(int)Projectile.ai[0] - 1];
            set
            {
                Projectile.ai[0] = value == null ? 0 : value.whoAmI + 1;
            }
        }

        public override void AI()
        {
            Projectile.damage = 0;
            float maxDetectRadius = 400f;

            if (DelayTimer < 16)
            {
                Projectile.velocity *= 0.85f;
                DelayTimer += 1;
                return;
            } 
            else
            {
                Projectile.velocity *= 1.05f;
            }

            if (Projectile.frame < 4)
            {
                ++Projectile.frame;
            }
            else
            {
                return;
            }

            if (Main.rand.NextBool(3))
            {
                for (int i = 0; i < 3; i++)
                {
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<WispDust>(), 0f, 0f, 150, default(Color), 0.5f);
                }
            }

            Projectile.damage = 7;
            Projectile.frame = 4;

            if (Projectile.penetrate > 0)
            {
                HomingTarget ??= Projectile.FindClosestNPC(maxDetectRadius);

                if (HomingTarget == null)
                {
                    return;
                }
                else
                {
                    //Projectile.alpha = 0;
                }

                Vector2 directionToTarget = HomingTarget.Center - Projectile.Center;
                directionToTarget.Normalize();

                float length = Projectile.velocity.Length();
                float targetAngle = Projectile.AngleTo(HomingTarget.Center);
                Projectile.velocity = Projectile.velocity.ToRotation().AngleTowards(targetAngle, MathHelper.ToRadians(6)).ToRotationVector2() * length;
                Projectile.Center += Main.rand.NextVector2Circular(1, 1);

                Projectile.rotation = directionToTarget.ToRotation();
            }
            else
            {
                for (int i = 0; i < 6; i++)
                {
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<WispDust>(), 0f, 0f, 150, default(Color), 0.5f);
                }
                Projectile.Kill();
            }
        }
    }
}