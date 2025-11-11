using System;

namespace ITD.Content.Projectiles.Friendly.Summoner;

public class ManuscriptSneakProj : ModProjectile
{
    public bool startAnim;
    public bool startExplode;
    private NPC FirstTarget
    {
        get => Projectile.ai[0] == 0 ? null : Main.npc[(int)Projectile.ai[0] - 1];
        set
        {
            Projectile.ai[0] = value == null ? 0 : value.whoAmI + 1;
        }
    }
    public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = 16;
        ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
        ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
    }
    public override void SetDefaults()
    {
        Projectile.width = 60;
        Projectile.height = 60;
        Projectile.netImportant = true;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = Projectile.SentryLifeTime;
        Projectile.penetrate = -1;
        Projectile.tileCollide = true;
        Projectile.sentry = true;
        Projectile.DamageType = DamageClass.Summon;
    }

    public override void AI()
    {
        float maxDetectRadius = 150f;
        FirstTarget ??= Projectile.FindClosestNPC(maxDetectRadius);

        if (FirstTarget != null)
        {
            startExplode = true;
        }
        if (Projectile.frame >= Main.projFrames[Projectile.type] - 1)
        {
            if (Projectile.timeLeft > 10)
            {
                for (int i = 0; i < 30; i++)
                {
                    Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.TintableDustLighted, 0f, 0f, 100, Color.HotPink, 4f);
                    dust.noGravity = true;
                    dust.velocity *= 5f;
                    dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.TintableDustLighted, 0f, 0f, 100, Color.HotPink, 3f);
                    dust.velocity *= 1.5f;
                }
                Projectile.frame = Main.projFrames[Projectile.type] - 1;
                Projectile.alpha = 255;

                Projectile.timeLeft = 10;
            }
            Projectile.Resize(300, 300);

            Projectile.tileCollide = false;

            Projectile.position = Projectile.Center;
            Projectile.Center = Projectile.position;
        }

        if (startAnim)
        {
            Projectile.frameCounter++;

            if (Projectile.frameCounter > 6)
            {
                if (Projectile.frame <= Main.projFrames[Projectile.type] - 1)
                {
                    Projectile.frame++;
                    if (!startExplode)
                    {
                        if (Projectile.frame >= 8)
                        {
                            Projectile.frame = 8;
                        }
                    }

                }
                Projectile.frameCounter = 0;
            }
        }
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC target = Main.npc[i];

            if (target.active && !target.friendly
                && Math.Abs(Projectile.Center.X - target.position.X)
                + Math.Abs(Projectile.Center.Y - target.position.Y) < Projectile.width * 2f)
            {
                if (Projectile.frame >= Main.projFrames[Projectile.type] - 1)
                {
                    target.AddBuff(BuffID.Confused, 1500);
                    target.AddBuff(BuffID.OnFire3, 1500);
                    target.AddBuff(BuffID.Ichor, 1500);

                }

            }
        }
        if (Projectile.velocity.Y < 16f)
        {
            Projectile.velocity.Y += 0.4f;
        }
        if (Projectile.timeLeft <= 30)
        {
            startExplode = true;
        }
    }
    public override Color? GetAlpha(Color lightColor)
    {
        return Color.White;
    }
    public override bool? CanDamage() => false;
    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        startAnim = true;
        return false;
    }
    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
    {
        fallThrough = false;
        return true;
    }
}