using ITD.Particles;
using ITD.Particles.Projectiles;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
namespace ITD.Content.Projectiles.Friendly.Mage;

public class TwilightDemiseProj : ITDProjectile
{
    public MiscShaderData Shader = new MiscShaderData(Main.VertexPixelShaderRef, "MagicMissile")
        .UseProjectionMatrix(true)
        .UseImage0("Images/Extra_" + 192)
        .UseImage1("Images/Extra_" + 194)
        .UseImage2("Images/Extra_" + 193)
        .UseSaturation(-2.8f)
        .UseOpacity(2f);

    public VertexStrip TrailStrip = new();

    public int HomingTime;
    public override void SetStaticDefaults()
    {

        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
    }
    private NPC HomingTarget
    {
        get => Projectile.ai[0] == 0 ? null : Main.npc[(int)Projectile.ai[0] - 1];
        set
        {
            Projectile.ai[0] = value == null ? 0 : value.whoAmI + 1;
        }
    }
    public ParticleEmitter emitter;
    public bool spawnAnim = false;
    public override void SetDefaults()
    {
        Projectile.width = 40;
        Projectile.height = 40;
        Projectile.aiStyle = 0;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.ignoreWater = true;
        Projectile.light = 1f;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 300;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 5;
        Projectile.alpha = 0;
        Projectile.Opacity = 1;

        emitter = ParticleSystem.NewEmitter<TwilightDemiseFlash>(ParticleEmitterDrawCanvas.WorldUnderProjectiles);
        emitter.tag = Projectile;
    }

    public override void OnSpawn(IEntitySource source)
    {
        if (emitter != null)
            emitter.keptAlive = true;
    }
    public override void AI()
    {
        if (emitter != null)
            emitter.keptAlive = true;
        Projectile.rotation = Projectile.velocity.ToRotation();
        if (Main.rand.NextBool(2))
        {
            emitter?.Emit(Projectile.Center + Main.rand.NextVector2Circular(15, 15), Vector2.Zero);
        }

        float maxDetectRadius = 800 - (Projectile.ai[2] * 100);
        if (!spawnAnim)
        {                //from clamtea
            float dustLoopCheck = 24f;
            int dustIncr = 0;
            while (dustIncr < dustLoopCheck)
            {
                Vector2 dustRotate = Vector2.UnitX * 0f;
                dustRotate += -Vector2.UnitY.RotatedBy((double)(dustIncr * (MathHelper.TwoPi / dustLoopCheck)), default) * new Vector2(2f, 8f);
                dustRotate = dustRotate.RotatedBy((double)Projectile.velocity.ToRotation(), default);
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.WhiteTorch, 0f, 0f, 0, default, 2f);
                dust.noGravity = true;
                dust.scale = 2f;
                dust.position = Projectile.Center + dustRotate;
                dust.velocity = Projectile.velocity * 0f + dustRotate.SafeNormalize(Vector2.UnitY) * 2f;
                dustIncr++;
            }
            spawnAnim = true;

        }
        if (HomingTime++ >= 4)
        {
            if (Projectile.timeLeft >= 20)
            {
                HomingTarget ??= Projectile.FindClosestNPC(maxDetectRadius);

                if (HomingTarget == null)
                    return;
                if (!HomingTarget.active || HomingTarget.life <= 0 || !HomingTarget.CanBeChasedBy())
                {
                    HomingTarget = null;
                    return;
                }
                int inertia = 20;
                int vel = 14;
                float length = Projectile.velocity.Length();
                float targetAngle = Projectile.AngleTo(HomingTarget.Center);
                Vector2 homeDirection = (HomingTarget.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                Projectile.velocity = (Projectile.velocity * inertia + homeDirection * vel) / (inertia + 1f);
            }
            else
            {
                Projectile.scale *= 0.98f;

                Projectile.alpha += 6;
            }
        }
    }
    public override bool? CanDamage()
    {
        if (Projectile.timeLeft > 20 && HomingTime >= 4)
        {
            return true;
        }
        else
        {
            return false;
        }

    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        Projectile.velocity *= 0f;

        if (Projectile.timeLeft > 20)
        {
            Projectile.timeLeft = 20;
        }
    }
    private Color StripColors(float progressOnStrip)
    {
        Color result = Color.Lerp(new Color(122, 0, 208, 0), Color.White, Utils.GetLerpValue(0f, 0.7f, progressOnStrip, true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip, false));
        result.A /= 2;
        return result * Projectile.Opacity;
    }
    private float StripWidth(float progressOnStrip)
    {
        return MathHelper.Lerp(36f, 60f, 2f);
    }
    public override void OnKill(int timeLeft)
    {

        for (int i = 0; i < 8; i++)
        {
            emitter?.Emit(Projectile.Center, (Vector2.UnitX * 3).RotatedByRandom(MathHelper.Pi) * Main.rand.NextFloat(0.75f, 2f));
        }

    }
    float innerScale = 1f;
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D outline = ModContent.Request<Texture2D>(Texture + "_Outline").Value;
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        //this sprite should be remade, i got this from fargo
        Texture2D texture2 = Mod.Assets.Request<Texture2D>("Content/Projectiles/Friendly/Mage/TwilightDemiseHorribleThing").Value;
        Rectangle frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
        Rectangle frame2 = texture2.Frame(1, Main.projFrames[Type], 0, Projectile.frame);

        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Texture2D deathTex = TextureAssets.Extra[ExtrasID.SharpTears].Value;
        Rectangle deathFrame = deathTex.Frame(1, Main.projFrames[Type], 0, Projectile.frame);

        void DrawAtProj(Texture2D tex)
        {
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, new Vector2(tex.Width * 0.5f, tex.Height / Main.projFrames[Type] * 0.5f), innerScale * Projectile.scale, Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
        }

        if (Projectile.timeLeft >= 20)
        {
            Shader.Apply(null);
            TrailStrip.PrepareStrip(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, Projectile.Size * 0.5f - Main.screenPosition, Projectile.oldPos.Length, true);
            TrailStrip.DrawTrail();
            innerScale = 1f;
        }
        else
        {
            Projectile.ai[2]++;
            innerScale *= 0.99f - Projectile.ai[2] / 50;
        }
        emitter?.InjectDrawAction(ParticleEmitterDrawStep.BeforePreDrawAll, () => Main.EntitySpriteDraw(texture2, Projectile.Center - Main.screenPosition, frame2, new Color(122, 0, 208, 0), Projectile.rotation, new Vector2(texture2.Width * 0.5f, texture2.Height / Main.projFrames[Type] * 0.5f), innerScale * Projectile.scale * 0.6f, SpriteEffects.None, 0f));

        emitter?.InjectDrawAction(ParticleEmitterDrawStep.BeforePreDrawAll, () => DrawAtProj(outline));
        if (Projectile.timeLeft >= 5)
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, new Vector2(texture.Width * 0.5f, texture.Height / Main.projFrames[Type] * 0.5f), innerScale * Projectile.scale, SpriteEffects.None, 0f);

        return false;
    }

    public override int ProjectileShader(int originalShader)
    {
        return GameShaders.Armor.GetShaderIdFromItemId(ItemID.TwilightDye);
    }
}
