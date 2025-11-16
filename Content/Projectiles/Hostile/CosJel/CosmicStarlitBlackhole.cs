using ITD.Utilities;
using System;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;

namespace ITD.Content.Projectiles.Hostile.CosJel;

public class CosmicStarlitBlackhole : ModProjectile
{
    public override string Texture => ITD.BlankTexture;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
    }
    public override void SetDefaults()
    {
        Projectile.width = 40;
        Projectile.height = 40;
        Projectile.aiStyle = 0;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.ignoreWater = true;
        Projectile.light = 1f;
        Projectile.tileCollide = true;
        Projectile.timeLeft = 1200;
        Projectile.penetrate = -1;
    }
    public override void AI()
    {
        Projectile.rotation += 0.05f;
        if (Projectile.timeLeft > 30)
        {
            Projectile.Resize(300, 300);
            Projectile.scale = MathHelper.Clamp(Projectile.scale + 0.2f, 0, 4);
        }
        else
            Projectile.scale = MathHelper.Clamp(Projectile.scale - 0.2f, 0, 4);
        if (Main.essScale >= 1)
        {
            for (int i = 0; i < 20; i++)
            {
                int dust = Dust.NewDust(Projectile.Center, 1, 1, DustID.UltraBrightTorch, 0, 0, 0, default, 1f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity = Vector2.UnitX.RotatedByRandom(Math.PI) * Main.rand.NextFloat(0.9f, 1.1f) * 10;
            }
        }
    }

    public override void OnSpawn(IEntitySource source)
    {
        Projectile.scale = 0;
    }
    public override Color? GetAlpha(Color lightColor)
    {
        return Color.White;
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
    public override bool PreDraw(ref Color lightColor)
    {
        Vector2 position = Projectile.Center - Main.screenPosition;
        Texture2D texture2 = Mod.Assets.Request<Texture2D>("Content/Projectiles/Friendly/Melee/WRipperRift").Value;
        Rectangle sourceRectangle = texture2.Frame(1, 1);
        Vector2 origin = sourceRectangle.Size() / 2f;
        float time = Main.GlobalTimeWrappedHourly;
        float timer = (float)Main.time / 240f + time * 0.04f;

        time %= 4f;
        time /= 2f;

        if (time >= 1f)
        {
            time = 2f - time;
        }

        time = time * 0.5f + 0.5f;

        for (float i = 0f; i < 1f; i += 0.35f)
        {
            float radians = (i + timer) * MathHelper.TwoPi;

            Main.EntitySpriteDraw(texture2, position + new Vector2(0f, 6).RotatedBy(radians) * time, null, new Color(255, 114, 70, 10) * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale * Main.essScale, SpriteEffects.None, 0);
        }

        float colorScale = MathHelper.Min(Projectile.scale, 1);
        Main.EntitySpriteDraw(texture2, position, sourceRectangle, new Color(150, 70, 255, 30) * colorScale, Projectile.rotation + MathHelper.PiOver4, origin, Projectile.scale * Main.essScale, SpriteEffects.None, 0f);
        Main.EntitySpriteDraw(texture2, position, sourceRectangle, new Color(90, 70, 255, 30) * colorScale, -Projectile.rotation - MathHelper.PiOver4, origin, Projectile.scale, SpriteEffects.None, 0f);


    
        return false;
    }
}
