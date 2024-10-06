﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ID;
using ITD.Detours;

namespace ITD.Particles
{
    public class ParticleSystem : DetourGroup // this also doubles as a particle loader
    {
        private static readonly List<ITDParticle> particlePrototypes = [];
        private static readonly Dictionary<uint, ITDParticle> particlesByID = [];
        public static int[] particleFramesVertical = [];
        public static int[] particleFramesHorizontal = [];
        public static ArmorShaderData[] particleShaders = [];
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

                var constructor = particleType.GetConstructor(Type.EmptyTypes);
                if (constructor != null)
                {
                    var newInstance = (ITDParticle)constructor.Invoke(null);
                    newInstance.type = type;
                    if (!Main.dedServ)
                    {
                        newInstance.texture = ModContent.Request<Texture2D>($"ITD/Particles/Textures/{particleType.Name}").Value;
                        //newInstance.shader = value.shader;
                    }
                    newInstance.Initialize();
                    newInstance.position = position;
                    newInstance.velocity = velocity;
                    DetourManager.GetInstance<ParticleSystem>().particles.Add(newInstance);
                    return newInstance;
                }
            }
            return null;
        }
        public void ClearParticlesOfType(uint type) // this method must be accessed through ModContent.GetInstance<ParticleSystem>();
        {
            particles.RemoveAll(x => x.type == type);
        }
        public override void Load()
        {
            if (Main.dedServ)
                return;
            particles = [];
            foreach (Type t in ITD.Instance.Code.GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(ITDParticle)))) // particle loader
            {
                var instance = (ITDParticle)RuntimeHelpers.GetUninitializedObject(t);
                instance.type = (uint)particlePrototypes.Count;
                particlesByType[t] = instance;
                particlesByID[instance.type] = instance;

                particlePrototypes.Add(instance);
            }
            Array.Resize(ref particleFramesVertical, particlePrototypes.Count + 1);
            Array.Resize(ref particleFramesHorizontal, particlePrototypes.Count + 1);
            Array.Resize(ref particleShaders, particlePrototypes.Count + 1);
            foreach (ITDParticle prototype in particlePrototypes)
            {
                particleFramesVertical[prototype.type] = 1;
                particleFramesHorizontal[prototype.type] = 1;
                particleShaders[prototype.type] = null;
                prototype.SetStaticDefaults();
            }
            On_Main.DrawSuperSpecialProjectiles += DrawParticlesUnderProjectiles; // subscribe to events for drawing
            On_Main.DrawCachedProjs += DrawParticlesOverProjectiles;
            On_Main.DrawInterface += DrawParticlesOnUI;
            On_Main.UpdateParticleSystems += UpdateAllParticles;
        }
        public override void Unload()
        {
            if (Main.dedServ)
                return;
            On_Main.DrawSuperSpecialProjectiles -= DrawParticlesUnderProjectiles; // unsubscribe from events on unload
            On_Main.DrawCachedProjs -= DrawParticlesOverProjectiles;
            On_Main.DrawInterface -= DrawParticlesOnUI;
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
        
        public void DrawParticles(ParticleDrawCanvas canvas)
        {
            Matrix transform = canvas == ParticleDrawCanvas.UI ? Main.UIScaleMatrix : Main.GameViewMatrix.TransformationMatrix;

            var particlesByType = particles
                .Where(p => p.canvas == canvas)
                .GroupBy(p => p.type);

            foreach (var particleGroup in particlesByType)
            {
                uint particleType = particleGroup.Key;
                var shader = particleShaders[particleType];
                
                if (shader != null)
                {
                    Main.spriteBatch.End();

                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, transform);
                }

                foreach (var particle in particleGroup)
                {
                    if (shader != null)
                    {
                        (Rectangle source, Vector2 origin) = particle.GetFramingData();
                        //shader.Apply(null, new DrawData(particle.texture, particle.position, source, Color.White, particle.rotation, origin, particle.scale, SpriteEffects.None));
                        shader.Apply(null, null);
                    }
                    particle.DrawParticle(Main.spriteBatch);
                }
                if (shader != null)
                {
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, RasterizerState.CullNone, default, transform);
                }
            }
        }
        
        public void DrawParticlesUnderProjectiles(On_Main.orig_DrawSuperSpecialProjectiles orig, Main self, List<int> projCache, bool startSpriteBatch)
        {
            orig(self, projCache, startSpriteBatch);
            if (!startSpriteBatch)
                Main.spriteBatch.End();

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            DrawParticles(ParticleDrawCanvas.WorldUnderProjectiles);

            Main.spriteBatch.End();

            if (!startSpriteBatch)
                Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);
        }
        public void DrawParticlesOverProjectiles(On_Main.orig_DrawCachedProjs orig, Main self, List<int> projCache, bool startSpriteBatch)
        {
            orig(self, projCache, startSpriteBatch);
            if (!startSpriteBatch)
                Main.spriteBatch.End();

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            DrawParticles(ParticleDrawCanvas.WorldOverProjectiles);

            Main.spriteBatch.End();

            if (!startSpriteBatch)
                Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);
        }
        public void DrawParticlesOnUI(On_Main.orig_DrawInterface orig, Main self, GameTime gameTime)
        {
            orig(self, gameTime);

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            DrawParticles(ParticleDrawCanvas.UI);

            Main.spriteBatch.End();
        }
    }
}