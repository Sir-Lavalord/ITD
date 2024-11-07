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
        internal Texture2D Texture { get { return ModContent.Request<Texture2D>(TexturePath).Value; } }
        /// <summary>
        /// this is the default texture path that points to Particles/Textures/nameof(this). this cannot be overriden but you can override TexturePath to change the texture path
        /// </summary>
        internal string ExpectedTexturePath;
        public virtual string TexturePath => ExpectedTexturePath;
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
        public float spawnScale;
        public float opacity = 1f;
        public ParticleDrawCanvas canvas;

        /// <summary>
        /// what if ai[] arrays were good
        /// </summary>
        public object tag;
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
            spawnScale = scale;
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
        public virtual Color GetAlpha()
        {
            return Lighting.GetColor(position.ToTileCoordinates());
        }
        public (Rectangle, Vector2) GetFramingData()
        {
            int framesVertical = ParticleSystem.particleFramesVertical[type];
            int framesHorizontal = ParticleSystem.particleFramesHorizontal[type];
            int frameHeight = Texture.Height / framesVertical;
            int frameWidth = Texture.Width / framesHorizontal;
            return (new Rectangle(frameWidth * frameHorizontal, frameHeight * frameVertical, frameWidth, frameHeight), new Vector2(frameWidth * 0.5f, frameHeight * 0.5f));
        }
        public Vector2 CanvasOffset { get { return canvas == ParticleDrawCanvas.UI ? Vector2.Zero : Main.screenPosition; } }
        private void Draw(SpriteBatch spriteBatch)
        {
            DrawCommon(spriteBatch, Texture, CanvasOffset);
        }
        public virtual void PostDraw(SpriteBatch spriteBatch)
        {

        }
        /// <summary>
        /// Draws something on the center of the particle (this is called in the regular particle draw call, but can be used for other cases)
        /// The data from GetFramingData() will be used unless specified otherwise here (origin and sourceRect)
        /// </summary>
        public void DrawCommon(SpriteBatch spriteBatch, Texture2D texture, Vector2 offset = default, Color? color = null, Rectangle? sourceRect = null, Vector2? origin = null, float? rotation = null, float? scale = null)
        {
            (Rectangle, Vector2) data = GetFramingData();
            if (sourceRect != null)
                data.Item1 = sourceRect.Value;
            if (origin != null)
                data.Item2 = origin.Value;
            Color drawColor = color == null ? GetAlpha() : color.Value;
            float drawRotation = rotation == null ? this.rotation : 0f;
            float drawScale = scale == null ? this.scale : 1f;

            spriteBatch.Draw(texture, position - offset, data.Item1, drawColor * opacity, drawRotation, data.Item2, drawScale, SpriteEffects.None, 0f);
        }
        public void DrawParticle(SpriteBatch spriteBatch)
        {
            Rectangle screen = canvas == ParticleDrawCanvas.UI ? new(0, 0, (int)(Main.screenWidth/Main.UIScale), (int)(Main.screenHeight/Main.UIScale)) : new((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight);
            int maxSize = (int)Math.Max((Texture.Height / ParticleSystem.particleFramesVertical[type]) * scale, (Texture.Width / ParticleSystem.particleFramesHorizontal[type]) * scale);
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