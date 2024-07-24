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
    public class SoulsnapperProjectile : ITDSnaptrap
    {
        public static LocalizedText OneTimeLatchMessage { get; private set; }
        int constantEffectFrames = 60;
        int constantEffectTimer = 0;
        public override void SetSnaptrapProperties()
        {
            OneTimeLatchMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"Projectiles.{nameof(SoulsnapperProjectile)}.OneTimeLatchMessage"));
            ShootRange = 16f * 16f;
            RetractAccel = 1.5f;
            ExtraFlexibility = 16f * 2f;
            FramesBetweenHits = 24;
            MinDamage = 12;
            MaxDamage = 28;
            FullPowerHitsAmount = 10;
            WarningFrames = 60;
            ChompDust = DustID.CorruptionThorns;
            ToChainTexture = "ITD/Content/Projectiles/Friendly/Snaptraps/SoulsnapperChain";
            DrawOffsetX = -9;
            DrawOriginOffsetY = -16;
        }
        private void Spit()
        {
            if (Main.myPlayer == myPlayer.whoAmI)
            {
                for (int i = 0; i < 8; i++)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2((float)Math.Cos(MathHelper.PiOver4 * i) * 2.75f, (float)Math.Sin(MathHelper.PiOver4 * i) * 2.75f), ModContent.ProjectileType<EvilSpitProjectile>(), 1, 0.1f, ai0: 0f);
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
                Color = Color.YellowGreen,
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