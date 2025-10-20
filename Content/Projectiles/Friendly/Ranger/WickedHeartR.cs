using Terraria.Graphics;
using Terraria.Graphics.Shaders;

namespace ITD.Content.Projectiles.Friendly.Ranger;

public class WickedHeartR : ModProjectile
{
    public override string Texture => ITD.BlankTexture;

    public MiscShaderData Shader = new MiscShaderData(Main.VertexPixelShaderRef, "MagicMissile").UseProjectionMatrix(true);
    public VertexStrip TrailStrip = new();

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }

    public override void SetDefaults()
    {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.aiStyle = ProjAIStyleID.Arrow;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.penetrate = 2;

        Shader.UseImage0("Images/Extra_" + 191);
        Shader.UseImage1("Images/Extra_" + 194);
        Shader.UseImage2("Images/Extra_" + 190);
        Shader.UseSaturation(-2.8f);
        Shader.UseOpacity(2f);
    }

    public override void AI()
    {
        Projectile.velocity.Y = Projectile.velocity.Y + 0.1f;
        if (Projectile.velocity.Y > 16f)
        {
            Projectile.velocity.Y = 16f;
        }

        Projectile.rotation = Projectile.velocity.ToRotation();

    }

    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < 3; i++)
        {
            int dust = Dust.NewDust(Projectile.Center, 1, 1, DustID.Shadowflame, 0f, 0f, 0, default, 2.5f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity *= 2f;
        }
    }

    private Color StripColors(float progressOnStrip)
    {
        Color result = Color.Lerp(Color.White, new Color(255, 50, 0), Utils.GetLerpValue(0f, 0.7f, progressOnStrip, true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip, false));
        result.A /= 2;
        return result * Projectile.Opacity;
    }
    private float StripWidth(float progressOnStrip)
    {
        return 16f;
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Shader.Apply(null);
        TrailStrip.PrepareStrip(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, Projectile.Size * 0.5f - Main.screenPosition, Projectile.oldPos.Length, true);
        TrailStrip.DrawTrail();
        Main.pixelShader.CurrentTechnique.Passes[0].Apply();

        return false;
    }
}