using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Friendly.Summoner
{
    public class NeptunePortal : ModProjectile
    {
        public override string Texture => ITD.BlankTexture;

        private float spawnGlow = 1f;
        private float pulseGlow = 1f;

        public override void SetDefaults()
        {
            Projectile.width = 150;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 90;
            Projectile.scale = 0f;
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            if (Projectile.timeLeft > 20)
                Projectile.scale = MathHelper.Clamp(Projectile.scale + 0.05f, 0, 0.5f);
            else
                Projectile.scale = MathHelper.Clamp(Projectile.scale - 0.05f, 0, 0.5f);

            if (pulseGlow <= 0)
                pulseGlow = 1;

            pulseGlow -= 0.01f;
            spawnGlow -= 0.025f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float slowPulse = 1 + (Main.essScale / 3);
            Texture2D effectTexture = TextureAssets.Extra[ExtrasID.PortalGateHalo].Value;
            Texture2D effectTexture2 = TextureAssets.Extra[ExtrasID.PortalGateHalo2].Value;

            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            Main.EntitySpriteDraw(effectTexture2, drawPosition + new Vector2(0, -30).RotatedBy(Projectile.rotation), null, new Color(10, 100, 255, 0), Projectile.rotation, effectTexture2.Size() / 2f, new Vector2(3 * Projectile.scale, 4) * slowPulse, SpriteEffects.None, 0);

            float time = Main.GlobalTimeWrappedHourly;
            float timer = (float)Main.time / 240f + time * 0.04f;
            time %= 4f;
            time /= 2f;
            if (time >= 1f) time = 2f - time;
            time = time * 0.5f + 0.5f;

            for (float i = 0f; i < 1f; i += 0.35f)
            {
                float radians = (i + timer) * MathHelper.TwoPi;
                Main.EntitySpriteDraw(effectTexture, drawPosition + new Vector2(0f, 6).RotatedBy(radians) * time, null, new Color(10, 100, 255, 0) * Projectile.Opacity, Projectile.rotation, effectTexture.Size() / 2f, new Vector2(3 * Projectile.scale, 0.75f), SpriteEffects.None, 0);
            }

            for (float i = 0f; i < 1f; i += 0.5f)
            {
                float radians = (i + timer) * MathHelper.TwoPi;
                Main.EntitySpriteDraw(effectTexture, drawPosition + new Vector2(0f, 2).RotatedBy(radians) * time, null, new Color(10, 100, 255, 0) * Projectile.Opacity, Projectile.rotation, effectTexture.Size() / 2f, new Vector2(3 * Projectile.scale, 0.75f), SpriteEffects.None, 0);
            }

            if (spawnGlow > 0)
            {
                float scale = 2.5f * Projectile.scale * (float)Math.Cos(Math.PI / 2 * spawnGlow);
                float opacity = Projectile.Opacity * (float)Math.Sqrt(spawnGlow);
                Main.EntitySpriteDraw(effectTexture, drawPosition, null, new Color(10, 100, 255, 0), Projectile.rotation, effectTexture.Size() / 2f, new Vector2(3 * Projectile.scale, 0.75f) * scale, SpriteEffects.None, 0);
            }
            if (pulseGlow > 0)
            {
                float scale = 2.25f * Projectile.scale * (float)Math.Cos(Math.PI / 2 * pulseGlow);
                float opacity = Projectile.Opacity * (float)Math.Sqrt(pulseGlow);
                Main.EntitySpriteDraw(effectTexture, drawPosition, null, new Color(10, 100, 255, 0), Projectile.rotation, effectTexture.Size() / 2f, new Vector2(3 * Projectile.scale, 0.75f) * slowPulse, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(effectTexture, drawPosition, null, new Color(10, 50, 255, 0) * opacity, Projectile.rotation, effectTexture.Size() / 2f, new Vector2(3 * Projectile.scale, 0.75f) * scale, SpriteEffects.None, 0);
            }

            return false;
        }
    }
}