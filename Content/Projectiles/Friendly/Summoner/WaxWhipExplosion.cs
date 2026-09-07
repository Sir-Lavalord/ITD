using ITD.Particles;
using ITD.Particles.Projectiles;
using Terraria.DataStructures;

namespace ITD.Content.Projectiles.Friendly.Summoner;

public class WaxWhipExplosion : ModProjectile
{
    public override string Texture => ITD.BlankTexture;
    public ParticleEmitter emitter;
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.Explosive[Projectile.type] = true;
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
    }
    public override void SetDefaults()
    {
        Projectile.width = 64;
        Projectile.height = 64;
        Projectile.aiStyle = ProjAIStyleID.Explosive;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Summon;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 3;
        Projectile.tileCollide = false;
        Projectile.light = 0.75f;
        Projectile.ignoreWater = true;
        Projectile.extraUpdates = 1;
        Projectile.localNPCHitCooldown = -1;
        Projectile.usesLocalNPCImmunity = true;
        emitter = ParticleSystem.NewEmitter<WispFlame>(ParticleEmitterDrawCanvas.WorldUnderProjectiles);
        emitter.tag = Projectile;
    }
    public override void OnSpawn(IEntitySource source)
    {
        for (int i = 0; i < 20; i++)
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.WhiteTorch, 0, 0, 150, Color.PaleTurquoise, 1.5f);
            dust.noGravity = true;
            dust.velocity = (Vector2.UnitX * 10).RotatedByRandom(MathHelper.Pi) * Main.rand.NextFloat(0.4f, 1.1f);
        }
    }
    public override void AI()
    {
        if (emitter != null) 
            emitter.keptAlive = true;

    }
    public override void OnKill(int timeLeft)
    {
        int rand = Main.rand.Next(4, 9);
        for (int i = 0; i < rand; i++)
        {
            emitter?.Emit(Projectile.Center + Main.rand.NextVector2Circular(Projectile.width,Projectile.height),
                -Vector2.UnitY * Main.rand.NextFloat(0.5f, 1.1f),0,(short)Main.rand.Next(20,30));
        }
    }
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        modifiers.HitDirectionOverride = (Projectile.Center.X < target.Center.X).ToDirectionInt();
    }
}
