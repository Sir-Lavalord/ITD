using Terraria;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace ITD.Content.Projectiles.Hostile.CosJel
{
    public class CosmicVoidShard : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 14; Projectile.height = 28;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 400;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
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
                Dust.NewDust(Projectile.Center, 8, 8, DustID.PortalBolt, Projectile.velocity.X, Projectile.velocity.Y, 0, Color.White, 1);
            }
            SoundEngine.PlaySound(SoundID.Item27, Projectile.position);
        }

        public override void AI()
        {
            if (Projectile.ai[1] == 2)
            {
                if (Projectile.ai[2]++ >= 30)
                {
                    Projectile.extraUpdates = 1;
                    Projectile.velocity *= 1.02f;
                }
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            Texture2D effectTexture = TextureAssets.Extra[98].Value;
            Vector2 effectOrigin = effectTexture.Size() / 2f;
            lightColor = Lighting.GetColor((int)player.Center.X / 16, (int)player.Center.Y / 16);

            Main.EntitySpriteDraw(effectTexture, Projectile.Center, new Rectangle?(Projectile.Hitbox), new Color(120, 184, 255, 50) * 0.05f * Projectile.timeLeft, Projectile.rotation, effectOrigin, Projectile.scale, SpriteEffects.None, 0f);
            return true;
        }
    }
}