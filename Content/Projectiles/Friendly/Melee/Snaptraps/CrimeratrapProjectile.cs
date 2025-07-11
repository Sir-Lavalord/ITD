using System;
using Terraria.Localization;
using ITD.Content.Projectiles.Friendly.Melee.Snaptraps.Extra;

namespace ITD.Content.Projectiles.Friendly.Melee.Snaptraps
{
    public class CrimeratrapProjectile : ITDSnaptrap
    {
        public static LocalizedText OneTimeLatchMessage { get; private set; }
        int constantEffectFrames = 55;
        int constantEffectTimer = 0;
        public override void SetSnaptrapDefaults()
        {
            OneTimeLatchMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"Projectiles.{nameof(CrimeratrapProjectile)}.OneTimeLatchMessage"));
            ShootRange = 16f * 10f;
            RetractAccel = 1.5f;
            ExtraFlexibility = 16f * 2f;
            MinDamage = 14;
            FullPowerHitsAmount = 10;
            WarningFrames = 60;
            ChompDust = DustID.CrimsonPlants;
            ToChainTexture = "ITD/Content/Projectiles/Friendly/Melee/Snaptraps/CrimeratrapChain";
            DrawOffsetX = -13;
            DrawOriginOffsetY = -19;
        }
        private void Spit()
        {
            if (Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < 8; i++)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2((float)Math.Cos(MathHelper.PiOver4 * i) * 3f, (float)Math.Sin(MathHelper.PiOver4 * i) * 3f), ModContent.ProjectileType<EvilSpitProjectile>(), 2, 0.1f, ai0: 1f);
                }
            }
        }
        public override bool OneTimeLatchEffect()
        {
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