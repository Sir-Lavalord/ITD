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

    public class MetaballRenderTarget : ARenderTargetContentByRequest
    {

        public MetaballRenderTarget() 
        {
            Main.OnResolutionChanged += OnResChanged;
        }

        protected override void HandleUseReqest(GraphicsDevice device, SpriteBatch spriteBatch)
        {

            PrepareARenderTarget_AndListenToEvents(ref _target, device, Main.screenWidth, Main.screenHeight, RenderTargetUsage.PreserveContents);
            var oldTargets = device.GetRenderTargets();
            device.SetRenderTarget(_target);
            device.Clear(Color.Cyan);
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

        MetaballRenderTarget MetaballRT;

        public override void Load()
        {
            Main.ContentThatNeedsRenderTargets.Add(MetaballRT = new MetaballRenderTarget());
        }

        public override void Unload()
        {
            Main.ContentThatNeedsRenderTargets.Remove(MetaballRT);
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
        public bool enabled = false;
        public MetaballsShaderData(Asset<Effect> effect)
        {

            this._effect = effect;

        }
        public void setProperties(Color color, Asset<Texture2D> texutre1 = null, Asset<Texture2D> texutre2 = null, Asset<Texture2D> texutre3 = null, Vector4 shaderData = default)
        {
            this._color = color;
            this._texutre1 = texutre1;
            this._texutre2 = texutre2;
            this._texutre3 = texutre3;
            this._shaderData = shaderData;
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
