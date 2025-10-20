using ITD.Content.Dusts;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;

namespace ITD.Content.Projectiles.Friendly.Mage;

public class FungalRodProj : ModProjectile
{
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }

    public override void SetDefaults()
    {
        Projectile.width = 22;
        Projectile.height = 28;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = 3;
        Projectile.timeLeft = 90;
        Projectile.extraUpdates = 0;
        Projectile.aiStyle = -1;
        Projectile.alpha = 160;
        Projectile.Opacity = 0.2f;
        Projectile.tileCollide = false;
        Projectile.stopsDealingDamageAfterPenetrateHits = true;
    }
    public override void OnSpawn(IEntitySource source)
    {
        Projectile.spriteDirection = -(int)Projectile.ai[1];
        trailCol = Color.White;
        trailCol2 = Color.LightSkyBlue;

    }

    public override void AI()
    {
        Projectile.rotation = Projectile.velocity.ToRotation();
        if (Projectile.ai[2]++ >= 20)
        {
            float curveStrength = Projectile.ai[0];
            if (curveStrength != 0)
            {
                float curveProgress = 1f - (Projectile.timeLeft / 120f);
                Projectile.velocity = Projectile.velocity.RotatedBy(curveStrength * curveProgress * 0.06f);
            }
            Projectile.extraUpdates = (int)MathHelper.Clamp(MathHelper.Lerp(0, 3, 1f - (Projectile.timeLeft / 120f)), 0, 2);
            if (Projectile.extraUpdates >= 1)
            {
                if (Projectile.alpha <= 60)
                {
                    trailCol = Color.LightSkyBlue;
                    trailCol2 = Color.DarkTurquoise;

                    Projectile.velocity *= 0.95f;

                }

                Projectile.Opacity += 0.01f;
                Projectile.alpha -= 3;
            }
        }
        else
        {
            Projectile.velocity *= 1.04f;
        }
    }
    public override void OnKill(int timeLeft)
    {
        SoundEngine.PlaySound(new SoundStyle("ITD/Content/Sounds/FungalRodPop"));
        for (int i = 0; i < 4; i++)
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<BlueshroomSporesDust>(), 0f, 0f, 0, default, Main.rand.NextFloat(0.8f, 1.1f));
            dust.noGravity = true;
        }
    }
    Color trailCol;
    Color trailCol2;

    public override Color? GetAlpha(Color lightColor)
    {
        Color color = new(255, 255, 255, 100);
        return color * Projectile.Opacity;
    }
    public VertexStrip TrailStrip = new();

    private Color StripColors(float progressOnStrip)
    {
        Color result = Color.Lerp(trailCol, trailCol2, Utils.GetLerpValue(0f, 0.7f, progressOnStrip, true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip, false));
        result.A /= 2;
        return result * 0.5f * Projectile.Opacity;
    }
    private float StripWidth(float progressOnStrip)
    {
        return MathHelper.Lerp(11f, 7f, Utils.GetLerpValue(0f, 0.2f, progressOnStrip, true)) * Utils.GetLerpValue(0f, 0.07f, progressOnStrip, true);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        GameShaders.Misc["LightDisc"].Apply(null);
        TrailStrip.PrepareStrip(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, Projectile.Size * 0.5f - Main.screenPosition, Projectile.oldPos.Length, true);
        TrailStrip.DrawTrail();

        Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        return true;
    }
}