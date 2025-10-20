namespace ITD.Content.Projectiles.Friendly.Melee.Snaptraps.Extra;

public class FrostgripIcicle : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.DamageType = DamageClass.Melee;
        Projectile.width = 12; Projectile.height = 12;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = 3;
        Projectile.timeLeft = 127;
        Projectile.ignoreWater = false;
        Projectile.tileCollide = true;
        Projectile.scale = 0.65f;
    }

    public override void AI()
    {
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        Projectile.velocity.Y = Projectile.velocity.Y + 0.25f;
        if (Projectile.velocity.Y > 16f)
        {
            Projectile.velocity.Y = 16f;
        }
    }

    public override void OnKill(int timeLeft)
    {
        for (int d = 0; d < 10; d++)
        {
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.SnowflakeIce, 0f, 0f, 150, default, 0.75f);
        }
    }
}
