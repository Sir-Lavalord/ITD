using Terraria.Audio;
using Terraria.GameContent;

namespace ITD.Content.Projectiles.Hostile;

public class IcyBoulder : ModProjectile
{
    public static float IcyBoulderGravity = 0.2f;
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }
    public override void SetDefaults()
    {
        Projectile.width = 64;
        Projectile.height = 64;
        Projectile.penetrate = -1;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.timeLeft = 300;
        DrawOffsetX = -6;
    }
    public override void AI()
    {
        Projectile.velocity.Y += IcyBoulderGravity;
        Projectile.rotation += Projectile.velocity.X < 0f ? -0.2f : 0.2f;
    }
    public override void OnKill(int timeLeft)
    {
        if (Main.dedServ)
            return;
        SoundEngine.PlaySound(SoundID.Item50, Projectile.Center);
        SoundEngine.PlaySound(SoundID.Item89, Projectile.Center);
        int gore0 = Mod.Find<ModGore>("IcyBoulderGore0").Type;
        int gore1 = Mod.Find<ModGore>("IcyBoulderGore1").Type;
        int gore2 = Mod.Find<ModGore>("IcyBoulderGore2").Type;
        for (int i = 0; i < 10; i++)
        {
            Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Ice);
            d.scale *= 1.5f;
        }
        for (int i = 0; i < 3; i++)
        {
            Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, new Vector2(-2f, -2f), gore0);
            Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, new Vector2(0f, -2f), gore1);
            Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, new Vector2(2f, -2f), gore2);
        }
        Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Vector2 drawOrigin = texture.Size() / 2f;
        for (int k = 0; k < Projectile.oldPos.Length; k++)
        {
            Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY + DrawOriginOffsetX) + new Vector2(DrawOffsetX, DrawOriginOffsetY);
            Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
            Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.oldRot[k], drawOrigin, Projectile.scale, SpriteEffects.None, 0);
        }

        return true;
    }
}
