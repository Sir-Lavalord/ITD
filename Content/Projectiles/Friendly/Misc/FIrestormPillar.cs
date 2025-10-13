using ITD.Particles;
using ITD.Particles.Misc;
using ITD.Utilities.EntityAnim;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace ITD.Content.Projectiles.Friendly.Misc;

public class FirestormPillar : ModProjectile
{
    private const int LifeTime = 360;
    private float ProgressOneToZero => Projectile.timeLeft / (float)LifeTime;
    public ParticleEmitter emitter;
    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 4;
    }
    public override void SetDefaults()
    {
        Projectile.hostile = false;
        Projectile.friendly = true;
        Projectile.width = 48;
        Projectile.height = 64;
        Projectile.timeLeft = LifeTime;
        Projectile.penetrate = -1;
        Projectile.usesIDStaticNPCImmunity = true;
        Projectile.idStaticNPCHitCooldown = 60;
        emitter = ParticleSystem.NewEmitter<PyroclasticParticle>(ParticleEmitterDrawCanvas.WorldOverProjectiles);
        emitter.tag = Projectile;
    }
    public int frameShift = 0;
    public float fireStart = 0;
    public override void OnSpawn(IEntitySource source)
    {
        if (emitter != null)
            emitter.keptAlive = true;
        frameShift = Main.rand.Next(-2, 6);
        fireStart = Projectile.ai[0] + 1;
    }
    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        return false;
    }
    public override void AI()
    {
        if (emitter != null)
            emitter.keptAlive = true;
        if (Projectile.ai[0] <= fireStart * 0.75f)
        {
            emitter?.Emit(Projectile.Bottom + new Vector2(Main.rand.NextFloat(-Projectile.width / 1.75f * (1 - (Projectile.ai[0] / fireStart)), Projectile.width / 1.75f * (1 - (Projectile.ai[0] / fireStart))), 0), Vector2.Zero, 20);
        }

        if (Projectile.ai[0]-- <= 0)
        {
            Projectile.ai[0] = 0;

            if (Main.rand.NextFloat() < ProgressOneToZero)
            {
                Rectangle dustRect = Projectile.Hitbox;
                ModifyDamageHitbox(ref dustRect);
                Dust d = Dust.NewDustDirect(dustRect.Location.ToVector2(), dustRect.Width, dustRect.Height, DustID.Torch, 0f, 0f, Scale: 1.5f);
                d.noGravity = true;
                d.velocity.Y -= 1f;
            }
            if (++Projectile.frameCounter >= 6 + frameShift)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
            }
            if (!Main.dedServ)
            {

            }
        }
        else
        {
            Projectile.timeLeft = LifeTime;
        }
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (Main.rand.NextBool(3))
            target.AddBuff(BuffID.OnFire, 60);
    }
    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
    {
        fallThrough = false;
        return true;
    }
    public override void ModifyDamageHitbox(ref Rectangle hitbox)
    {
        Rectangle baseHitbox = hitbox;
        int newHeight = (int)MathHelper.Lerp(baseHitbox.Height, 1, 1f - ProgressOneToZero);
        int newWidth = (int)MathHelper.Lerp(baseHitbox.Width, baseHitbox.Width * 1.35f, 1f - ProgressOneToZero);
        int newX = baseHitbox.Center.X - newWidth / 2;
        int newY = baseHitbox.Y + (baseHitbox.Height - newHeight);
        hitbox = new(newX, newY, newWidth, newHeight);
    }
    float factor = 1f;

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = TextureAssets.Projectile[Type].Value;
        int frameHeight = tex.Height / Main.projFrames[Type];
        Vector2 origin = new(tex.Width / 2, frameHeight); // bottom middle origin
        Rectangle frame = tex.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
        float progress = 1f - ProgressOneToZero;
        if (Projectile.ai[0] <= 0)
        {
            float lim = 0.05f;
            if (progress <= lim)
            {
                factor = MathHelper.Lerp(1f, 0f, EasingFunctions.OutQuad(progress / lim));
            }
        }
        frame.Height = (int)MathHelper.Lerp(frame.Height, 1, factor);

        Main.spriteBatch.Draw(tex, Projectile.Bottom - Main.screenPosition + Vector2.UnitY * (frameHeight - frame.Height), frame, Color.White with { A = 0 }, 0f, origin, 1f, frameShift % 2 == 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
        return false;
    }
}
