using ITD.Utilities;
using Terraria.Audio;

namespace ITD.Content.Projectiles.Friendly.Melee.Snaptraps.Extra
{
    public class MiniMandible : ModProjectile
    {
        private int DelayTimer = 0;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.damage = 99;
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 600;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = (15);
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

            if (DelayTimer < 5)
            {
                DelayTimer += 1;
                return;
            }

            Projectile.damage = 10;

            if (Projectile.penetrate > 1)
            {
                HomingTarget ??= Projectile.FindClosestNPC(maxDetectRadius);

                if (HomingTarget == null)
                    return;
                if (!HomingTarget.active || HomingTarget.life <= 0 || !HomingTarget.CanBeChasedBy())
                {
                    HomingTarget = null;
                    return;
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
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Sand, 0f, 0f, 150, default(Color), 1.5f);
                }
                Projectile.Kill();
            }

            if (++Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame >= Main.projFrames[Projectile.type])
                    Projectile.frame = 0;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextBool(5))
            {
                SoundEngine.PlaySound(SoundID.NPCHit31, Projectile.Center);
            }
        }
    }
}