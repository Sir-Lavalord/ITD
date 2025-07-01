using Terraria;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using System.Collections.Generic;
using System.Threading;
using Terraria.UI;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using ITD.Content.Dusts;

namespace ITD.Content.Projectiles.Hostile.CosJel
{
    public class CosmicWave : ModProjectile
    {
        //john vertexstrip!
        public VertexStrip TrailStrip = new VertexStrip();
       
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            Main.projFrames[Projectile.type] = 5;
        }
        public override void SetDefaults()
        {
            Projectile.width = 40; Projectile.height = 30;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 400;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.Kill();
            return false;
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
            if (Projectile.ai[1]++ < 40)
            {
                Projectile.velocity *= 1.05f;
                Projectile.netUpdate = true;
                if (++Projectile.frameCounter >= 4)
                {
                    Projectile.frameCounter = 0;
                    if (Projectile.frame < 2)
                        Projectile.frame++;
                }
            }
            else if (Projectile.ai[1] >= 60 && Projectile.ai[1] <= 120)
            {
                Projectile.velocity *= 0.98f;
                Projectile.netUpdate = true;
                if (++Projectile.frameCounter >= 8)
                {
                    Projectile.frameCounter = 0;
                    if (Projectile.frame < 4)
                        Projectile.frame++;
                }

            }
            else if (Projectile.ai[1] > 120)
            {
                if (Projectile.alpha < 180)
                    Projectile.alpha += 2;
                else Projectile.Kill();
            }
            
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * (1f - Projectile.alpha / 255f);
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
            stretch = new Vector2(1f, 1f + Projectile.velocity.Length() * 0.05f);
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

            for (float i = 0f; i < 1f; i += 0.25f)
            {
                float radians = (i + timer) * MathHelper.TwoPi;

                Main.EntitySpriteDraw(tex, miragePos + new Vector2(0f, 4f).RotatedBy(radians) * time, frame, new Color(90, 70, 255, 50), Projectile.rotation, origin, stretch, SpriteEffects.None, 0);
            }

            for (float i = 0f; i < 1f; i += 0.34f)
            {
                float radians = (i + timer) * MathHelper.TwoPi;

                Main.EntitySpriteDraw(tex, miragePos + new Vector2(0f, 6f).RotatedBy(radians) * time, frame, new Color(90, 70, 255, 50), Projectile.rotation, origin, stretch, SpriteEffects.None, 0);
            }
            Main.EntitySpriteDraw(tex, center, frame, default, Projectile.rotation, origin, stretch, SpriteEffects.None, 0);

            return false;
        }
    }
}