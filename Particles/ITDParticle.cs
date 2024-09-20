using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using System;
using Terraria.DataStructures;
using Terraria.ModLoader.Config;
using Terraria.ModLoader;
namespace ITD.Particles
{
    public enum ParticleDrawCanvas
    {
        World,
        Screen,
        UI
    }
    public enum WorldParticleDrawLayer
    {
        UnderProjectiles,
        OverProjectiles
    }
    public abstract class ITDParticle() : IDisposable
    {
        internal uint type;
        internal Texture2D texture;
        public Rectangle frame;
        public Vector2 position;
        public Vector2 velocity;
        public int timeLeft;
        public float rotation;
        public float scale = 1f;
        public float opacity;
        public ParticleDrawCanvas canvas;
        public WorldParticleDrawLayer? layer;
        public virtual void SetStaticDefaults()
        {

        }
        public virtual void SetDefaults()
        {

        }
        public void Initialize()
        {
            int frames = ParticleSystem.particleFrames[type] == 0 ? 1 : ParticleSystem.particleFrames[type];
            int frameHeight = texture.Height / frames;
            frame = new Rectangle(0, 0, texture.Width, frameHeight);
            SetDefaults();
        }
        public virtual void PreUpdate()
        {

        }
        public void Update()
        {
            PreUpdate();
            position += velocity;
            if (timeLeft-- <= 0)
            {
                ModContent.GetInstance<ParticleSystem>().particles.Remove(this);
                Dispose();
            }
        }
        public void Dispose()
        {
            texture = null;
            layer = null;
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Return false to stop default regular drawing
        /// </summary>
        /// <param name="spriteBatch"></param>
        public virtual bool PreDraw(SpriteBatch spriteBatch)
        {
            return true;
        }
        private void Draw(SpriteBatch spriteBatch)
        {
            int frames = ParticleSystem.particleFrames[type] == 0 ? 1 : ParticleSystem.particleFrames[type];
            Vector2 origin = new Vector2(texture.Width * 0.5f, texture.Height / frames * 0.5f);
            spriteBatch.Draw(texture, position - Main.screenPosition, frame, Color.White, rotation, origin, scale, SpriteEffects.None, 0f);
        }
        public virtual void PostDraw(SpriteBatch spriteBatch)
        {

        }
        public void DrawParticle(SpriteBatch spriteBatch)
        {
            if (PreDraw(spriteBatch))
            {
                Draw(spriteBatch);
            }
            PostDraw(spriteBatch);
        }
    }
}
