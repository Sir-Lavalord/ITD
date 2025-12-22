using Terraria.Audio;

namespace ITD.Content.Projectiles.Hostile.Sandberus;

public class SandberusSkull : ModProjectile
{
    public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = 5;
    }
    public override void SetDefaults()
    {
        Projectile.width = 48;
        Projectile.height = 48;
        Projectile.penetrate = -1;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.timeLeft = 300;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
    }

    public override void AI()
    {
        if (Projectile.velocity.Length() < 18f)
        {
            Projectile.velocity *= 1.05f;
        }
        if (Projectile.localAI[0] == 0f)
        {
            Projectile.localAI[0] = 1f;
            for (int i = 0; i < 3; i++)
            {
                int spawnDust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Dirt, Projectile.velocity.X, Projectile.velocity.Y, 0, default, 2f);
                Main.dust[spawnDust].noGravity = true;
                Main.dust[spawnDust].velocity = Projectile.Center - Main.dust[spawnDust].position;
                Main.dust[spawnDust].velocity.Normalize();
                Main.dust[spawnDust].velocity *= -5f;
                Main.dust[spawnDust].velocity += Projectile.velocity / 2f;
            }
        }

        Projectile.spriteDirection = (Projectile.velocity.X > 0).ToDirectionInt();

        if (Projectile.spriteDirection == 1)
            Projectile.rotation = Projectile.velocity.ToRotation();
        else
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2 * 2;

        if (++Projectile.frameCounter >= 6)
        {
            Projectile.frameCounter = 0;
            Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];

            int trailDust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Dirt, Projectile.velocity.X, Projectile.velocity.Y, 0, default, 1.5f);
            Main.dust[trailDust].noGravity = true;
            Main.dust[trailDust].velocity *= 0f;
        }
    }

    public override void OnKill(int timeLeft)
    {
        for (int i = 1; i < 10; i += 1)
        {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(16f, 16f), DustID.Dirt, new Vector2?(Main.rand.NextVector2Circular(3f, 3f)), 0, default, 1f);
            dust.velocity.Y *= 0.2f;
            dust.velocity += Projectile.velocity * 0.2f;
        }
    }
}
