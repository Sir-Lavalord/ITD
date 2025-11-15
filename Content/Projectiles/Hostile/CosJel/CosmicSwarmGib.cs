using ITD.Content.Dusts;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace ITD.Content.Projectiles.Hostile.CosJel;

public class CosmicSwarmGib : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 14; Projectile.height = 28;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 400;
        Projectile.light = 0.5f;
        Projectile.ignoreWater = false;
        Projectile.tileCollide = false;
    }
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5; // The length of old position to be recorded
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2; // The recording mode
        Main.projFrames[Projectile.type] = 1;
    }
    public override Color? GetAlpha(Color lightColor)
    {
        return Color.White;
    }
    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        return false;
    }
    public override void OnSpawn(IEntitySource source)
    {
    }
    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < 10; i++)
        {
            Dust.NewDust(Projectile.Center, 8, 8, DustID.PortalBolt, Projectile.velocity.X, Projectile.velocity.Y, 0, Color.White, 1);
        }
        SoundEngine.PlaySound(SoundID.Item27, Projectile.position);
    }

    public override void AI()
    {
        if (Main.rand.NextBool(2))
        {
            for (int i = 0; i < 1; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.OrangeTorch, 0f, 0f, 0, default(Color), 2f);
                Main.dust[d].noGravity = true;
                Main.dust[d].noLight = true;
                Main.dust[d].velocity *= 4f;
            }
        }
        Projectile.rotation = Projectile.velocity.ToRotation();
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = TextureAssets.Projectile[Type].Value;
        Texture2D effectTexture = TextureAssets.Extra[ExtrasID.StardustJellyfishSmall].Value;
        Vector2 effectOrigin = effectTexture.Size() / 2f;
        Rectangle frame = tex.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
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
        Main.EntitySpriteDraw(effectTexture, miragePos - new Vector2 (24,0).RotatedBy(Projectile.rotation), null, new Color(90, 70, 200, 50) * Projectile.Opacity, Projectile.rotation + MathHelper.PiOver2, effectOrigin, Projectile.scale * 1.25f, SpriteEffects.None, 0);

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

            Main.EntitySpriteDraw(tex, miragePos + new Vector2(0f, 6).RotatedBy(radians) * time, frame, new Color(255, 114, 70, 50) * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
        }

        for (float i = 0f; i < 1f; i += 0.5f)
        {
            float radians = (i + timer) * MathHelper.TwoPi;

            Main.EntitySpriteDraw(tex, miragePos + new Vector2(0f, 8).RotatedBy(radians) * time, frame, new Color(255, 114, 70, 50) * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
        }

        Main.EntitySpriteDraw(tex, miragePos, frame, Color.White * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
        return false;
    }
}