using ITD.Particles;
using ITD.Particles.Projectiles;
using ITD.Systems;
using ITD.Utilities;
using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Hostile.MotherWisp;

public class WispFireBreath : ModProjectile
{
    public override string Texture => ITD.BlankTexture;
    public ParticleEmitter emitter;

    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 1;
        ProjectileID.Sets.TrailCacheLength[Type] = 30;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults()
    {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.aiStyle = -1;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 180;
        Projectile.hide = true;

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

    public override void AI()
    {
        if (emitter != null)
            emitter.keptAlive = true;

        if (Main.rand.NextBool(2))
            emitter?.Emit(Projectile.Center + Main.rand.NextVector2Square(-Projectile.width / 4, Projectile.width / 4), Projectile.velocity * 0.2f, Projectile.velocity.ToRotation() - MathHelper.PiOver2, 20);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        return false;
    }
}