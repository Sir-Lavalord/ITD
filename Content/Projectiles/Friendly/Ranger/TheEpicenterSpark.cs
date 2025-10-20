using ITD.Particles;
using ITD.Particles.Projectiles;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;

namespace ITD.Content.Projectiles.Friendly.Ranger;

public class TheEpicenterSpark : ModProjectile
{
    public MiscShaderData Shader = new MiscShaderData(Main.VertexPixelShaderRef, "MagicMissile")
        .UseProjectionMatrix(true)
        .UseImage0("Images/Extra_" + 192)
        .UseImage1("Images/Extra_" + 194)
        .UseImage2("Images/Extra_" + 190)
        .UseSaturation(-4f)
        .UseOpacity(2f);

    public VertexStrip TrailStrip = new();
    public ParticleEmitter emitter;

    public override string Texture => ITD.BlankTexture;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }
    public override void SetDefaults()
    {
        Projectile.width = 20;
        Projectile.height = 20;
        Projectile.aiStyle = 0;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.ignoreWater = true;
        Projectile.light = 1f;
        Projectile.tileCollide = true;
        Projectile.timeLeft = 300;
        Projectile.penetrate = 1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
        emitter = ParticleSystem.NewEmitter<TheEpicenterFlash>(ParticleEmitterDrawCanvas.WorldOverProjectiles);
        emitter.tag = Projectile;
    }
    bool isBig;
    Color bigColor;
    public override void OnSpawn(IEntitySource source)
    {
        if (Projectile.ai[0] == 1)
        {
            isBig = true;
        }
    }
    public override void AI()
    {
        Projectile.spriteDirection = (Projectile.velocity.X < 0).ToDirectionInt();
        if (Projectile.spriteDirection == 1)
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2 * 2;
        else
            Projectile.rotation = Projectile.velocity.ToRotation();
        if (emitter != null)
            emitter.keptAlive = true;
    }
    public override void OnKill(int timeLeft)
    {
        emitter?.Emit(Projectile.Center, new Vector2(), Projectile.oldVelocity.ToRotation() + MathHelper.PiOver2, 20);
    }
    public override Color? GetAlpha(Color lightColor)
    {
        return Color.White;
    }
    private Color StripColors(float progressOnStrip)
    {
        Color result = Color.Lerp(Color.White, isBig ? Color.Black : Color.White, Utils.GetLerpValue(0f, 0.7f, progressOnStrip, true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip, false));
        result.A /= 2;
        return result * Projectile.Opacity;
    }
    private float StripWidth(float progressOnStrip)
    {
        return isBig ? 60f : 30f;
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D effectTexture = TextureAssets.Extra[ExtrasID.SharpTears].Value;
        Vector2 effectOrigin = effectTexture.Size() / 2f;
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        float scaleX = isBig ? 1.25f : 0.75f;
        float scaleY = isBig ? 1.75f : 1.75f;
        Rectangle rectangle = texture.Frame(1, 1);
        Player player = Main.player[Projectile.owner];
        lightColor = Lighting.GetColor((int)player.Center.X / 16, (int)player.Center.Y / 16);
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Shader.Apply(null);
        TrailStrip.PrepareStrip(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, Projectile.Size * 0.5f - Main.screenPosition, Projectile.oldPos.Length, true);
        TrailStrip.DrawTrail();
        Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        Main.EntitySpriteDraw(effectTexture, drawPosition, null, new Color(255, 255, 255, 10), Projectile.velocity.ToRotation() + MathHelper.PiOver2, effectTexture.Size() / 2f, new Vector2(scaleX, scaleY), SpriteEffects.None, 0);
        return false;
    }
}
