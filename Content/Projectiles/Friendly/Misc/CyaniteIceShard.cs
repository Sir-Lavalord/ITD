namespace ITD.Content.Projectiles.Friendly.Misc;

public class CyaniteIceShard : ModProjectile
{
    private static readonly int duration = 30;
    public override void SetDefaults()
    {
        Projectile.damage = 44;
        Projectile.width = 44;
        Projectile.height = 12;
        Projectile.scale = 1.2f;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Generic;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.timeLeft = duration;
        Projectile.penetrate = -1;
        Projectile.stopsDealingDamageAfterPenetrateHits = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.direction = (int)Main.rand.NextFloat(0, 360);
        Projectile.timeLeft = 300;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        target.AddBuff(BuffID.Frostburn2, 600);
    }
}
