using ITD.Particles.CosJel;
using Microsoft.Xna.Framework.Graphics;
using ITD.Particles;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using System.Collections.Generic;
using Terraria.ID;
using ITD.DetoursIL;
using System.Linq;

namespace ITD.Content.Projectiles.Unused
{
    public class CosJelHandTest : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 7;
        }
        public override void SetDefaults()
        {
            Projectile.timeLeft = 600;
            Projectile.height = Projectile.width = 32;
            Projectile.hostile = true;
            Projectile.friendly = false;
        }
        public override void AI()
        {
            Vector2 towardsMouse = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero) * 8f;
            Projectile.velocity = Vector2.SmoothStep(Projectile.velocity, towardsMouse, 0.1f);
            if (++Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = ++Projectile.frame % Main.projFrames[Type];
            }
            if (Main.rand.NextBool(3))
            {
                ///ITDParticle spaceMist = ParticleSystem.NewEmitter<SpaceMist>(Projectile.Center, (-Projectile.velocity).RotatedByRandom(1f), 0f);
                //spaceMist.tag = Projectile;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D outline = ModContent.Request<Texture2D>(Texture + "_Outline").Value;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);

            void DrawAtProj(Texture2D tex)
            {
                sb.Draw(tex, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, new Vector2(tex.Width * 0.5f, (tex.Height / Main.projFrames[Type]) * 0.5f), Projectile.scale, SpriteEffects.None, 0f);
            }
            DrawAtProj(outline);
            /*
            foreach (ITDParticle mist in ParticleSystem.Instance.particles.Where(p => p.tag == Projectile))
            {
                if (mist is SpaceMist sMist)
                {
                    sMist.DrawOutline(sb);
                }
            }
            foreach (ITDParticle mist in ParticleSystem.Instance.particles.Where(p => p.tag == Projectile))
            {
                mist.DrawCommon(sb, mist.Texture, mist.CanvasOffset);
            }
            */
            DrawAtProj(texture);
            return false;
        }
    }
}
