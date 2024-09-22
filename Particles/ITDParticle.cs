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
        public int frameVertical;
        public int frameHorizontal;
        public Vector2 position;
        public Vector2 velocity;
        public int spawnTimeLeft;
        public int timeLeft;
        public float ProgressZeroToOne { get { return (spawnTimeLeft - timeLeft) / (float)spawnTimeLeft; } }
        public float ProgressOneToZero { get { return 1f - ProgressZeroToOne; } }
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
            SetDefaults();
            spawnTimeLeft = timeLeft;
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
        public (Rectangle, Vector2) GetFramingData()
        {
            int framesVertical = ParticleSystem.particleFramesVertical[type];
            int framesHorizontal = ParticleSystem.particleFramesHorizontal[type];
            int frameHeight = texture.Height / framesVertical;
            int frameWidth = texture.Width / framesHorizontal;
            return (new Rectangle(frameWidth * frameHorizontal, frameHeight * frameVertical, frameWidth, frameHeight), new Vector2(frameWidth * 0.5f, frameHeight * 0.5f));
        }
        private void Draw(SpriteBatch spriteBatch)
        {
            (Rectangle, Vector2) data = GetFramingData();
            spriteBatch.Draw(texture, position - Main.screenPosition, data.Item1, Color.White * ProgressOneToZero, rotation, data.Item2, scale * ProgressOneToZero, SpriteEffects.None, 0f);
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
