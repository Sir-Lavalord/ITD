using Terraria.DataStructures;
namespace ITD.Content.Projectiles.Hostile.CosJel;

public class CosmicSwarm : ModProjectile
{
    public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = 5;
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }

    public override void SetDefaults()
    {
        Projectile.width = 76;
        Projectile.height = 54;
        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hostile = true;
        CooldownSlot = 1;
        Projectile.scale = 1.5f;
        Projectile.alpha = 50;
        Projectile.extraUpdates = 0;
        Projectile.timeLeft = 400;
    }
    readonly bool isStuck = false;
    public float spawnRot;
    public override void AI()
    {
        if (++Projectile.frameCounter >= 6)
        {
            Projectile.frameCounter = 0;
            Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
        }
        Projectile.rotation = Projectile.velocity.ToRotation();
    }
    public override void OnSpawn(IEntitySource source)
    {
        for (int i = 0; i < 12; i++)
        {
            Dust dust = Dust.NewDustDirect(Projectile.Center, Projectile.width, Projectile.height, DustID.ShimmerTorch, 0, 0f, 40, default, Main.rand.NextFloat(2f, 3f));
            dust.velocity *= 2f;
            Dust dust2 = Dust.NewDustDirect(Projectile.Center, Projectile.width, Projectile.height, DustID.ShimmerTorch, 0, 0f, 40, default, Main.rand.NextFloat(2f, 3f));
            dust2.velocity *= 1f;
            dust2.noGravity = true;
        }
    }
    public override void OnKill(int timeleft)
    {

    }
    public override Color? GetAlpha(Color lightColor)
    {
        return Color.White * Projectile.Opacity;
    }
}