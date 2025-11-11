namespace ITD.Content.Projectiles.Friendly.Melee;

public class BigBerthExplosion : ModProjectile
{
    public override string Texture => ITD.BlankTexture;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.Explosive[Projectile.type] = true;
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
    }
    //should have made explosion a partial class
    public override void SetDefaults()
    {
        Projectile.width = 100;
        Projectile.height = 100;
        Projectile.aiStyle = 0;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Melee;
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
        if (target.Gimmickable())
        {
            float kbIntensity = 1 - ((Vector2.Distance(Projectile.Center, target.Center))/(Projectile.width/2));
            target.velocity.Y = -16f * kbIntensity;
        }
        modifiers.HitDirectionOverride = (Projectile.Center.X < target.Center.X).ToDirectionInt();
    }
}
