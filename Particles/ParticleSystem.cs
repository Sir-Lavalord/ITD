using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Particles
{
    public class ParticleSystem : ModSystem // this also doubles as a particle loader
    {
        private static readonly List<ITDParticle> particlePrototypes = [];
        private static readonly Dictionary<uint, ITDParticle> particlesByID = [];
        public static int[] particleFramesVertical = [];
        public static int[] particleFramesHorizontal = [];
        private static readonly Dictionary<Type, ITDParticle> particlesByType = [];
        public List<ITDParticle> particles;
        public static uint ParticleType<T>() where T : ITDParticle
        {
            var type = typeof(T);
            if (particlesByType.TryGetValue(type, out ITDParticle value))
            {
                return value.type;
            }
            return 0;
        }
        public static ITDParticle NewParticle(uint type, Vector2 position, Vector2 velocity)
        {
            if (particlesByID.TryGetValue(type, out ITDParticle value))
            {
                var particleTemplate = value;
                var particleType = particleTemplate.GetType();

                // which is best? i'm kinda dumb when it comes to reflection. this is the first option:
                /*
                var newInstance = Activator.CreateInstance(particleType, position, velocity);
                if (newInstance != null)
                {
                    ITDParticle particleInstance = newInstance as ITDParticle;
                    particleInstance.type = type;
                    ModContent.GetInstance<ParticleSystem>().particles.Add(particleInstance);
                    return particleInstance;
                }
                */

                // second option:
                var constructor = particleType.GetConstructor(Type.EmptyTypes);
                if (constructor != null)
                {
                    var newInstance = (ITDParticle)constructor.Invoke(null);
                    newInstance.type = type;
                    if (!Main.dedServ)
                        newInstance.texture = ModContent.Request<Texture2D>($"ITD/Particles/Textures/{particleType.Name}").Value;
                    newInstance.Initialize();
                    newInstance.position = position;
                    newInstance.velocity = velocity;
                    ModContent.GetInstance<ParticleSystem>().particles.Add(newInstance);
                    return newInstance;
                }
                // we need to make sure this isn't crappy for ease of use, and possibly performance?
            }
            return null;
        }
        public override void Load()
        {
            particles = [];
            foreach (Type t in Mod.Code.GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(ITDParticle)))) // particle loader
            {
                var instance = (ITDParticle)RuntimeHelpers.GetUninitializedObject(t);
                instance.type = (uint)particlePrototypes.Count;
                particlesByType[t] = instance;
                particlesByID[instance.type] = instance;

                particlePrototypes.Add(instance);
            }
            Array.Resize(ref particleFramesVertical, particlePrototypes.Count + 1);
            Array.Resize(ref particleFramesHorizontal, particlePrototypes.Count + 1);
            foreach (ITDParticle prototype in particlePrototypes)
            {
                particleFramesVertical[prototype.type] = 1;
                particleFramesHorizontal[prototype.type] = 1;
                prototype.SetStaticDefaults();
            }
            On_Main.DrawSuperSpecialProjectiles += DrawParticlesUnderProjectiles; // subscribe to events for drawing
            On_Main.DrawCachedProjs += DrawParticlesOverProjectiles;
            On_Main.UpdateParticleSystems += UpdateAllParticles;
        }
        public override void Unload()
        {
            On_Main.DrawSuperSpecialProjectiles -= DrawParticlesUnderProjectiles; // unsubscribe from events on unload
            On_Main.DrawCachedProjs -= DrawParticlesOverProjectiles;
            On_Main.UpdateParticleSystems -= UpdateAllParticles;
            particles?.Clear();
            particlePrototypes?.Clear();
            particlesByType?.Clear();
            particlesByID?.Clear();
        }
        public void UpdateAllParticles(On_Main.orig_UpdateParticleSystems orig, Main self)
        {
            orig(self);
            foreach (ITDParticle particle in particles.ToList()) // avoid particle deletion funkiness by cloning the particles list (better way to do this?)
            {
                particle.Update();
            }
        }
        public void DrawParticlesUnderProjectiles(On_Main.orig_DrawSuperSpecialProjectiles orig, Main self, List<int> projCache, bool startSpriteBatch)
        {
            orig(self, projCache, startSpriteBatch);
            if (!startSpriteBatch)
                Main.spriteBatch.End();

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.EffectMatrix);

            foreach (ITDParticle particle in particles.Where(p => p.layer == WorldParticleDrawLayer.UnderProjectiles))
            {
                particle.DrawParticle(Main.spriteBatch);
            }

            Main.spriteBatch.End();

            if (!startSpriteBatch)
                Main.spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);
        }
        public void DrawParticlesOverProjectiles(On_Main.orig_DrawCachedProjs orig, Main self, List<int> projCache, bool startSpriteBatch)
        {
            orig(self, projCache, startSpriteBatch);
            if (!startSpriteBatch)
                Main.spriteBatch.End();

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.EffectMatrix);

            foreach (ITDParticle particle in particles.Where(p => p.layer == WorldParticleDrawLayer.OverProjectiles))
            {
                particle.DrawParticle(Main.spriteBatch);
            }

            Main.spriteBatch.End();

            if (!startSpriteBatch)
                Main.spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
