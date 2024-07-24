using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ITD.Content.Dusts;
using Terraria.Localization;
using ITD.Content.Projectiles.Friendly.Snaptraps.Extra;

namespace ITD.Content.Projectiles.Friendly.Snaptraps
{
    public class DespoticSnaptrapProjectile : ITDSnaptrap
    {
        public static LocalizedText OneTimeLatchMessage { get; private set; }
        private const string ChainTextureExtraPath = "ITD/Content/Projectiles/Friendly/Snaptraps/DespoticSnaptrapChain1";
        private const string ChainTextureExtra2Path = "ITD/Content/Projectiles/Friendly/Snaptraps/DespoticSnaptrapChain2";
        private readonly int constantEffectFrames = 200;
        int constantEffectTimer = 0;
        public override void SetSnaptrapProperties()
        {
            OneTimeLatchMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"Projectiles.{nameof(DespoticSnaptrapProjectile)}.OneTimeLatchMessage"));
            ShootRange = 16f * 20f;
            RetractAccel = 1.5f;
            FramesUntilRetractable = 10;
            ExtraFlexibility = 16f * 6f;
            FramesBetweenHits = 16;
            MinDamage = 3560;
            MaxDamage = 8900;
            FullPowerHitsAmount = 10;
            WarningFrames = 200;
            ChompDust = DustID.IceTorch;
            ToChainTexture = "ITD/Content/Projectiles/Friendly/Snaptraps/DespoticSnaptrapChain";
            DrawOffsetX = -20;
            DrawOriginOffsetY = -24;
            Projectile.extraUpdates = 1;
        }
        private void SummonJaw()
        {
            if (Main.myPlayer == myPlayer.whoAmI)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<DespoticJawProjectile>(), 0, 0.1f);
            }
        }
        public override void PostAI()
        {
            Dust.NewDust(Projectile.Center, 6, 6, ChompDust, 0f, 0f, 0, default, 1f);
            Dust.NewDust(Projectile.Center, 6, 6, DustID.MushroomTorch, 0f, 0f, 0, Color.Blue, 2f);
            if (!retracting)
            {
                if (Main.rand.NextBool() == true)
                {
                    Dust.NewDust(Projectile.Center, 4, 4, ModContent.DustType<DespoticDust>(), 0, 0, 0, default, 1f);
                }
            }
        }
        public override void OneTimeLatchEffect()
        {
            SoundEngine.PlaySound(snaptrapMetal, Projectile.Center);
            AdvancedPopupRequest popupSettings = new()
            {
                Text = OneTimeLatchMessage.Value,
                //Text = "Munchity munch...",
                Color = Color.DodgerBlue,
                DurationInFrames = 60 * 2,
                Velocity = Projectile.velocity,
            };
            PopupText.NewText(popupSettings, Projectile.Center + new Vector2(0f, -50f));
            SummonJaw();
        }

        public override void ConstantLatchEffect()
        {
            constantEffectTimer++;
            if (constantEffectTimer >= constantEffectFrames)
            {
                constantEffectTimer = 0;
                SummonJaw();
            }
        }
        public override Asset<Texture2D> GetChainTexture(Asset<Texture2D> defaultTexture, Vector2 chainDrawPosition, int chainCount)
        {
            if (chainCount >= 12)
            {
                
            }
            else if (chainCount >= 8)
            {
                return ModContent.Request<Texture2D>(ChainTextureExtraPath);
            }
            else
            {
                return ModContent.Request<Texture2D>(ChainTextureExtra2Path);
            }
            return defaultTexture;
        }
        public override Color GetChainColor(Vector2 chainDrawPosition, int chainCount)
        {
            var chainDrawColor = base.GetChainColor(chainDrawPosition, chainCount);
            if (chainCount >= 12)
            {
                // Use normal chainTexture and lighting, no changes
            }
            else if (chainCount >= 8)
            {
                // Near to the ball, we draw a custom chain texture and slightly make it glow if unlit.
                byte minValue = 140;
                if (chainDrawColor.R < minValue)
                    chainDrawColor.R = minValue;

                if (chainDrawColor.G < minValue)
                    chainDrawColor.G = minValue;

                if (chainDrawColor.B < minValue)
                    chainDrawColor.B = minValue;
            }
            else
            {
                // Close to the ball, we draw a custom chain texture and draw it at full brightness glow.
                return Color.White;
            }
            return chainDrawColor;
        }
    }
}