using Daybreak.Common.Features.Hooks;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ITD.Particles;

public static class ParticleSystem
{
    private static readonly HashSet<ParticleEmitter> emitterPrototypes = [];
    public static byte[] particleFramesVertical = [];
    public static byte[] particleFramesHorizontal = [];
    public static bool[] particleUsesRenderTarget = [];
    private static readonly Dictionary<Type, ParticleEmitter> emittersByType = [];
    public static ParticleEmitter currentlyDrawnEmitter;
    public static List<ParticleEmitter> emitters = [];
    //public static ParticlesRT particlesRT;
    public static T NewEmitter<T>(ParticleEmitterDrawCanvas canvas = ParticleEmitterDrawCanvas.WorldOverProjectiles) where T : ParticleEmitter, new()
    {
        Type particleType = typeof(T);
        T newInstance = new()
        {
            canvas = canvas,
            type = emittersByType[particleType].type,
            ExpectedTexturePath = Main.dedServ ? null : $"ITD/Particles/Textures/{particleType.Name}"
        };
        emitters.Add(newInstance);
        return newInstance;
    }
    public static ParticleEmitter NewSingleParticle<T>(Vector2 position, Vector2 velocity, float rotation = 0f, short lifetime = 30, ParticleEmitterDrawCanvas canvas = ParticleEmitterDrawCanvas.WorldOverProjectiles) where T : ParticleEmitter, new()
    {
        var emitter = NewEmitter<T>(canvas);
        emitter.Emit(position, velocity, rotation, lifetime);
        return emitter;
    }
    public static void ClearParticlesOfType<T>()
    {
        emitters.RemoveAll(x => x.GetType() == typeof(T));
    }
    public static ushort RegisterEmitter(ParticleEmitter instance)
    {
        ushort newID = (ushort)emitterPrototypes.Count;
        Type t = instance.GetType();
        emittersByType[t] = instance;
        emitterPrototypes.Add(instance);
        return newID;
    }
    public static void ResizeArrays()
    {
        Array.Resize(ref particleFramesVertical, emitterPrototypes.Count + 1);
        Array.Resize(ref particleFramesHorizontal, emitterPrototypes.Count + 1);
        Array.Resize(ref particleUsesRenderTarget, emitterPrototypes.Count + 1);
    }
    public static void DefaultStaticValues(ushort type)
    {
        particleFramesVertical[type] = 1;
        particleFramesHorizontal[type] = 1;
        particleUsesRenderTarget[type] = false;
    }
    [OnLoad(Side = ModSide.Client)]
    public static void Load()
    {
        //Main.ContentThatNeedsRenderTargets.Add(particlesRT = new());
        On_Main.DrawSuperSpecialProjectiles += DrawParticlesUnderProjectiles; // subscribe to events for drawing
        On_Main.DrawCachedProjs += DrawParticlesOverProjectiles;
        On_Main.DrawInterface += DrawParticlesOnUI;
        On_Main.UpdateParticleSystems += UpdateAllParticles;
    }
    [OnUnload(Side = ModSide.Client)]
    public static void Unload()
    {
        if (Main.dedServ)
            return;
        //Main.ContentThatNeedsRenderTargets.Remove(particlesRT);
        emitters?.Clear();
        emitterPrototypes?.Clear();
        emittersByType?.Clear();
    }
    private static void UpdateAllParticles(On_Main.orig_UpdateParticleSystems orig, Main self)
    {
        orig(self);
        for (int i = emitters.Count - 1; i >= 0; i--)
        {
            ParticleEmitter emitter = emitters[i];
            emitter.UpdateAllParticles();
            emitter.timeLeft--;
            if (emitter.timeLeft < 0)
            {
                /*
                if (currentlyDrawnEmitter != null)
                {
                    if (currentlyDrawnEmitter.GetHashCode() == emitter.GetHashCode())
                        currentlyDrawnEmitter = null;
                }
                */
                emitters.RemoveAt(i);
            }
        }
    }

    private static void DrawParticles(ParticleEmitterDrawCanvas canvas)
    {
        foreach (ParticleEmitter p in CollectionsMarshal.AsSpan(emitters))
        {
            if (p.canvas == canvas)
                p.DrawFully();
        }
        /*
        for (int i = 0; i < emitters.Count; i++)
        {
            ParticleEmitter p = emitters[i];
            if (p.canvas != canvas)
                continue;
            p.DrawFully();
            /*
            currentlyDrawnEmitter = emitter;
            if (!particleUsesRenderTarget[currentlyDrawnEmitter.type])
            {
                emitter.DrawFully();
                continue;
            }
            particlesRT.Request();
            if (particlesRT.IsReady)
                Main.spriteBatch.Draw(particlesRT.GetTarget(), Vector2.Zero, Color.White);
            //
        }
        */
    }

    private static void DrawParticlesUnderProjectiles(On_Main.orig_DrawSuperSpecialProjectiles orig, Main self, List<int> projCache, bool startSpriteBatch)
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
    private static void DrawParticlesOverProjectiles(On_Main.orig_DrawCachedProjs orig, Main self, List<int> projCache, bool startSpriteBatch)
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
    private static void DrawParticlesOnUI(On_Main.orig_DrawInterface orig, Main self, GameTime gameTime)
    {
        orig(self, gameTime);

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
            DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

        DrawParticles(ParticleEmitterDrawCanvas.UI);

        Main.spriteBatch.End();
    }
}
/*
public class ParticlesRT : ARenderTargetContentByRequest
{
    public override void HandleUseReqest(GraphicsDevice device, SpriteBatch spriteBatch)
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
*/