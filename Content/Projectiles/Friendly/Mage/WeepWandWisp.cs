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
        private int deathTimer = 0;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 8;
        }
        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Magic;
            Projectile.width = 18;
            Projectile.height = 160;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = (15);

            Projectile.frame = 0;
            Projectile.frameCounter = 0;
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
            float maxDetectRadius = 800f;

            if (Projectile.frame < 7)
            {
                if (DelayTimer < 2)
                {
                    Projectile.velocity *= 0.85f;
                    DelayTimer += 1;
                }
                else
                {
                    DelayTimer = 0;
                    Projectile.frame++;
                }
                return;
            }

            Projectile.friendly = true;
            Projectile.velocity *= 1.05f;

            if (Projectile.penetrate > 0)
            {
                HomingTarget ??= Projectile.FindClosestNPC(maxDetectRadius);

                if (HomingTarget == null)
                {
                    Projectile.velocity *= 0.90f;
                    deathTimer++;
                    if (deathTimer > 45)
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<WispDust>(), 0f, 0f, 150, default(Color), 1f);
                        }
                        Projectile.Kill();
                    }
                    return;
                }

                Vector2 directionToTarget = HomingTarget.Center - Projectile.Center;

                float targetAngle = directionToTarget.ToRotation();

                float currentAngle = Projectile.velocity.ToRotation();

                float angleStep = MathHelper.ToRadians(2);
                float newAngle = currentAngle.AngleTowards(targetAngle, angleStep);

                float length = Projectile.velocity.Length();
                Projectile.velocity = newAngle.ToRotationVector2() * length;

                Projectile.Center += Main.rand.NextVector2Circular(0.5f, 0.5f); 

                Projectile.rotation = newAngle + MathHelper.PiOver2;
            }
            else
            {
                for (int i = 0; i < 6; i++)
                {
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<WispDust>(), 0f, 0f, 150, default(Color), 1f);
                }
                Projectile.Kill();
            }
        }
    }
}