using Daybreak.Common.Rendering;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;

namespace ITD.Content.Projectiles.Friendly.Melee;

public class DespoticSuperSpecialProj : ModProjectile
{
    public override string Texture => "ITD/Content/Items/Weapons/Melee/DespoticSuperMeleeSword";

    public MiscShaderData Shader = new MiscShaderData(Main.VertexPixelShaderRef, "MagicMissile")
        .UseProjectionMatrix(true)
        .UseImage0("Images/Extra_" + 192)
        .UseImage1("Images/Extra_" + 194)
        .UseImage2("Images/Extra_" + 193)
        .UseSaturation(-2.8f)
        .UseOpacity(2f);
    public VertexStrip TrailStrip = new();

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }

    public override void SetDefaults()
    {
        Projectile.DamageType = DamageClass.Melee;
        Projectile.width = Projectile.height = 32;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 30;
        Projectile.ignoreWater = false;
        Projectile.tileCollide = false;
        Projectile.scale = 1.75f;
    }

    public override void AI()
    {
        if (Projectile.timeLeft > 10)
        {
            int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.DungeonSpirit, 0, 0, 100, default, 1f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity = Projectile.velocity / 5f;
        }
        else
        {
            if (Projectile.timeLeft == 10)
                Projectile.velocity *= 0.2f;
            Projectile.Opacity -= 0.1f;
        }

        Projectile.rotation = Projectile.velocity.ToRotation();
    }
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        float num32 = 0f;
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + Projectile.velocity.SafeNormalize(-Vector2.UnitY) * 90f * Projectile.scale, 32f * Projectile.scale, ref num32);
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        Main.LocalPlayer.ITD().BetterScreenshake(4, 4, 4, false);
        SoundEngine.PlaySound(SoundID.Item94, target.Center);
        for (int j = 0; j < 8; ++j)
        {
            int dust = Dust.NewDust(target.Center, 0, 0, DustID.DungeonSpirit, 0f, 0f, 100, default, 1.5f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity *= 3f;
        }
    }

    private Color StripColors(float progressOnStrip)
    {
        Color result = Color.Lerp(Color.Cyan, Color.Black, Utils.GetLerpValue(0f, 0.7f, progressOnStrip, true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip, false));
        return result * Projectile.Opacity * Projectile.Opacity;
    }
    private float StripWidth(float progressOnStrip)
    {
        return 96f;
    }
    public override bool PreDraw(ref Color lightColor)
    {
        string path = "ITD/Content/Projectiles/Friendly/Melee/";
        Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
        Texture2D glow = ModContent.Request<Texture2D>(path + "DespoticSword_Glow").Value;

        Shader.Apply(null);
        TrailStrip.PrepareStrip(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, Projectile.Size * 0.5f - Main.screenPosition, Projectile.oldPos.Length, true);
        TrailStrip.DrawTrail();

        Main.spriteBatch.End(out SpriteBatchSnapshot spriteBatchData); // unapply shaders
        Main.spriteBatch.Begin(spriteBatchData);

        Vector2 drawPos = Projectile.Center - Main.screenPosition;
        Main.EntitySpriteDraw(tex, drawPos, null, lightColor * Projectile.Opacity, Projectile.rotation + MathHelper.PiOver4, tex.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
        Main.EntitySpriteDraw(glow, drawPos, null, Color.White * Projectile.Opacity, Projectile.rotation + MathHelper.PiOver4, glow.Size() * 0.5f, Projectile.scale, SpriteEffects.None);

        return false;
    }
}
