// :emoji_23:
/*
using Terraria;
using Microsoft.Xna.Framework.Graphics;
using System;
using ITD.Particles;
using Terraria.Graphics.Renderers;

namespace ITD.Content.Projectiles.Hostile
{
    public class RingMuzzleFlash : ITDParticle
    {
        private float OriginalScale;
        private float FinalScale;
        private float opacity = 1;
        private Color BaseColor;
        public override void SetDefaults()
        {
            timeLeft = 30;

            canvas = ParticleEmitterDrawCanvas.WorldUnderProjectiles;
        }
        public RingMuzzleFlash(Vector2 pos, Vector2 vel, Color col, Vector2 morph, float rotation, float originalScale, float finalScale, int lifeTime)
        {

            position = pos;
            velocity = vel;
            BaseColor = Color.White;
            texMorph = new Vector2(0.5f, 1f);
            OriginalScale = originalScale;
            FinalScale = 0.05f;
            scale = 0.34f + Main.rand.NextFloat(0.3f);
            rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }
        public override void AI()
        {

            BaseColor = Color.White;
            texMorph = new Vector2(0.5f, 1f);
            OriginalScale = 0.05f;
            FinalScale = 0.34f + Main.rand.NextFloat(0.3f);
            scale = MathHelper.Lerp(OriginalScale, FinalScale, 0.25f);

            opacity = (float)Math.Sin(MathHelper.PiOver2 + timeLeft * MathHelper.PiOver2);

            Lighting.AddLight(position, BaseColor.R / 255f, BaseColor.G / 255f, BaseColor.B / 255f);
            velocity *= 0.95f;
        }

        public override void PostDraw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, position - Main.screenPosition, null, BaseColor * opacity, rotation, Texture.Size() / 2f, scale, SpriteEffects.None, 0);
        }

    }
}
*/