using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Hostile.MotherWisp
{
    public class WispChevronBullet : ModProjectile
    {
        public override string Texture => "ITD/Content/Projectiles/Hostile/MotherWisp/WispFireBall";

        public VertexStrip TrailStrip = new();
        public VertexStrip TrailStrip2 = new();

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 540;
            Projectile.alpha = 255;
            Projectile.hide = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindProjectiles.Add(index);
        }

        private Color StripColors(float progressOnStrip) => new Color(53, 247, 180) * Projectile.Opacity;
        private Color StripColors2(float progressOnStrip) => Color.White * Projectile.Opacity;

        private float StripWidth(float progressOnStrip) => MathHelper.Lerp(16f, 0f, Utils.GetLerpValue(0f, 0.6f, progressOnStrip, true)) * Projectile.scale;
        private float StripWidth2(float progressOnStrip) => MathHelper.Lerp(8f, 0f, Utils.GetLerpValue(0f, 0.4f, progressOnStrip, true)) * Projectile.scale;

        public override void AI()
        {
            float sweepAngle = Projectile.ai[0];
            float chevronIndex = Projectile.ai[1];
            float sweepDelay = Projectile.ai[2];

            float time = Projectile.localAI[0]++;

            if (time < 40f)
            {
                Projectile.velocity = Vector2.Zero;
                Projectile.alpha = 255;
                return;
            }
            if (time == 40f)
            {
                if (chevronIndex == 0) SoundEngine.PlaySound(SoundID.Item34, Projectile.Center);

                float initialSpeed = 18f;
                Projectile.velocity = sweepAngle.ToRotationVector2() * initialSpeed;
                Projectile.rotation = Projectile.velocity.ToRotation();
            }
            float bulletTime = time - 40f;

            float spawnDelay = (chevronIndex + 6f) * 2f;

            float baseFormTime = 30f - spawnDelay;
            if (baseFormTime < 2f) baseFormTime = 2f;

            float totalFormTime = baseFormTime + (sweepDelay * 40f);

            if (bulletTime < totalFormTime)
            {
                float fadeProgress = MathHelper.Clamp(bulletTime / totalFormTime, 0f, 1f);
                Projectile.alpha = (int)MathHelper.Lerp(255, 0, fadeProgress);
            }
            else
            {
                Projectile.alpha = 0;
            }

            float morphDuration = 35f;
            float finalUnifiedSpeed = 8f;

            if (bulletTime < baseFormTime)
            {
                float minSpeed = 3f;
                float gentleBrake = 0.85f;

                if (Projectile.velocity.Length() > minSpeed)
                {
                    Projectile.velocity *= gentleBrake;
                }
            }
            else if (bulletTime < totalFormTime)
            {
                Projectile.velocity *= 0.8f;

                if (Projectile.velocity.Length() < 0.01f)
                {
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * 0.01f;
                }
            }
            else if (bulletTime < totalFormTime + morphDuration)
            {
                float catchupFactor = 0.25f;
                float speedAdjustment = (Math.Abs(chevronIndex) * catchupFactor);

                if (Math.Abs(chevronIndex) <= 0.5f) speedAdjustment = 2f;

                float targetSpeed = finalUnifiedSpeed * speedAdjustment;
                Vector2 targetVel = Projectile.velocity.SafeNormalize(Vector2.UnitY) * targetSpeed;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetVel, 0.08f);
            }
            else
            {
                Vector2 finalVel = Projectile.velocity.SafeNormalize(Vector2.UnitY) * finalUnifiedSpeed;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, finalVel, 0.15f);
            }

            Projectile.rotation = Projectile.velocity.ToRotation();
            Lighting.AddLight(Projectile.Center, 0.4f, 0.1f, 0.6f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float time = Projectile.localAI[0];

            if (time < 40f)
            {
                if (ModContent.RequestIfExists<Texture2D>("ITD/Content/Projectiles/Friendly/Mage/TwilightDemiseHorribleThing", out var effectTexture))
                {
                    float spawnerScale = 1f - (time / 40f);
                    Vector2 drawPosition = Projectile.Center - Main.screenPosition;

                    Main.EntitySpriteDraw(
                        effectTexture.Value,
                        drawPosition,
                        null,
                        new Color(37, 255, 152, 0),
                        Projectile.rotation,
                        effectTexture.Value.Size() / 2f,
                        new Vector2(2f, 2f) * spawnerScale,
                        SpriteEffects.None,
                        0
                    );
                }
                return false;
            }
            SpriteBatch sb = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame(1, 1, 0, 0);
            Vector2 origin = new(texture.Width * 0.5f, texture.Height * 0.5f);
            Vector2 offset = Projectile.Size * 0.5f - Main.screenPosition;
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            if (ModContent.RequestIfExists<Texture2D>("ITD/Content/Projectiles/Friendly/Mage/TwilightDemiseHorribleThing", out var texture2))
            {
                Rectangle frame2 = texture2.Value.Frame(1, 1, 0, 0);
                Main.EntitySpriteDraw(
                    texture2.Value,
                    Projectile.Center - Main.screenPosition,
                    frame2,
                    new Color(207, 255, 200, 180) * Projectile.Opacity,
                    Projectile.rotation,
                    new Vector2(texture2.Value.Width * 0.5f, texture2.Value.Height * 0.5f),
                    Projectile.scale * 0.6f,
                    SpriteEffects.None,
                    0f
                );
            }

            sb.Draw(texture, Projectile.Center + Main.rand.NextVector2Circular(2, 2) - Main.screenPosition, frame, Projectile.GetAlpha(Color.White), Projectile.rotation, origin, Projectile.scale, effects, 0f);

            GameShaders.Misc["LightDisc"].Apply(null);

            TrailStrip.PrepareStrip(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, offset, Projectile.oldPos.Length, true);
            TrailStrip2.PrepareStrip(Projectile.oldPos, Projectile.oldRot, StripColors2, StripWidth2, offset, Projectile.oldPos.Length, true);

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

            TrailStrip.DrawTrail();
            TrailStrip2.DrawTrail();

            return false;
        }
    }
}