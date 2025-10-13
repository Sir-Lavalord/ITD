namespace ITD.Content.Projectiles.Hostile;

public class IncendipedeFireSpike : ModProjectile
{
    public override string Texture => ITD.BlankTexture;
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 6;
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 30;
    }
    public override void AI()
    {
        if (Projectile.ai[0] == 0)
            Projectile.ai[0] = Main.rand.NextFromList(-1, 1);
        Projectile.velocity = Projectile.velocity.RotatedBy(Projectile.ai[0] / 16f);
        Dust d = Dust.NewDustPerfect(Projectile.position, DustID.Torch, Scale: 2f);
        d.noGravity = true;
        d.velocity = Vector2.Zero;
    }
    public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
    {
        target.AddBuff(BuffID.OnFire, 60, false);
    }
}
