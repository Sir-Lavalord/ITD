using System;
using Terraria.Localization;
using ITD.Content.Projectiles.Friendly.Melee.Snaptraps.Extra;

namespace ITD.Content.Projectiles.Friendly.Melee.Snaptraps
{
    public class SoulsnapperProjectile : ITDSnaptrap
    {
        public static LocalizedText OneTimeLatchMessage { get; private set; }
        int constantEffectFrames = 60;
        int constantEffectTimer = 0;
        public override void SetSnaptrapDefaults()
        {
            OneTimeLatchMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"Projectiles.{nameof(SoulsnapperProjectile)}.OneTimeLatchMessage"));
            ShootRange = 16f * 10f;
            RetractAccel = 1.5f;
            ExtraFlexibility = 16f * 2f;
            MinDamage = 12;
            FullPowerHitsAmount = 10;
            WarningFrames = 60;
            ChompDust = DustID.CorruptionThorns;
            ToChainTexture = "ITD/Content/Projectiles/Friendly/Melee/Snaptraps/SoulsnapperChain";
            DrawOffsetX = -9;
            DrawOriginOffsetY = -16;
        }
        private void Spit()
        {
            if (Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < 8; i++)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2((float)Math.Cos(MathHelper.PiOver4 * i) * 2.75f, (float)Math.Sin(MathHelper.PiOver4 * i) * 2.75f), ModContent.ProjectileType<EvilSpitProjectile>(), 1, 0.1f, ai0: 0f);
                }
            }
        }
        public override bool OneTimeLatchEffect()
        {
            AdvancedPopupRequest popupSettings = new()
            {
                Text = OneTimeLatchMessage.Value,
                //Text = "+4% crit chance!",
                Color = Color.YellowGreen,
                DurationInFrames = 60 * 2,
                Velocity = Projectile.velocity,
            };
            PopupText.NewText(popupSettings, Projectile.Center + new Vector2(0f, -50f));
            Spit();
            return true;
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