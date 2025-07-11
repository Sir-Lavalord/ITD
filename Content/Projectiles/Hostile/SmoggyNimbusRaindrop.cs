using System;
using System.Collections.Generic;
using Terraria.GameContent;

namespace ITD.Content.Projectiles.Hostile
{
    public class SmoggyNimbusRaindrop : ModProjectile
    {
        public override string Texture => "ITD/Particles/Textures/LavaRainParticle";
        public override void SetDefaults()
        {
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.width = 12;
            Projectile.height = 28;
            Projectile.timeLeft = 120;
            Projectile.hide = true;
        }
        public override void AI()
        {
            Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch);
            d.noGravity = true;
        }
        public override void OnKill(int timeLeft)
        {
            int amountOfDusts = 6;
            for (int i = 0; i < amountOfDusts; i++)
            {
                float angleRange = MathHelper.PiOver4;
                float rotRadians = angleRange / (float)amountOfDusts;
                Vector2 velocity = (angleRange - rotRadians).ToRotationVector2().RotatedBy(rotRadians * i) * 4f;
                Dust.NewDust(Projectile.Bottom + Vector2.UnitX * Projectile.velocity.X, 1, 1, DustID.Torch, velocity.X, velocity.Y, Scale: 1.5f);
            }
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.Lerp(Color.White, Color.SlateGray, (MathF.Sin((float)Main.timeForVisualEffects / 8f) + 1f) / 2f);
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.localAI[0] < 1f)
                Projectile.localAI[0] += 0.1f;
            Texture2D glowTex = ModContent.Request<Texture2D>("ITD/Particles/Textures/LyteflyParticle_Glow").Value;
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Rectangle texFrame = tex.Frame(3, 1, (int)Projectile.ai[0]);
            Vector2 texOrigin = new(tex.Width / 3 / 2, tex.Height / 2);
            Color glowColor = (Color.OrangeRed with { A = 0 }) * 0.5f * Projectile.localAI[0];
            Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, glowColor, 0f, glowTex.Size() * 0.5f, 0.5f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, Color.Yellow with { A = 0 } * 0.4f * Projectile.localAI[0], 0f, glowTex.Size() * 0.5f, 0.2f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, texFrame, GetAlpha(Color.White).Value, Projectile.velocity.ToRotation() - MathHelper.PiOver2, texOrigin, Projectile.localAI[0] * 1.05f, SpriteEffects.None, 0f);
            return false;
        }
    }
}
