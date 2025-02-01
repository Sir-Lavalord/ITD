using ITD.Content.World;
using ITD.DetoursIL;
using ITD.Particles.MetaBalls;
using ITD.PrimitiveDrawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Initializers;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace ITD.Particles
{
    public abstract class Metaball : ModType
    {
        public Vector2 Position = Vector2.Zero;
        public Vector2 Velocity = Vector2.Zero;
        public int TimeLeft = 300;
        public SimpleSquare SimpleSquare = new SimpleSquare();
        public bool Active = true;
        public int Type {  get; private set; } 


        public static Metaball NewMetaball<T>( Vector2 Position, Vector2 Velocity, float Radius = default) where T : Metaball
        {
            var metaball = (Metaball)Activator.CreateInstance<T>();
            metaball.SetDefaults();
            metaball.Position = Position;
            metaball.Velocity = Velocity;
            MetaballSystem.Metaballs.Add(metaball);
            return metaball;
        }


        public sealed override void SetupContent()
        {
            MetaballSystem.Sets.ResizeArrays(MetaballSystem.MetaballInstances.Count + 1);
            SetStaticDefaults();
        }

        public virtual void SetDefaults() 
        {
            
        }

        public virtual void AI() 
        {

        }


        public virtual void Draw(SpriteBatch spriteBatch)
        {

        }

        protected sealed override void Register()
        {
            ModTypeLookup<Metaball>.Register(this);
            MetaballSystem.MetaballInstances.Add(this);
            Type = MetaballSystem.MetaballInstances.Count - 1;
            Main.ContentThatNeedsRenderTargets.Add(MetaballSystem.RT[Type] = new MetaballsRenderTarget(Type));
        }

        // utils stuff

        public void Clear() 
        {
        
        
            

        
        }

    }

    public class MetaballSystem : ModSystem 
    {
        public readonly static List<Metaball> Metaballs = new List<Metaball>();
        public readonly static List<Metaball> MetaballInstances = new List<Metaball>();
        public static Dictionary<int, MetaballsRenderTarget> RT = new Dictionary<int, MetaballsRenderTarget>();

        public override void Load()
        {
            On_Main.DrawDust += Draw;
            On_Main.DoUpdateInWorld += Update;
        }


        public static void Update(On_Main.orig_DoUpdateInWorld orig, Main self, Stopwatch sw)
        {
            orig(self, sw);

            //DEBUG
            if (WorldGenSystem.JustPressed(Microsoft.Xna.Framework.Input.Keys.D1))
                Metaball.NewMetaball<DebugMetaBall>(Main.MouseWorld, Vector2.Zero, 100);
            //DEBUG

            Metaballs.RemoveAll(b => !b.Active);

            for(int type = 0; type < MetaballInstances.Count; type++)
                for (int index = 0; index < Metaballs.Where(b => b.Type == type).Count(); index++)
                {

                    Metaballs[index].Position += Metaballs[index].Velocity;
                    Metaballs[index].TimeLeft--;

                    if (Metaballs[index].TimeLeft <= 0)
                        Metaballs[index].Active = false;

                    Metaballs[index].AI();
                }
        }
        private void Draw(On_Main.orig_DrawDust orig, Main self)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            for (int type = 0; type < MetaballInstances.Count; type++)
            {

                Vector2[] positions = new Vector2[50];

                for (int index = 0; index < Metaballs.Where(b => b.Type == type).Count() && index < positions.Length; index++) 
                {
                    positions[index] = (Metaballs.Where(b => b.Type == type).ElementAt(index).Position - Main.screenPosition) / Main.LastLoadedResolution.ToVector2();
                }

                // do we want to be setting shader properties every frame? especially since they're static anyway.
                // this would be better done after setstaticdefaults.
                // add something else just in case we actually want dynamic properties.
                // sorry if that's what you were planning on doing anyway :ech:
                // -q
                ITD.ITDMetaBallsShaders["Metaballs"].SetProperties(Sets.Color[type], Sets.OutlineColor[type], Sets.OutlineThickness[type], TextureAssets.Extra[87], positions, Metaballs.Where(b => b.Type == type).Count());
                ITD.ITDMetaBallsShaders["Metaballs"].apply();

                RT[type].Request();
                if (RT[type].IsReady)
                {

                    spriteBatch.Draw(RT[type].GetTarget(), Vector2.One, Color.White);

                }

                Main.pixelShader.CurrentTechnique.Passes[0].Apply();


            }

            spriteBatch.End();
            orig(self);

        }


        public static Metaball GetMetaball(int Type)
        {
            return Type >= 0 && Type < MetaballInstances.Count ? MetaballInstances[Type] : null;
        }

        public static class Sets
        {

            public enum MetaballType : byte
            {

                Fake = 0,
                Real = 1

            }

            public static void ResizeArrays(int count)
            {
                Array.Resize(ref OutlineColor, count);
                Array.Resize(ref Color, count);
                Array.Resize(ref OutlineThickness, count);
                Array.Resize(ref Type, count);

            }

            public static Color[] OutlineColor = [];
            public static Color[] Color = [];
            public static float[] OutlineThickness = [];
            public static MetaballType[] Type = [];

        }
    }

    public class MetaballsRenderTarget : ARenderTargetContentByRequest
    {
        public int MetaBallsType;
        public MetaballsRenderTarget(int MetaBallsType) 
        {
            this.MetaBallsType = MetaBallsType;
            Main.OnResolutionChanged += OnResChanged;
        }

        protected override void HandleUseReqest(GraphicsDevice device, SpriteBatch spriteBatch)
        {

            PrepareARenderTarget_AndListenToEvents(ref _target, device, Main.screenWidth, Main.screenHeight, RenderTargetUsage.PreserveContents);
            var oldTargets = device.GetRenderTargets();
            device.SetRenderTarget(_target);
            device.Clear(Color.Transparent);

            spriteBatch.Begin();


            DrawBalls(spriteBatch);
            spriteBatch.End();

            device.SetRenderTarget(null);
            _wasPrepared = true;
            device.SetRenderTargets(oldTargets);
        }

        private void OnResChanged(Vector2 vector)
        {
            Reset();
        }

 
        public void DrawBalls(SpriteBatch spriteBatch)
        {
            foreach (Metaball ball in MetaballSystem.Metaballs.Where(b => b.Type == MetaBallsType)) 
                ball.Draw(spriteBatch);
        }
    }

    public class MetaballShaderData : IDisposable
    {
        static GraphicsDevice GraphicsDevice => Main.instance.GraphicsDevice;
        public Asset<Effect> _effect;
        Color _color = new Color(0, 0, 0);
        Asset<Texture2D> _texutre1 = null;
        Asset<Texture2D> _texutre2 = null;
        Asset<Texture2D> _texutre3 = null;
        Vector4 _shaderData = new Vector4(0, 0, 0, 0);
        Color _outlineColor = new Color(0, 0,0);
        float _outlineThickness = 15;
        public bool enabled = false;
        Texture2D screenTexture;
        Vector2[] _positions = new Vector2[50];
        int _amount = 0;
        public MetaballShaderData(Asset<Effect> effect)
        {

            this._effect = effect;

        }
        public void SetProperties(Color color,Color outlineColor , float outlineThickness, Asset<Texture2D> baseTexture, Vector2[] positions, int amount ,Vector4 shaderData = default)
        {
            this._color = color;
            this._outlineColor = outlineColor;
            this._outlineThickness = outlineThickness;
            this._shaderData = shaderData;
            _texutre1 = baseTexture;
            _positions = positions;
            _amount = amount;
        }

        public void SetupTextures()
        {


            if (_texutre1 != null)
            {
                GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
                GraphicsDevice.Textures[1] = _texutre1.Value;
            }
            if (_texutre2 != null)
            {
                GraphicsDevice.SamplerStates[2] = SamplerState.LinearWrap;
                GraphicsDevice.Textures[2] = _texutre2.Value;
            }
            if (_texutre3 != null)
            {
                GraphicsDevice.SamplerStates[3] = SamplerState.LinearWrap;
                GraphicsDevice.Textures[3] = _texutre3.Value;
            }
        }
        public void apply()
        {
            
            var viewport = GraphicsDevice.Viewport;
            Effect effect = _effect.Value;
            SetupTextures();
            effect.Parameters["viewWorldProjection"].SetValue(Matrix.CreateTranslation(new Vector3(-Main.screenPosition, 0)) * Main.GameViewMatrix.TransformationMatrix * Matrix.CreateOrthographicOffCenter(left: 0, right: viewport.Width, bottom: viewport.Height, top: 0, zNearPlane: -1, zFarPlane: 10));
            effect.Parameters["time"].SetValue(Main.GlobalTimeWrappedHourly);
            effect.Parameters["color"].SetValue(_color.ToVector3());
            effect.Parameters["shaderData"].SetValue(_shaderData);
            effect.Parameters["screenPosition"].SetValue(Main.screenPosition);
            effect.Parameters["screenResolution"].SetValue(Main.LastLoadedResolution.ToVector2());
            effect.Parameters["outlineColor"].SetValue(_outlineColor.ToVector3());
            effect.Parameters["outlineThickness"].SetValue(_outlineThickness);
            effect.Parameters["positions"].SetValue(_positions);
            effect.Parameters["amount"].SetValue(_amount);

            effect.CurrentTechnique.Passes[0].Apply();

        }

        public void Dispose()
        {
            Main.RunOnMainThread(() => {

                _effect.Dispose();

            }).Wait();
        }
    }
}
