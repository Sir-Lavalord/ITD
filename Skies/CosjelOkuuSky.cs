using ITD.Content.NPCs.Bosses;
using Terraria.Graphics.Effects;

namespace ITD.Skies
{
    public class CosjelOkuuSky : CustomSky
    {
        private bool isActive = false;
        private float intensity = 0f;

        public override void Update(GameTime gameTime)
        {
            const float increment = 0.01f;
            if (NPC.AnyNPCs(ModContent.NPCType<CosmicJellyfish>()))
            {
                intensity += increment;
                if (intensity > 1f)
                {
                    intensity = 1f;
                }
            }
            else
            {
                intensity -= increment;
                if (intensity < 0f)
                {
                    intensity = 0f;
                    Deactivate();
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 0 && minDepth < 0)
            {
                spriteBatch.Draw(ITD.Instance.Assets.Request<Texture2D>("Skies/CosjelOkuuSky", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value,
                    new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black * intensity * 1f);
            }
        }

        public override float GetCloudAlpha()
        {
            return 2f - intensity;
        }

        public override void Activate(Vector2 position, params object[] args)
        {
            isActive = true;
        }

        public override void Deactivate(params object[] args)
        {
            isActive = false;
        }
        public override void Reset()
        {
            isActive = false;
        }

        public override bool IsActive()
        {
            return isActive;
        }

        public override Color OnTileColor(Color inColor)
        {
            return new Color(Vector4.Lerp(new Vector4(0f, 0f, 0f, 1f), inColor.ToVector4(), 1f - intensity));
        }
    }
}