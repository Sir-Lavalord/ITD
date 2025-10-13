namespace ITD.Content.Projectiles.Friendly.Misc;

public class BlankExplosion : ModProjectile
{
    public override string Texture => ITD.BlankTexture;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
    }
    //A default for all explosion that isn't the og proj
    //Multiply, divide to get the intended explosion
    public override void SetDefaults()
    {
        Projectile.width = 100;
        Projectile.height = 100;
        Projectile.aiStyle = 0;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Generic; //fargo's soul explosion note here, aka funny words
        Projectile.penetrate = -1;
        Projectile.timeLeft = 3;
        Projectile.tileCollide = false;
        Projectile.light = 0.75f;
        Projectile.ignoreWater = true;
        Projectile.extraUpdates = 1;
        AIType = ProjectileID.Bullet;
        Projectile.localNPCHitCooldown = -1;
        Projectile.usesLocalNPCImmunity = true;
    }
    public override void AI()
    {
    }
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        modifiers.HitDirectionOverride = (Projectile.Center.X < target.Center.X).ToDirectionInt();
    }
}
