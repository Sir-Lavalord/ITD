using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ITD.Content.Projectiles.Friendly.Snaptraps.Extra;

namespace ITD.Content.Projectiles.Friendly.Snaptraps
{
    public class CrimeratrapProjectile : ITDSnaptrap
    {
        public static LocalizedText OneTimeLatchMessage { get; private set; }
        int constantEffectFrames = 55;
        int constantEffectTimer = 0;
        public override void SetSnaptrapProperties()
        {
            OneTimeLatchMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"Projectiles.{nameof(CrimeratrapProjectile)}.OneTimeLatchMessage"));
            shootRange = 16f * 16f;
            retractAccel = 1.5f;
            extraFlexibility = 16f * 2f;
            framesBetweenHits = 24;
            minDamage = 14;
            maxDamage = 29;
            fullPowerHitsAmount = 10;
            warningFrames = 60;
            chompDust = DustID.CrimsonPlants;
            toChainTexture = "ITD/Content/Projectiles/Friendly/Snaptraps/CrimeratrapChain";
            DrawOffsetX = -13;
            DrawOriginOffsetY = -19;
        }
        private void Spit()
        {
            if (Main.myPlayer == myPlayer.whoAmI)
            {
                for (int i = 0; i < 8; i++)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2((float)Math.Cos(MathHelper.PiOver4 * i) * 3f, (float)Math.Sin(MathHelper.PiOver4 * i) * 3f), ModContent.ProjectileType<EvilSpitProjectile>(), 2, 0.1f, ai0: 1f);
                }
            }
        }
        public override void OneTimeLatchEffect()
        {
            SoundEngine.PlaySound(snaptrapMetal, Projectile.Center);
            AdvancedPopupRequest popupSettings = new AdvancedPopupRequest
            {
                Text = OneTimeLatchMessage.Value,
                //Text = "+4% crit chance!",
                Color = Color.IndianRed,
                DurationInFrames = 60 * 2,
                Velocity = Projectile.velocity,
            };
            PopupText.NewText(popupSettings, Projectile.Center + new Vector2(0f, -50f));
            Spit();
        }
        public override void ConstantLatchEffect()
        {
            constantEffectTimer++;
            if (constantEffectTimer >= constantEffectFrames)
            {
                constantEffectTimer = 0;
                Spit();
            }
        }
    }
}