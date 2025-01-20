using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;

namespace ITD.Particles
{
    public enum ParticleEmitterDrawCanvas : byte
    {
        WorldUnderProjectiles,
        WorldOverProjectiles,
        UI
    }
    public enum ParticleEmitterDrawStep : byte
    {
        BeforePreDrawAll,
        AfterPreDrawAll,
        AfterDrawAll,
    }
    public readonly record struct ParticleEmitterDrawAction(ParticleEmitterDrawStep Step, Action Action);
    public abstract class ParticleEmitter : IDisposable
    {
        internal ushort type;
        internal Texture2D Texture { get { return ModContent.Request<Texture2D>(TexturePath).Value; } }
        /// <summary>
        /// this is the default texture path that points to Particles/Textures/nameof(this). this cannot be overriden but you can override TexturePath to change the texture path
        /// </summary>
        internal string ExpectedTexturePath;
        public virtual string TexturePath => ExpectedTexturePath;
        public byte frameVertical;
        public byte frameHorizontal;
        public byte frameCounter;
        public short timeLeft;
        public bool keptAlive;
        public bool additive = false;
        public ParticleEmitterDrawCanvas canvas;
        private readonly List<ParticleEmitterDrawAction> drawActions = [];
        public List<ITDParticle> particles = [];
        public object tag;
        public void InjectDrawAction(ParticleEmitterDrawStep step, Action action)
        {
            drawActions.Add(new(step, action));
        }
        public void FlushDrawActions(ParticleEmitterDrawStep step)
        {
            for (int i = drawActions.Count - 1; i >= 0; i--)
            {
                ParticleEmitterDrawAction action = drawActions[i];
                if (action.Step != step)
                    continue;
                action.Action();
                drawActions.RemoveAt(i);
            }
        }
        public virtual void SetStaticDefaults()
        {

        }
        public ParticleFramingData GetFramingData()
        {
            int framesVertical = ParticleSystem.particleFramesVertical[type];
            int framesHorizontal = ParticleSystem.particleFramesHorizontal[type];
            int frameHeight = Texture.Height / framesVertical;
            int frameWidth = Texture.Width / framesHorizontal;
            return new(new Rectangle(frameWidth * frameHorizontal, frameHeight * frameVertical, frameWidth, frameHeight), new Vector2(frameWidth * 0.5f, frameHeight * 0.5f));
        }
        public Vector2 CanvasOffset => canvas == ParticleEmitterDrawCanvas.UI ? Vector2.Zero : Main.screenPosition;
        public virtual Color GetAlpha(ITDParticle particle)
        {
            return Lighting.GetColor(particle.position.ToTileCoordinates());
        }
        public virtual void Initialize()
        {

        }
        /// <summary>
        /// Passes in a reference to the particle after it is emitted, but before <see cref="ITDParticle.spawnParameters"/> are set and it is put into the <see cref="particles"/> list.
        /// </summary>
        /// <param name="particleIndex"></param>
        /// <param name="particle"></param>
        public virtual void OnEmitParticle(ref ITDParticle particle)
        {

        }
        /// <summary>
        /// Emits a particle with the given parameters.
        /// For further modification, override OnEmitParticle for this Emitter type.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="velocity"></param>
        /// <param name="rotation"></param>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        public int Emit(Vector2 position, Vector2 velocity, float rotation = 0f, short lifetime = 30)
        {
            ITDParticle particle = new()
            {
                position = position,
                velocity = velocity,
                rotation = rotation,
                timeLeft = lifetime,
            };
            int index = particles.Count;
            OnEmitParticle(ref particle);
            particle.spawnParameters = new(lifetime, particle.scale);
            particle.parent = this;
            particles.Add(particle);
            return index;
        }
        /// <summary>
        /// <para>This handles all particle update operations. Override this to stop the normal behavior or add extra steps to the behavior.</para>
        /// For defining AI, override the AI method, it passes in a reference to the particle you can modify freely without needing reassignment.
        /// </summary>
        public virtual void UpdateAllParticles()
        {
            if (keptAlive)
            {
                timeLeft = 2;
                keptAlive = false;
            }
            else
            {
                if (particles.Count > 0)
                    timeLeft = Math.Max(timeLeft, particles.Max(p => p.timeLeft));
            }
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                ITDParticle p = particles[i];
                AI(ref p);
                p.position += p.velocity;
                p.rotation += p.angularVelocity;
                p.timeLeft--;
                particles[i] = p;
                if (particles[i].timeLeft <= 0)
                    particles.RemoveAt(i);
            }
        }
        public virtual void AI(ref ITDParticle particle)
        {

        }
        public virtual void PreDrawAllParticles()
        {

        }
        public virtual void DrawAllParticles()
        {
            SpriteBatch sb = Main.spriteBatch;
            for (int i = 0; i < particles.Count; i++)
            {
                // since we don't do actual operations here, we don't have to reassign the value.
                ITDParticle p = particles[i];
                p.DrawCommon(sb, Texture, CanvasOffset);
            }
        }
        public void Dispose()
        {
            tag = null;
            drawActions.Clear();
            particles.Clear();
            particles = null;
        }
    }
}
