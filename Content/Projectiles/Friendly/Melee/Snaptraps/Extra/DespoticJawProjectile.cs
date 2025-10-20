namespace ITD.Content.Projectiles.Friendly.Melee.Snaptraps.Extra;

public class DespoticJawProjectile : ModProjectile
{
    private int timeLeftMax;
    bool hasntChomped = true;
    public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = 5;
    }

    public override void SetDefaults()
    {
        Projectile.DamageType = DamageClass.Melee;
        Projectile.width = 80; Projectile.height = 80;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 120;
        timeLeftMax = Projectile.timeLeft;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
    }

    public override Color? GetAlpha(Color lightColor)
    {
        float progressOneToZero = Projectile.timeLeft / (float)timeLeftMax;
        return Color.White * progressOneToZero;
    }
    public override void AI()
    {
        if (Projectile.frame < Main.projFrames[Projectile.type] - 1)
        {
            if (++Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
            }
        }
        if (Projectile.frame == Main.projFrames[Projectile.type] - 1 && hasntChomped)
        {
            hasntChomped = false;
            Projectile.damage = 4500;
        }
        else
        {
            Projectile.damage = 0;
        }
    }
}
