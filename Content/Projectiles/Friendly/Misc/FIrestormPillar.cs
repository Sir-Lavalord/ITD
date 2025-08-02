using Terraria.GameContent;
using ITD.Utilities.EntityAnim;
using ITD.Particles;
using ITD.Particles.Misc;

namespace ITD.Content.Projectiles.Friendly.Misc
{
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
            emitter = ParticleSystem.NewEmitter<PyroclasticParticle>(ParticleEmitterDrawCanvas.WorldUnderProjectiles);
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }
        public override void AI()
        {
            if (Projectile.ai[0]-- < 0)
            {

                Projectile.Opacity = 1f;
                emitter.keptAlive = true;
                if (Main.rand.NextFloat() < ProgressOneToZero)
                {
                    Rectangle dustRect = Projectile.Hitbox;
                    ModifyDamageHitbox(ref dustRect);
                    Dust d = Dust.NewDustDirect(dustRect.Location.ToVector2(), dustRect.Width, dustRect.Height, DustID.Torch, 0f, 0f, Scale: 1.5f);
                    d.noGravity = true;
                    d.velocity.Y -= 1f;
                }
                if (++Projectile.frameCounter >= 6)
                {
                    Projectile.frameCounter = 0;
                    Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
                }
                if (!Main.dedServ)
                {

                }
            }
            else Projectile.Opacity = 0f;
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
        float factor = 0f;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            int frameHeight = tex.Height / Main.projFrames[Type];
            Vector2 origin = new(tex.Width / 2, frameHeight); // bottom middle origin
            Rectangle frame = tex.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            if (Projectile.ai[0]-- < 0)
            {
                float progress = 1f - ProgressOneToZero;
                float lim = 0.15f;
                if (Projectile.ai[0]-- < 0)
                {
                    if (progress <= lim)
                    {
                        factor = MathHelper.Lerp(1f, 0f, EasingFunctions.OutQuad(progress / lim));
                    }
                    else
                    {

                        factor = MathHelper.Lerp(0f, 1f, EasingFunctions.InQuad((progress - lim) / (1f - lim)));

                    }
                }
                frame.Height = (int)MathHelper.Lerp(frame.Height, 1, factor);
            }
            Main.spriteBatch.Draw(tex, Projectile.Bottom - Main.screenPosition + Vector2.UnitY * (frameHeight - frame.Height), frame, Color.White with { A = 0 }, 0f, origin, 1f, SpriteEffects.None, 0f);
            return false;
        }
    }
}
