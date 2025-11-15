using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
namespace ITD.Content.Projectiles.Hostile.CosJel;

public class CosmicSwarm : ModProjectile
{
    public VertexStrip TrailStrip = new();

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 40;
        Main.projFrames[Projectile.type] = 5;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }

    public override void SetDefaults()
    {
        Projectile.width = 76;
        Projectile.height = 54;
        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hostile = true;
        CooldownSlot = 1;
        Projectile.scale = 1.5f;
        Projectile.alpha = 50;
        Projectile.extraUpdates = 0;
        Projectile.timeLeft = 400;
    }
    public float spawnRot;
    public override void AI()
    {
        if (++Projectile.frameCounter >= 6)
        {
            Projectile.frameCounter = 0;
            Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
        }
        Projectile.rotation = Projectile.velocity.ToRotation();
    }
    public override void OnSpawn(IEntitySource source)
    {
        for (int i = 0; i < 12; i++)
        {
            Dust dust = Dust.NewDustDirect(Projectile.Center, Projectile.width, Projectile.height, DustID.ShimmerTorch, 0, 0f, 40, default, Main.rand.NextFloat(2f, 3f));
            dust.velocity *= 2f;
            Dust dust2 = Dust.NewDustDirect(Projectile.Center, Projectile.width, Projectile.height, DustID.ShimmerTorch, 0, 0f, 40, default, Main.rand.NextFloat(2f, 3f));
            dust2.velocity *= 1f;
            dust2.noGravity = true;
        }
    }
    private Color StripColors(float progressOnStrip)
    {
        return new Color(90, 70, 255, 10);
    }
    private float StripWidth(float progressOnStrip)
    {
        return MathHelper.Lerp(20f, 2f, Utils.GetLerpValue(0f, 0.6f, progressOnStrip, true)) * Utils.GetLerpValue(0f, 0.07f, progressOnStrip, true);
    }
    public override void OnKill(int timeleft)
    {

    }
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = TextureAssets.Projectile[Type].Value;
        Rectangle frame = tex.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
        Vector2 center = Projectile.Size / 2f;
        for (int i = Projectile.oldPos.Length - 1; i > 0; i--)
        {
            Projectile.oldRot[i] = Projectile.oldRot[i - 1];
            Projectile.oldRot[i] = Projectile.rotation;

        }
        GameShaders.Misc["LightDisc"].Apply(null);
        TrailStrip.PrepareStrip(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, Projectile.Size * 0.5f - Main.screenPosition, Projectile.oldPos.Length, true);
        TrailStrip.DrawTrail();

        Main.pixelShader.CurrentTechnique.Passes[0].Apply();
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

        time = time * 0.5f + 0.5f;

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

        Main.EntitySpriteDraw(tex, miragePos, frame, Color.White * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
        return false;
    }
    public override Color? GetAlpha(Color lightColor)
    {
        return Color.White * Projectile.Opacity;
    }
}