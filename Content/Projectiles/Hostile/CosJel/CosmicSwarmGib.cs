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
        ProjectileID.Sets.TrailingMode[Projectile.type] = 0; // The recording mode
        Main.projFrames[Projectile.type] = 2;
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
        Projectile.frame = Main.rand.Next(0, 2);
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
        Projectile.rotation += 0.2f;
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Player player = Main.player[Projectile.owner];
        Texture2D effectTexture = TextureAssets.Extra[ExtrasID.SharpTears].Value;
        Vector2 effectOrigin = effectTexture.Size() / 2f;
        lightColor = Lighting.GetColor((int)player.Center.X / 16, (int)player.Center.Y / 16);

        Main.EntitySpriteDraw(effectTexture, Projectile.Center, new Rectangle?(Projectile.Hitbox), new Color(120, 184, 255, 50) * 0.05f * Projectile.timeLeft, Projectile.rotation, effectOrigin, Projectile.scale, SpriteEffects.None, 0f);
        return true;
    }
}