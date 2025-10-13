using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;

namespace ITD.Content.Projectiles.Hostile.CosJel;

public class CosmicGlowStar : ModProjectile
{
    public MiscShaderData Shader = new MiscShaderData(Main.VertexPixelShaderRef, "MagicMissile")
        .UseProjectionMatrix(true)
        .UseImage0("Images/Extra_" + 192)
        .UseImage1("Images/Extra_" + 194)
        .UseImage2("Images/Extra_" + 190)
        .UseSaturation(-4f)
        .UseOpacity(2f);

    public VertexStrip TrailStrip = new();
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
        Projectile.tileCollide = false;
        Projectile.timeLeft = 240;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }
    public float spawnTime;
    public Player player => Main.player[(int)Projectile.ai[0]];
    public override void AI()
    {
        if (Projectile.localAI[0] != 0)
        {
            Projectile.scale = Projectile.localAI[0];
        }
        if (Projectile.ai[2]++ >= spawnTime && Projectile.ai[2] < spawnTime * 1.5f)
        {
            Projectile.velocity *= 0.9f;
        }
        if (Projectile.ai[2] == spawnTime * 1.5f)
        {
            if (Projectile.localAI[0] != 0)
            {
                Projectile.velocity = Vector2.Normalize(player.Center - Projectile.Center) * 24;
            }
            else Projectile.velocity = Vector2.Normalize(player.Center - Projectile.Center) * 18;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.netUpdate = true;
        }
    }
    public override bool? CanDamage()
    {
        return Projectile.ai[2] >= spawnTime * 1.5f;
    }

    public override void OnSpawn(IEntitySource source)
    {
        spawnTime = Projectile.ai[1];
        if (Projectile.localAI[0] != 0)
        {
            Projectile.scale = Projectile.localAI[0];
        }
    }
    public override Color? GetAlpha(Color lightColor)
    {
        return Color.White;
    }
    private Color StripColors(float progressOnStrip)
    {

        Color result = Color.Lerp(Color.White, new Color(90, 70, 255, 50), Utils.GetLerpValue(0f, 0.7f, progressOnStrip, true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip, false));
        result.A /= 2;
        return result * Projectile.Opacity;
    }
    private float StripWidth(float progressOnStrip)
    {
        return Projectile.localAI[0] != 0 ? 60f : 30f;
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Vector2 stretch = new(1f, 1f);
        Texture2D tex = TextureAssets.Projectile[Type].Value;
        Rectangle frame = tex.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
        Vector2 center = Projectile.Size / 2f;
        for (int i = Projectile.oldPos.Length - 1; i > 0; i--)
        {
            Projectile.oldRot[i] = Projectile.oldRot[i - 1];
            Projectile.oldRot[i] = Projectile.rotation + MathHelper.PiOver2;

        }
        Vector2 miragePos = Projectile.position - Main.screenPosition + center;
        Vector2 origin = new(tex.Width * 0.5f, tex.Height / Main.projFrames[Type] * 0.5f);
        if (Projectile.ai[2] >= spawnTime * 1.5f)
        {
            Shader.Apply(null);
            TrailStrip.PrepareStrip(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, Projectile.Size * 0.5f - Main.screenPosition, Projectile.oldPos.Length, true);
            TrailStrip.DrawTrail();
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
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

        for (float i = 0f; i < 1f; i += 0.35f)
        {
            float radians = (i + timer) * MathHelper.TwoPi;

            Main.EntitySpriteDraw(tex, miragePos + new Vector2(0f, 4).RotatedBy(radians) * time, frame, new Color(90, 70, 255, 50) * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
        }

        for (float i = 0f; i < 1f; i += 0.5f)
        {
            float radians = (i + timer) * MathHelper.TwoPi;

            Main.EntitySpriteDraw(tex, miragePos + new Vector2(0f, 6).RotatedBy(radians) * time, frame, new Color(90, 70, 255, 50) * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
        }

        Main.EntitySpriteDraw(tex, miragePos, frame, Color.White * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
        return false;
    }
}
