using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.Graphics.Shaders;
using ITD.DetoursIL;
using ITD.Systems.Extensions;
using ITD.Systems.DataStructures;
using Terraria.GameContent;
using ITD.Content.World;
using ITD.Particles.Misc;

namespace ITD.Particles
{
    public class ParticleSystem : DetourGroup // this also doubles as a particle loader
    {
        private static readonly List<ParticleEmitter> emitterPrototypes = [];
        public static byte[] particleFramesVertical = [];
        public static byte[] particleFramesHorizontal = [];
        public static bool[] particleUsesRenderTarget = [];
        private static readonly Dictionary<Type, ParticleEmitter> emmitersByType = [];
        public static ParticleEmitter currentlyDrawnEmitter;
        public List<ParticleEmitter> emitters;
        public static ParticlesRT particlesRT;
        public static ParticleSystem Instance => DetourManager.GetInstance<ParticleSystem>();
        public static ParticleEmitter NewEmitter<T>(ParticleEmitterDrawCanvas canvas = ParticleEmitterDrawCanvas.WorldOverProjectiles) where T : ParticleEmitter
        {
            Type particleType = typeof(T);
            if (emmitersByType.TryGetValue(particleType, out ParticleEmitter value))
            {
                var newInstance = Activator.CreateInstance<T>();

                newInstance.type = value.type;
                newInstance.canvas = canvas;
                if (!Main.dedServ)
                {
                    newInstance.ExpectedTexturePath = $"ITD/Particles/Textures/{particleType.Name}";
                }
                newInstance.Initialize();
                DetourManager.GetInstance<ParticleSystem>().emitters.Add(newInstance);
                return newInstance;
            }
            return null;
        }
        public static ParticleEmitter NewSingleParticle<T>(Vector2 position, Vector2 velocity, float rotation = 0f, short lifetime = 30, ParticleEmitterDrawCanvas canvas = ParticleEmitterDrawCanvas.WorldOverProjectiles) where T : ParticleEmitter
        {
            var emitter = NewEmitter<T>(canvas);
            emitter.Emit(position, velocity, rotation, lifetime);
            return emitter;
        }
        public void ClearParticlesOfType<T>() // this method must be accessed through ModContent.GetInstance<ParticleSystem>();
        {
            emitters.RemoveAll(x => x.GetType() == typeof(T));
        }
        public override void SetStaticDefaults()
        {
            foreach (var prototype in emitterPrototypes)
                prototype.SetStaticDefaults();
        }
        public override void Load()
        {
            if (Main.dedServ)
                return;
            Main.ContentThatNeedsRenderTargets.Add(particlesRT = new());
            emitters = [];
            foreach (Type t in ITD.Instance.Code.GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(ParticleEmitter)))) // particle loader
            {
                var instance = (ParticleEmitter)RuntimeHelpers.GetUninitializedObject(t);
                instance.type = (ushort)emitterPrototypes.Count;
                emmitersByType[t] = instance;

                emitterPrototypes.Add(instance);
            }
            Array.Resize(ref particleFramesVertical, emitterPrototypes.Count + 1);
            Array.Resize(ref particleFramesHorizontal, emitterPrototypes.Count + 1);
            Array.Resize(ref particleUsesRenderTarget, emitterPrototypes.Count + 1);
            foreach(var prototype in emitterPrototypes)
            {
                particleFramesVertical[prototype.type] = 1;
                particleFramesHorizontal[prototype.type] = 1;
                particleUsesRenderTarget[prototype.type] = false;
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
            Main.ContentThatNeedsRenderTargets.Remove(particlesRT);
            emitters?.Clear();
            emitterPrototypes?.Clear();
            emmitersByType?.Clear();
        }
        public void UpdateAllParticles(On_Main.orig_UpdateParticleSystems orig, Main self)
        {
            orig(self);
            // DEBUG LINE v

            // DEBUG LINE ^
            for (int i = emitters.Count - 1; i >= 0; i--)
            {
                ParticleEmitter emitter = emitters[i];
                emitter.UpdateAllParticles();
                emitter.timeLeft--;
                if (emitter.timeLeft < 0)
                {
                    if (currentlyDrawnEmitter != null)
                    {
                        if (currentlyDrawnEmitter.GetHashCode() == emitter.GetHashCode())
                            currentlyDrawnEmitter = null;
                    }
                    emitters.RemoveAt(i);
                }
            }
        }

        public void DrawParticles(ParticleEmitterDrawCanvas canvas)
        {
            foreach (ParticleEmitter emitter in emitters.Where(e => e.canvas == canvas))
            {
                currentlyDrawnEmitter = emitter;
                if (!particleUsesRenderTarget[currentlyDrawnEmitter.type])
                {
                    emitter.DrawFully();
                    continue;
                }
                particlesRT.Request();
                if (particlesRT.IsReady)
                    Main.spriteBatch.Draw(particlesRT.GetTarget(), Vector2.Zero, Color.White);
            }
        }
        
        public void DrawParticlesUnderProjectiles(On_Main.orig_DrawSuperSpecialProjectiles orig, Main self, List<int> projCache, bool startSpriteBatch)
        {
            orig(self, projCache, startSpriteBatch);
            if (!startSpriteBatch)
                Main.spriteBatch.End();

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            DrawParticles(ParticleEmitterDrawCanvas.WorldUnderProjectiles);

            Main.spriteBatch.End();

            if (!startSpriteBatch)
                Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);
        }
        public void DrawParticlesOverProjectiles(On_Main.orig_DrawCachedProjs orig, Main self, List<int> projCache, bool startSpriteBatch)
        {
            orig(self, projCache, startSpriteBatch);

            if (projCache != Main.instance.DrawCacheProjsOverPlayers)
                return;

            if (!startSpriteBatch)
                Main.spriteBatch.End();

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            DrawParticles(ParticleEmitterDrawCanvas.WorldOverProjectiles);

            Main.spriteBatch.End();

            if (!startSpriteBatch)
                Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);
        }
        public void DrawParticlesOnUI(On_Main.orig_DrawInterface orig, Main self, GameTime gameTime)
        {
            orig(self, gameTime);

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            DrawParticles(ParticleEmitterDrawCanvas.UI);

            Main.spriteBatch.End();
        }
    }
    public class ParticlesRT : ARenderTargetContentByRequest
    {
        protected override void HandleUseReqest(GraphicsDevice device, SpriteBatch spriteBatch)
        {
            PrepareARenderTarget_AndListenToEvents(ref _target, device, Main.screenWidth, Main.screenHeight, RenderTargetUsage.PreserveContents);

            var gd = spriteBatch.GraphicsDevice;
            var oldTargets = gd.GetRenderTargets();
            gd.SetRenderTarget(_target);
            gd.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            ParticleSystem.currentlyDrawnEmitter?.DrawFully();
            spriteBatch.End();
            gd.SetRenderTargets(oldTargets);
            _wasPrepared = true;
        }
    }
}