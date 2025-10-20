namespace ITD.Content.Projectiles.Other;

public class CacophanyBulletCasing : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 46;
        Projectile.height = 18;
        Projectile.hostile = false;
        Projectile.friendly = false;
        Projectile.ignoreWater = true;
        Projectile.aiStyle = 1;
        Projectile.scale = 0.5f;

        Projectile.penetrate = -1;
        Projectile.tileCollide = true;

        Projectile.timeLeft = 90;
        Projectile.extraUpdates = 1;
    }

    public bool runOnce = true;
    public float rotationSpeed;

    public override void AI()
    {
        Player player = Main.player[Projectile.owner];
        if (runOnce)
        {
            rotationSpeed = player.direction * Main.rand.Next(0, 241);
            runOnce = false;
        }

        Projectile.rotation += MathHelper.ToRadians(rotationSpeed);
        if (Projectile.timeLeft <= 20)
        {
            Projectile.alpha = (int)(255f - Projectile.timeLeft / 20f * 255f);
        }
    }
    public override bool OnTileCollide(Vector2 velocityChange)
    {
        Projectile.velocity.X /= 2;
        return false;
    }
}