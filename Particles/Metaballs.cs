using ITD.PrimitiveDrawing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using static ITD.Particles.MetaballSystem.Sets;

// HEADS UP: this code kinda sucks because its my first time making a metaball + fakemetaball system -John Shader
// so basically how this works is that each metaball Type Has its own 2 to 3 RTs (based on Metaball.Sets.Type) that covers the entire screen,
// each RT applies a different type of shader based on MetaballSystem.Sets.Type,
// those RTs gets drawn only when atleast one Object of its Metaball Type exists inside the world.
// shader types are as follows:
// 1. Outline shader (Fake Metaball): based on the amount Setter, draw X copies of its self with the color of the outlineColor Setter
// 2. Real Metaball: draws a metaball at the Metaball Object position, Miscshader Setter applies a custom shader to it

namespace ITD.Particles
{
    public abstract class Metaball : ModType
    {
        public Vector2 Position = Vector2.Zero;
        public Vector2 Velocity = Vector2.Zero;
        /// <summary>
        /// set this MaxTimeLeft instead of this
        /// </summary>
        public int TimeLeft = 300;
        /// <summary>
        /// set this instead of TimeLeft
        /// </summary>
        public int MaxTimeLeft;
        public SimpleSquare SimpleSquare = new SimpleSquare();
        public bool Active = true;
        public float Size;
        public int Type {  get; private set; } 


        public static Metaball NewMetaball<T>( Vector2 Position, Vector2 Velocity, float Radius = 3, int TimeLeft = 300) where T : Metaball
        {
            var metaball = (Metaball)Activator.CreateInstance<T>();
            metaball.SetDefaults();
            metaball.Position = Position;
            metaball.Velocity = Velocity;
            metaball.Size = Radius;
            metaball.TimeLeft = TimeLeft;
            metaball.MaxTimeLeft = TimeLeft;
            MetaballSystem.Metaballs.Add(metaball);
            return metaball;
        }


        public sealed override void SetupContent()
        {
            MetaballSystem.Sets.ResizeArrays(MetaballSystem.MetaballInstances.Count + 1);
            if (Main.dedServ)
                return;
            SetStaticDefaults();
        }

        public virtual void SetDefaults() 
        {
            
        }

        public virtual void AI() 
        {

        }

        /// <summary>
        /// use this to draw things that will be ignored by the misc shader
        /// </summary>
        /// <param name="spriteBatch"></param>
        public virtual void PostDraw(SpriteBatch spriteBatch)
        {

        }
        /// <summary>
        /// use this to draw things that the misc shader will apply to
        /// </summary>
        /// <param name="spriteBatch"></param>
        public virtual void PreDraw(SpriteBatch spriteBatch)
        {

        }
        protected sealed override void Register()
        {
            ModTypeLookup<Metaball>.Register(this);
            MetaballSystem.MetaballInstances.Add(this);
            Type = MetaballSystem.MetaballInstances.Count - 1;
            Main.ContentThatNeedsRenderTargets.Add(MetaballSystem.MetaballRT[Type] = new MetaballsRenderTarget(Type));
            Main.ContentThatNeedsRenderTargets.Add(MetaballSystem.DrawingRT[Type] = new MetaballsNormalDrawingRenderTarget(Type));
            Main.ContentThatNeedsRenderTargets.Add(MetaballSystem.MiscShaderRT[Type] = new MetaballsMiscShaderRenderTarget(Type));

        }

    }

    public class MetaballSystem : ModSystem 
    {
        public readonly static List<Metaball> Metaballs = new List<Metaball>();
        public readonly static List<Metaball> MetaballInstances = new List<Metaball>();
        public static Dictionary<int, MetaballsRenderTarget> MetaballRT = new Dictionary<int, MetaballsRenderTarget>();
        public static Dictionary<int, MetaballsNormalDrawingRenderTarget> DrawingRT = new Dictionary<int, MetaballsNormalDrawingRenderTarget>();
        public static Dictionary<int, MetaballsMiscShaderRenderTarget> MiscShaderRT = new Dictionary<int, MetaballsMiscShaderRenderTarget>();

        public static Dictionary<int, Vector2[]> RealMetaballPositions = new Dictionary<int, Vector2[]>();
        public static Dictionary<int, float[]> RealMetaballSizes = new Dictionary<int, float[]>();

        public override void Load()
        {
            On_Main.DrawDust += Draw;
            On_Main.DoUpdateInWorld += Update;
        }


        private void Draw(On_Main.orig_DrawDust orig, Main self)
        {


            orig(self);

            SpriteBatch spriteBatch = Main.spriteBatch;
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
            // ensure that a metaball exists ingame before drawing its RTs
            bool aMetaballExists = false;

            for (int type = 0; type < MetaballInstances.Count; type++)
            {

                for (int index = 0; index < Metaballs.Count; index++)
                {
                    if (Metaballs[index].Type == type) 
                    {
                        aMetaballExists = true;
                        break;

                    }
                }

                if (!aMetaballExists)
                    continue;

                if (Sets.Type[type] == MetaballType.Fake || Sets.Type[type] == MetaballType.Both)
                {

                    ITD.ITDMetaBallsShaders["FakeMetaballs"].SetProperties(Sets.MainColor[type], Sets.OutlineColor[type], Sets.radius[type], [MetaballSystem.Sets.Image0[type], MetaballSystem.Sets.Image1[type], MetaballSystem.Sets.Image2[type]], RealMetaballPositions[type], MetaballSystem.Sets.outlineAmount[type], new Vector2(512), MetaballSystem.RealMetaballSizes[type]);
                    ITD.ITDMetaBallsShaders["FakeMetaballs"].apply();

                }

                MiscShaderRT[type].Request();
                if (MiscShaderRT[type].IsReady)
                    spriteBatch.Draw(MiscShaderRT[type].GetTarget(), Vector2.One, Color.Transparent);

                Main.pixelShader.CurrentTechnique.Passes[0].Apply();
            }
            spriteBatch.End();
        }

        public static void Update(On_Main.orig_DoUpdateInWorld orig, Main self, Stopwatch sw)
        {
            orig(self, sw);

            //DEBUG
            /*
            if (WorldGenSystem.JustPressed(Microsoft.Xna.Framework.Input.Keys.D1))
                Metaball.NewMetaball<CosmicMetaball>(Main.MouseWorld, Vector2.Zero, 0.1f, 300);
            //DEBUG
            */
            Metaballs.RemoveAll(b => b.TimeLeft <= 0);

            for (int type = 0; type < MetaballInstances.Count; type++) 
            {
                // real metaballs caps at 50 because of shader limitations
                Vector2[] positions = new Vector2[50];
                float[] sizes = new float[50];

                IEnumerable<Metaball> ballsOfType = Metaballs.Where(b => b.Type == type);

                for (int index = 0; index < ballsOfType.Count() - 1; index++)
                {

                    if (index < 50)
                    {
                        positions[index] = (ballsOfType.ElementAt(index).Position - Main.screenPosition) / new Vector2(Main.screenWidth, Main.screenHeight);
                        sizes[index] = (ballsOfType.ElementAt(index)).Size;
                    }
                    else if (Sets.Type[type] == MetaballType.Both || Sets.Type[type] == MetaballType.Real)
                    {
                        Metaball oldest = ballsOfType.First();
                        for (int i = 0; i < ballsOfType.Count(); i++)
                        {
                            if (ballsOfType.ElementAt(i).TimeLeft < oldest.TimeLeft)
                                oldest = Metaballs.ElementAt(i);
                        }

                        Metaballs.Remove(oldest);
                    }

                    

                    Metaballs[index].Position += Metaballs[index].Velocity;
                    Metaballs[index].TimeLeft--;


                    if (Metaballs[index].TimeLeft <= 0)
                        Metaballs[index].Active = false;

                    Metaballs[index].AI();
                }
                RealMetaballPositions[type] = positions;
                RealMetaballSizes[type] = sizes;

            }
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
                Real = 1,
                Both = 2,
            }
            public static void ResizeArrays(int count)
            {
                Array.Resize(ref OutlineColor, count);
                Array.Resize(ref MainColor, count);
                Array.Resize(ref OutlineThickness, count);
                Array.Resize(ref Type, count);
                Array.Resize(ref MiscShader, count);
                Array.Resize(ref Image0, count);
                Array.Resize(ref Image1, count);
                Array.Resize(ref Image2, count);
                Array.Resize(ref outlineAmount, count);
                Array.Resize(ref radius, count);
            }

            public static Color[] OutlineColor = [];
            public static Color[] MainColor = [];
            public static float[] OutlineThickness = [];
            public static MetaballType[] Type = [];
            public static MiscShaderData[] MiscShader = [];
            public static Asset<Texture2D>[] Image0 = [];
            public static Asset<Texture2D>[] Image1 = [];
            public static Asset<Texture2D>[] Image2 = [];
            public static int[] outlineAmount = [];
            public static float[] radius = [];




        }
    }
    /// <summary>
    /// Draw the metaball content with a different RT to allow shaders effects
    /// </summary>
    public class MetaballsNormalDrawingRenderTarget : ARenderTargetContentByRequest
    {
        public int MetaBallsType;
        public SimpleSquare simpleSquare = new SimpleSquare();
        public MetaballsNormalDrawingRenderTarget(int MetaBallsType)
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

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            foreach (Metaball ball in MetaballSystem.Metaballs.Where(b => b.Type == MetaBallsType))
            {

                ball.PreDraw(spriteBatch);
                ball.PostDraw(spriteBatch);

            }

            spriteBatch.End();

            device.SetRenderTarget(null);
            _wasPrepared = true;
            device.SetRenderTargets(oldTargets);
        }

        private void OnResChanged(Vector2 vector)
        {
            Reset();
        }


    }
    public class MetaballsMiscShaderRenderTarget : ARenderTargetContentByRequest
    {
        public int MetaBallsType;
        public MetaballsMiscShaderRenderTarget(int MetaBallsType)
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

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            if (MetaballSystem.Sets.MiscShader[MetaBallsType] != null)
            {


                MetaballSystem.Sets.MiscShader[MetaBallsType].UseColor(MetaballSystem.Sets.MainColor[MetaBallsType]);
                MetaballSystem.Sets.MiscShader[MetaBallsType].UseSecondaryColor(MetaballSystem.Sets.OutlineColor[MetaBallsType]);
                MetaballSystem.Sets.MiscShader[MetaBallsType].UseImage1(MetaballSystem.Sets.Image1[MetaBallsType]);
                MetaballSystem.Sets.MiscShader[MetaBallsType].UseImage2(MetaballSystem.Sets.Image2[MetaBallsType]);

                MetaballSystem.Sets.MiscShader[MetaBallsType].Apply();

            }

            if (MetaballSystem.Sets.Type[MetaBallsType] == MetaballType.Real || MetaballSystem.Sets.Type[MetaBallsType] == MetaballType.Both) 
            {
                MetaballSystem.MetaballRT[MetaBallsType].Request();
                if (MetaballSystem.MetaballRT[MetaBallsType].IsReady)

                {
                    spriteBatch.Draw(MetaballSystem.MetaballRT[MetaBallsType].GetTarget(), Vector2.One, Color.Transparent);
                }

            }
            else 
            {
                MetaballSystem.DrawingRT[MetaBallsType].Request();
                if (MetaballSystem.DrawingRT[MetaBallsType].IsReady)
                    spriteBatch.Draw(MetaballSystem.DrawingRT[MetaBallsType].GetTarget(), Vector2.One, Color.Transparent);
            }

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

            spriteBatch.End();

            device.SetRenderTarget(null);
            _wasPrepared = true;
            device.SetRenderTargets(oldTargets);
        }

        private void OnResChanged(Vector2 vector)
        {
            Reset();
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

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);


            ITD.ITDMetaBallsShaders["Metaballs"].SetProperties(MetaballSystem.Sets.MainColor[MetaBallsType], MetaballSystem.Sets.OutlineColor[MetaBallsType], MetaballSystem.Sets.OutlineThickness[MetaBallsType], [MetaballSystem.Sets.Image0[MetaBallsType], MetaballSystem.Sets.Image1[MetaBallsType], MetaballSystem.Sets.Image2[MetaBallsType]], MetaballSystem.RealMetaballPositions[MetaBallsType], MetaballSystem.Metaballs.Where(b => b.Type == MetaBallsType).Count(), new Vector2(512), MetaballSystem.RealMetaballSizes[MetaBallsType]);
            ITD.ITDMetaBallsShaders["Metaballs"].apply();

            MetaballSystem.DrawingRT[MetaBallsType].Request();
            if (MetaballSystem.DrawingRT[MetaBallsType].IsReady)
                spriteBatch.Draw(MetaballSystem.DrawingRT[MetaBallsType].GetTarget(), Vector2.One, Color.Transparent);
            
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

            spriteBatch.End();

            device.SetRenderTarget(null);
            _wasPrepared = true;
            device.SetRenderTargets(oldTargets);
        }

        private void OnResChanged(Vector2 vector)
        {
            Reset();
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
        Vector2 _pixelization;
        float[] _sizes = new float[50];
        public MetaballShaderData(Asset<Effect> effect)
        {

            this._effect = effect;

        }
        /// <summary>
        /// dont use it sucks
        /// </summary>
        /// <param name="color"></param>
        /// <param name="outlineColor"></param>
        /// <param name="outlineThicknessSlashRadius"></param>
        /// <param name="Textures"></param>
        /// <param name="positions"></param>
        /// <param name="amount"></param>
        /// <param name="pixelization"> only works for with Real Metaballs </param>
        /// <param name="shaderData"></param>
        public void SetProperties(Color color,Color outlineColor , float outlineThicknessSlashRadius, Asset<Texture2D>[] Textures, Vector2[] positions = default, int amount = default, Vector2 pixelization = default, float[] sizes = default, Vector4 shaderData = default)
        {
            this._color = color;
            this._outlineColor = outlineColor;
            this._outlineThickness = outlineThicknessSlashRadius;
            this._shaderData = shaderData;

            _texutre1 = Textures[0];
            _texutre2 = Textures[1];
            _texutre3 = Textures[2];

            _positions = positions;
            _amount = amount;
            if(pixelization != default)
                _pixelization = pixelization;
            else
                _pixelization = Vector2.One;

            _sizes = sizes;
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
            effect.Parameters["screenResolution"].SetValue(new Vector2(Main.screenWidth,Main.screenHeight));
            effect.Parameters["outlineColor"].SetValue(_outlineColor.ToVector3());
            effect.Parameters["outlineThickness"].SetValue(_outlineThickness);
            effect.Parameters["positions"].SetValue(_positions);
            effect.Parameters["amount"].SetValue(_amount);
            effect.Parameters["pixelization"].SetValue(_pixelization);
            effect.Parameters["sizes"].SetValue(_sizes);

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
