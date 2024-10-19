using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using System;
using Terraria.DataStructures;
using Terraria.ModLoader.Config;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using ITD.DetoursIL;
namespace ITD.Particles
{
    public enum ParticleDrawCanvas
    {
        WorldUnderProjectiles,
        WorldOverProjectiles,
        Screen,
        UI
    }
    public abstract class ITDParticle() : IDisposable
    {
        internal uint type;
        internal Texture2D texture;
        public int frameVertical;
        public int frameHorizontal;
        public int frameCounter;
        public Vector2 position;
        public Vector2 velocity;
        public Vector2 texMorph;
        public int spawnTimeLeft;
        public int timeLeft;
        public float ProgressZeroToOne { get { return (spawnTimeLeft - timeLeft) / (float)spawnTimeLeft; } }
        public float ProgressOneToZero { get { return 1f - ProgressZeroToOne; } }
        public float rotation;
        public float scale = 1f;
        public float opacity = 1f;
        public ParticleDrawCanvas canvas;
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
        public virtual void AI()
        {

        }
        public void Update()
        {
            AI();
            position += velocity;
            if (timeLeft-- <= 0)
            {
                DetourManager.GetInstance<ParticleSystem>().particles.Remove(this);
                Dispose();
            }
        }
        public void Dispose()
        {
            texture = null;
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
            Vector2 offset = canvas == ParticleDrawCanvas.UI ? Vector2.Zero : Main.screenPosition;
            spriteBatch.Draw(texture, position - offset, data.Item1, Color.White * opacity, rotation, data.Item2, scale, SpriteEffects.None, 0f);
        }
        public virtual void PostDraw(SpriteBatch spriteBatch)
        {

        }
        public void DrawParticle(SpriteBatch spriteBatch)
        {
            Rectangle screen = canvas == ParticleDrawCanvas.UI ? new(0, 0, (int)(Main.screenWidth/Main.UIScale), (int)(Main.screenHeight/Main.UIScale)) : new((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight);
            int maxSize = (int)Math.Max((texture.Height / ParticleSystem.particleFramesVertical[type]) * scale, (texture.Width / ParticleSystem.particleFramesHorizontal[type]) * scale);
            Rectangle particleBoundingBox = new((int)position.X - maxSize / 2, (int)position.Y - maxSize / 2, maxSize, maxSize);
            // debug: see particle bounding box vvv
            //spriteBatch.Draw(TextureAssets.MagicPixel.Value, particleBoundingBox, Color.Red * 0.5f);
            if (screen.Intersects(particleBoundingBox))
            {
                if (PreDraw(spriteBatch))
                {
                    Draw(spriteBatch);
                }
                PostDraw(spriteBatch);
            }
        }
    }
}