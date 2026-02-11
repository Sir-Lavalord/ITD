using ITD.Particles;
using ITD.Particles.Projectiles;
using ITD.Systems;
using ITD.Utilities;
using System;

namespace ITD.Content.Projectiles.Hostile.MotherWisp;

public class WispFireRain : ModProjectile
{
    public override string Texture => ITD.BlankTexture;
    public ParticleEmitter emitter;

    public override void SetDefaults()
    {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.aiStyle = 0;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 120;
        emitter = ParticleSystem.NewEmitter<WispFlame>(ParticleEmitterDrawCanvas.WorldOverProjectiles);
        emitter.tag = Projectile;
    }

    public override bool? CanDamage()
    {
        return true;
    }

    public override Color? GetAlpha(Color lightColor)
    {
        return new Color(255, 255, 255);
    }
    private float rayPosY//be accurate
    {
        get => Projectile.ai[1];
        set => Projectile.ai[1] = value;
    }
    public override void AI()
    {
        if (emitter != null)
            emitter.keptAlive = true;
        switch ((int)(Projectile.ai[0]))
        {
            case 0:
                if (Main.rand.NextBool(5 - (Projectile.timeLeft / 30)))
                    emitter?.Emit(Projectile.Center + Main.rand.NextVector2Square(-8f, 8f), Projectile.velocity * 0.2f, Projectile.velocity.ToRotation() - MathHelper.PiOver2, 20);
                break;
            case 1:
                if (rayPosY - Projectile.Center.Y > 20)
                {
                    if (Main.rand.NextBool(2))
                        emitter?.Emit(Projectile.Center, Projectile.velocity * 0.2f, Projectile.velocity.ToRotation() - MathHelper.PiOver2, 20);
                    Projectile.tileCollide = false;
                }
                else
                {
                    Projectile.velocity *= 0;
                    Projectile.tileCollide = true;
                    if (Main.rand.NextBool(4))
                        emitter?.Emit(Projectile.Center, Projectile.velocity * 0.2f,0, 20);
                }
                break;
        }
    }
    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        return false;
    }
    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
    {
        if (rayPosY - Projectile.Center.Y > 20)
        {
            fallThrough = false;
        }
        else
        {
            fallThrough = true;
        }
        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }
}