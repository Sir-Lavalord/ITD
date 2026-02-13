using ITD.Particles;
using ITD.Particles.Projectiles;
using ITD.Systems;
using ITD.Utilities;
using System;
using Terraria.GameContent;

namespace ITD.Content.Projectiles.Hostile.MotherWisp;

public class WispFireRain : ModProjectile
{
/*    public override string Texture => "ITD/Particles/Textures/WispFlame";*/
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
                Projectile.hide = true;
                if (Main.rand.NextBool(5 - (Projectile.timeLeft / 30)))
                    emitter?.Emit(Projectile.Center + Main.rand.NextVector2Square(-Projectile.width/4, Projectile.width / 4), Projectile.velocity * 0.2f, Projectile.velocity.ToRotation() - MathHelper.PiOver2, 20);
                break;
            case 1:
                if (rayPosY - Projectile.Center.Y > 20)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.UltraBrightTorch, 0, 0, 120, Color.Turquoise, Main.rand.NextFloat(0.5f, 1.1f));
                        dust.noGravity = true;
                        dust.velocity = Vector2.Zero;
                    }
                    Projectile.tileCollide = false;
                }
                else
                {
                    Projectile.hide = true;
                    Projectile.velocity *= 0.9f;
                    Projectile.tileCollide = true;
                    for (int i = 0; i < 2; i++)
                    {
                        Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.UltraBrightTorch, 0, 0, 120, Color.Turquoise, Main.rand.NextFloat(0.5f, 1.1f));
                        dust.noGravity = true;
                        dust.velocity = Vector2.Zero;
                    }
                    if (Main.rand.NextBool(1))
                        emitter?.Emit(Projectile.Center + new Vector2(Main.rand.Next(-Projectile.width / 2, Projectile.width / 2), Main.rand.Next(-2, 2)), Projectile.velocity * 0.2f, 0, 20);
                }
                break;
        }
    }
    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        for (int i = 0; i < 4; i++)
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.UltraBrightTorch, 0, 0, 120, Color.Turquoise, Main.rand.NextFloat(0.9f, 1.1f));
            dust.noGravity = false;
            dust.velocity = -Projectile.oldVelocity.RotatedByRandom(MathHelper.ToRadians(30)) / 5;
        }
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

    public override bool PreDraw(ref Color lightColor)
    {
        Vector2 stretch = new(1f, 1f);
        Texture2D tex = TextureAssets.Projectile[Type].Value;
        stretch = new Vector2(1f + Projectile.velocity.Length() * 0.05f, 1f);

        float rotation = Projectile.rotation;
        Vector2 offset = Vector2.UnitY * -10;
        Vector2 drawPos = Projectile.Center + offset;

        int sizeY = tex.Height / Main.projFrames[Type];
        int frameY = Projectile.frame * sizeY;
        Rectangle rectangle = new(0, frameY, tex.Width, sizeY);
        Vector2 origin = rectangle.Size() / 2f;
        SpriteEffects spriteEffects = Projectile.velocity.X >= 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        Color glowColor = new Color(36, 12, 34, 50) * 0.5f;

        for (float i = 0; i < ProjectileID.Sets.TrailCacheLength[Type]; i += 0.15f)
        {
            Color color27 = glowColor * 0.35f;
            Color color28 = glowColor;
            color27 *= (float)(ProjectileID.Sets.TrailCacheLength[Type] - i) / ProjectileID.Sets.TrailCacheLength[Type] * 0.75f;
            color28 *= (float)(ProjectileID.Sets.TrailCacheLength[Type] - i) / ProjectileID.Sets.TrailCacheLength[Type] * 2f;

            float scale = Projectile.scale;
            scale *= (float)(ProjectileID.Sets.TrailCacheLength[Type] - i) / ProjectileID.Sets.TrailCacheLength[Type];
            int max0 = (int)i - 1;//Math.Max((int)i - 1, 0);
            if (max0 < 0)
                continue;
            Vector2 value4 = Projectile.oldPos[max0];
            float num165 = Projectile.rotation; //NPC.oldRot[max0];
            Vector2 center2 = Vector2.Lerp(Projectile.oldPos[(int)i], Projectile.oldPos[max0], 1 - i % 1);
            center2 += Projectile.Size / 2;
            Main.EntitySpriteDraw(tex, center2 - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), rectangle, new Color(65, 80, 128, 50),
num165, origin, stretch * scale, spriteEffects, 0);
        }

        float time = Main.GlobalTimeWrappedHourly;
        float timer = (float)Main.time / 240f + time * 0.04f;

        time %= 4f;
        time /= 2f;

        if (time >= 1f)
        {
            time = 2f - time;
        }

        time = time * 0.5f + 0.5f;

        for (float i = 0f; i < 1f; i += 0.25f)
        {
            float radians = (i + timer) * MathHelper.TwoPi;

            Main.EntitySpriteDraw(tex, drawPos - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY) + new Vector2(0f, 4f).RotatedBy(radians) * time, rectangle, new Color(255, 255, 255, 50), Projectile.rotation, origin, stretch, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
        }

        for (float i = 0f; i < 1f; i += 0.34f)
        {
            float radians = (i + timer) * MathHelper.TwoPi;

            Main.EntitySpriteDraw(tex, drawPos - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY) + new Vector2(0f, 6f).RotatedBy(radians) * time, rectangle, new Color(255, 255, 255, 50), Projectile.rotation, origin, stretch, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
        }
        Main.EntitySpriteDraw(tex, drawPos - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), rectangle, Projectile.GetAlpha(Color.White),
                rotation, origin, stretch, spriteEffects, 0);

        return false;
    }
}