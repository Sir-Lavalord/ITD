using ITD.Content.Items.Accessories.Combat.Melee.Snaptraps;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace ITD.Particles
{
    public class Metaballs
    {
    }

    public class MetaballsTestRenderTarget : ARenderTargetContentByRequest
    {

        public MetaballsTestRenderTarget() 
        {
            Main.OnResolutionChanged += OnResChanged;
        }

        protected override void HandleUseReqest(GraphicsDevice device, SpriteBatch spriteBatch)
        {

            PrepareARenderTarget_AndListenToEvents(ref _target, device, Main.screenWidth, Main.screenHeight, RenderTargetUsage.PreserveContents);
            var oldTargets = device.GetRenderTargets();
            device.SetRenderTarget(_target);
            device.Clear(Color.Transparent);

            device.SetRenderTarget(null);
            _wasPrepared = true;
            device.SetRenderTargets(oldTargets);

        }

        private void OnResChanged(Vector2 vector)
        {
            Reset();
        }
    }

    public class MetaballsSystem : ModSystem
    {

        MetaballsTestRenderTarget MetaballsTestRT;

        public override void Load()
        {
            Main.ContentThatNeedsRenderTargets.Add(MetaballsTestRT = new MetaballsTestRenderTarget());
        }

        public override void Unload()
        {
            Main.ContentThatNeedsRenderTargets.Remove(MetaballsTestRT);
        }

        public override void PostDrawTiles()
        {
            Main.spriteBatch.Begin();

            ITD.ITDMetaBallsShaders["MetaballsTest"].setProperties(Main.LocalPlayer.Center - Main.screenPosition, 256, Color.Cyan);
            ITD.ITDMetaBallsShaders["MetaballsTest"].apply();

            if (MetaballsTestRT.IsReady)
                Main.spriteBatch.Draw(MetaballsTestRT.GetTarget(), Main.screenPosition, Color.White);

            Main.spriteBatch.End();
        }
    }

    public class MetaballsShaderData : ILoadable
    {
        static GraphicsDevice GraphicsDevice => Main.instance.GraphicsDevice;
        Asset<Effect> _effect;
        Color _color = new Color(0, 0, 0);
        Asset<Texture2D> _texutre1 = null;
        Asset<Texture2D> _texutre2 = null;
        Asset<Texture2D> _texutre3 = null;
        Vector4 _shaderData = new Vector4(0, 0, 0, 0);
        float _radius = 10;
        Vector2 _position = new Vector2(0, 0);
        public bool enabled = false;
        public MetaballsShaderData(Asset<Effect> effect)
        {

            this._effect = effect;

        }
        public void setProperties(Vector2 position, float radius ,Color color, Asset<Texture2D> texutre1 = null, Asset<Texture2D> texutre2 = null, Asset<Texture2D> texutre3 = null, Vector4 shaderData = default)
        {
            this._color = color;
            this._texutre1 = texutre1;
            this._texutre2 = texutre2;
            this._texutre3 = texutre3;
            this._shaderData = shaderData;
            this._position = position;
            this._radius = radius;
        }

        public void setupTextures()
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
            setupTextures();
            effect.Parameters["viewWorldProjection"].SetValue(Matrix.CreateTranslation(new Vector3(-Main.screenPosition, 0)) * Main.GameViewMatrix.TransformationMatrix * Matrix.CreateOrthographicOffCenter(left: 0, right: viewport.Width, bottom: viewport.Height, top: 0, zNearPlane: -1, zFarPlane: 10));
            effect.Parameters["time"].SetValue(Main.GlobalTimeWrappedHourly);
            effect.Parameters["color"].SetValue(_color.ToVector3());
            effect.Parameters["shaderData"].SetValue(_shaderData);
            effect.Parameters["position"].SetValue(_position);
            effect.Parameters["radius"].SetValue(_radius);
            effect.CurrentTechnique.Passes[0].Apply();

        }

        public void Load(Mod mod)
        {

        }

        public void Unload()
        {
            Main.RunOnMainThread(() => {

                _effect.Dispose();

            }).Wait();
        }
    }
}
