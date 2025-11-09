using ITD.Content.Dusts;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace ITD.Content.Projectiles.Hostile.CosJel;

public class CosmicLightningOrb: ModProjectile
{
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5; // The length of old position to be recorded
        ProjectileID.Sets.TrailingMode[Projectile.type] = 0; // The recording mode
        Main.projFrames[Projectile.type] = 4;
    }

    readonly int defaultWidthHeight = 96;
    public override void SetDefaults()
    {
        Projectile.width = defaultWidthHeight; Projectile.height = defaultWidthHeight;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 1200;
        Projectile.light = 0.5f;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        DrawOffsetX = -16;
        DrawOriginOffsetY = -16;
    }
    public override Color? GetAlpha(Color lightColor)
    {
        return Color.White * (1f - Projectile.alpha / 255f);
    }
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {

        return base.Colliding(projHitbox, targetHitbox);
    }
    public override void OnSpawn(IEntitySource source)
    {
        for (int i = 0; i < 20; i++)
        {
            int dust = Dust.NewDust(Projectile.Center, 1, 1, DustID.UltraBrightTorch, 0, 0, 0, default, 1f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity = Vector2.UnitX.RotatedByRandom(Math.PI) * Main.rand.NextFloat(0.9f, 1.1f) * 10;
        }
    }
    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < 20; i++)
        {
            int dust = Dust.NewDust(Projectile.Center, 1, 1, DustID.UltraBrightTorch, 0, 0, 0, default, 1f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity = Vector2.UnitX.RotatedByRandom(Math.PI) * Main.rand.NextFloat(0.9f, 1.1f) * 10;
        }
    }
    float spawnGlow = 1;
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = TextureAssets.Projectile[Type].Value;
        Rectangle frame = tex.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
        Texture2D tex2 = Mod.Assets.Request<Texture2D>("Content/Projectiles/Friendly/Mage/TwilightDemiseHorribleThing").Value;
        Rectangle frame2 = tex2.Frame(1, 1, 0, 0);
        Vector2 center = Projectile.Size / 2f;
        for (int i = Projectile.oldPos.Length - 1; i > 0; i--)
        {
            Projectile.oldRot[i] = Projectile.oldRot[i - 1];
            Projectile.oldRot[i] = Projectile.rotation + MathHelper.PiOver2;

        }
        Vector2 miragePos = Projectile.position - Main.screenPosition + center;
        Vector2 origin = new(tex.Width * 0.5f, tex.Height / Main.projFrames[Type] * 0.5f);
        float time = Main.GlobalTimeWrappedHourly;
        float timer = (float)Main.time / 240f + time * 0.04f;

        time %= 4f;
        time /= 2f;
        if (time >= 1f)
        {
            time = 2f - time;
        }

        time = time + 0.5f;

        for (float i = 0f; i < 1f; i += 0.35f)
        {
            float radians = (i + timer) * MathHelper.TwoPi;

            Main.EntitySpriteDraw(tex, miragePos + new Vector2(0f, 6).RotatedBy(radians) * time, frame, new Color(90, 70, 255, 50) * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
        }

        for (float i = 0f; i < 1f; i += 0.5f)
        {
            float radians = (i + timer) * MathHelper.TwoPi;

            Main.EntitySpriteDraw(tex, miragePos + new Vector2(0f, 8).RotatedBy(radians) * time, frame, new Color(90, 70, 255, 50) * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
        }
        if (spawnGlow > 0)//fargo eridanus epic
        {
            float scale = 3f * Projectile.scale * (float)Math.Cos(Math.PI / 2 * spawnGlow);
            float opacity = Projectile.Opacity * (float)Math.Sqrt(spawnGlow);
            Main.EntitySpriteDraw(tex2, Projectile.Center - Main.screenPosition, frame2, new Color(150, 251, 255, 200) * scale,
    Projectile.rotation, new Vector2(tex2.Width * 0.5f, tex2.Height * 0.5f), Projectile.scale * 0.75f * scale, SpriteEffects.None, 0f);
        }
        Main.EntitySpriteDraw(tex, miragePos, frame, Color.White * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
        Main.EntitySpriteDraw(tex2, Projectile.Center - Main.screenPosition, frame2, new Color(150, 251, 255, 150) * Main.essScale,
Projectile.rotation, new Vector2(tex2.Width * 0.5f, tex2.Height * 0.5f),  Projectile.scale * 1.75f * Main.essScale, SpriteEffects.None, 0f);

        return false;
    }
    readonly bool expertMode = Main.expertMode;
    readonly bool masterMode = Main.masterMode;
    public override void AI()
    {
        if (Main.essScale >= 1)
        {
            for (int i = 0; i < 10; i++)
            {
                int dust = Dust.NewDust(Projectile.Center, 1, 1, DustID.UltraBrightTorch, 0, 0, 0, default, 1f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity = Vector2.UnitX.RotatedByRandom(Math.PI) * Main.rand.NextFloat(0.9f, 1.1f) * 10;
            }
        }
        if (++Projectile.frameCounter >= 10)
        {
            Projectile.frameCounter = 0;
            Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
        }
        spawnGlow -= 0.05f;
    }
}
