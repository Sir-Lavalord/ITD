using ITD.Content.Projectiles.Friendly.Misc;

namespace ITD.Content.Projectiles.Friendly.Ranger;

public class ResonanceBlast : BigBlankExplosion
{
    public override int Lifetime => 30;
    public override Vector2 ScaleRatio => new(1.5f, 1f);

    public override Color GetCurrentExplosionColor(float pulseCompletionRatio) => Color.Lerp(Color.White * 1.6f, Color.LightGray, MathHelper.Clamp(pulseCompletionRatio * 2.2f, 0f, 1f));

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 1000;
    }
    public override string Texture => ITD.BlankTexture;
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 2;
        Projectile.ignoreWater = true;
        Projectile.hostile = false;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = Lifetime;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.DamageType = DamageClass.Ranged;
    }
    public override void OnKill(int timeLeft)
    {
    }
    public override bool? CanDamage()
    {
        return CurrentRadius <= MaxRadius / 0.8f;
    }
    public override void AI()
    {
        base.AI();
    }
    public override Color? GetAlpha(Color lightColor)
    {
        return new Color(243, 162, 63);
    }
    public override void PostAI() => Lighting.AddLight(Projectile.Center, 0.2f, 0.1f, 0f);
}