/*using Terraria.GameContent;
using ITD.Utilities;
using ITD.Particles;
using ITD.Particles.Misc;


namespace ITD.Content.Projectiles.Hostile.CosJel
{
    public class CosmicJellyfish_SetHand : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            Main.projFrames[Projectile.type] = 7;
        }
        public override void SetDefaults()
        {
            Projectile.width = 64; Projectile.height = 64;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.scale = 1.5f;
            emitter = ParticleSystem.NewEmitter<BeanMist>(ParticleEmitterDrawCanvas.WorldUnderProjectiles);
            emitter.tag = Projectile;
        }
        public ParticleEmitter emitter;
        public override void AI()
        {
            Main.LocalPlayer.ITD().BetterScreenshake(20, 6, 8, true);

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (emitter != null)
                emitter.keptAlive = true;
            if (Main.rand.NextBool(4))
            {
                Vector2 velo = Projectile.rotation.ToRotationVector2().RotatedBy(MathHelper.PiOver2);
                Vector2 veloDelta = Projectile.velocity;
                emitter?.Emit(Projectile.Center + new Vector2(0f, Projectile.height / 2 - 14), ((velo * 1.25f) + veloDelta).RotatedByRandom(0.1f));
                Projectile proj1 = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, -((velo * 2f) + veloDelta).RotatedByRandom(0.3f), ModContent.ProjectileType<CosmicVoidShard>(), (Projectile.damage), 0, Main.myPlayer);
                proj1.tileCollide = false;
                emitter?.Emit(Projectile.Center + new Vector2(0f, Projectile.height / 2 - 14), ((velo * 2f) + veloDelta).RotatedByRandom(0.3f));
            }
            Projectile.frame = 6;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D outline = ModContent.Request<Texture2D>(Texture + "_Outline").Value;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 center = Projectile.Size / 2f;
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + center;
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Vector2 origin = new(outline.Width * 0.5f, (outline.Height / Main.projFrames[Type]) * 0.5f);
                sb.Draw(outline, drawPos, frame, color, Projectile.oldRot[k], origin, Projectile.scale, SpriteEffects.None, 0f);
            }
            void DrawAtProj(Texture2D tex)
            {
                sb.Draw(tex, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, new Vector2(tex.Width * 0.5f, (tex.Height / Main.projFrames[Type]) * 0.5f), Projectile.scale, Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            }
            emitter?.InjectDrawAction(ParticleEmitterDrawStep.BeforePreDrawAll, () => DrawAtProj(outline));
            emitter?.InjectDrawAction(ParticleEmitterDrawStep.AfterPreDrawAll, () => DrawAtProj(texture));
            return false;
        }
    }
}
*/