using Terraria.Audio;
using Terraria.GameContent;
using System.Collections.Generic;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using ITD.Content.Dusts;

namespace ITD.Content.Projectiles.Hostile.CosJel
{
    public class CosmicStar : ModProjectile
    {
        //john vertexstrip!
        public VertexStrip TrailStrip = new VertexStrip();

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            Main.projFrames[Projectile.type] = 1;
        }
        public override void SetDefaults()
        {
            Projectile.width = 24; Projectile.height = 24;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
/*            Projectile.Kill();
*/            return false;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(Projectile.Center, 8, 8, ModContent.DustType<StarlitDust>(), Projectile.velocity.X, Projectile.velocity.Y, 0, Color.White, 1);
            }
            SoundEngine.PlaySound(SoundID.Item21, Projectile.position);
        }

        public override void AI()
        {
            if (Projectile.localAI[1]++ >= 60)
            {
                Projectile.alpha += 2;

                if (Projectile.alpha > 255)
                {
                    Projectile.Kill();
                }
            }
            Projectile.rotation = Projectile.velocity.ToRotation();
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
        private Color StripColors(float progressOnStrip)
        {
            Color result = Color.Lerp(new Color(36, 12, 34), new Color(84, 73, 255), Utils.GetLerpValue(0f, 0.7f, progressOnStrip, true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip, false));
            result.A /= 2;
            return result * 0.5f;
        }
        private float StripWidth(float progressOnStrip)
        {
            return 18 * Projectile.scale;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 stretch = new Vector2(1f, 1f);
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Rectangle frame = tex.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            Vector2 center = Projectile.Size / 2f;
            for (int i = Projectile.oldPos.Length - 1; i > 0; i--)
            {
                Projectile.oldRot[i] = Projectile.oldRot[i - 1];
                Projectile.oldRot[i] = Projectile.rotation + MathHelper.PiOver2;

            }
            Vector2 miragePos = Projectile.position - Main.screenPosition + center;
            Vector2 origin = new(tex.Width * 0.5f, (tex.Height / Main.projFrames[Type]) * 0.5f);

            GameShaders.Misc["LightDisc"].Apply(null);
            TrailStrip.PrepareStrip(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, Projectile.Size * 0.5f - Main.screenPosition, Projectile.oldPos.Length, true);
            TrailStrip.DrawTrail();
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
            //old treasure bag draw code, augh
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

                Main.EntitySpriteDraw(tex, miragePos + new Vector2(0f, 2).RotatedBy(radians) * time, frame, new Color(90, 70, 255, 50) * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            }

            for (float i = 0f; i < 1f; i += 0.5f)
            {
                float radians = (i + timer) * MathHelper.TwoPi;

                Main.EntitySpriteDraw(tex, miragePos + new Vector2(0f, 4).RotatedBy(radians) * time, frame, new Color(90, 70, 255, 50) * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(tex, miragePos, frame, Color.White * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}