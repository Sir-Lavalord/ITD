using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles
{
    public class SoulsnapperProjectile : ITDSnaptrap
    {
        public static LocalizedText OneTimeLatchMessage { get; private set; }
        public override void SetSnaptrapProperties()
        {
            OneTimeLatchMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"Projectiles.{nameof(SoulsnapperProjectile)}.OneTimeLatchMessage"));
            shootRange = 16f * 16f;
            retractAccel = 1.5f;
            extraFlexibility = 16f * 2f;
            framesBetweenHits = 22;
            minDamage = 12;
            maxDamage = 34;
            fullPowerHitsAmount = 10;
            warningFrames = 60;
            chompDust = DustID.CorruptionThorns;
            toChainTexture = "ITD/Content/Projectiles/Friendly/SoulsnapperChain";
            DrawOffsetX = -9;
            DrawOriginOffsetY = -16;
        }
        public override void OneTimeLatchEffect()
        {
            SoundEngine.PlaySound(snaptrapMetal, Projectile.Center);
            AdvancedPopupRequest popupSettings = new AdvancedPopupRequest
            {
                Text = OneTimeLatchMessage.Value,
                //Text = "+4% crit chance!",
                Color = Color.YellowGreen,
                DurationInFrames = 60 * 2,
                Velocity = Projectile.velocity,
            };
            PopupText.NewText(popupSettings, Projectile.Center + new Vector2(0f, -50f));
        }
    }
}