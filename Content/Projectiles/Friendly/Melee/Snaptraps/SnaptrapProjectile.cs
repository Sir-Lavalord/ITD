using Terraria.Localization;

namespace ITD.Content.Projectiles.Friendly.Melee.Snaptraps
{
    public class SnaptrapProjectile : ITDSnaptrap
    {
        public static LocalizedText OneTimeLatchMessage { get; private set; }
        private readonly int addCritChance = 4;
        public override void SetSnaptrapDefaults()
        {
            OneTimeLatchMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"Projectiles.{nameof(SnaptrapProjectile)}.OneTimeLatchMessage"));
            ShootRange = 16f * 12f;
            RetractAccel = 1.5f;
            ExtraFlexibility = 16f * 2f;
            MinDamage = 20;
            FullPowerHitsAmount = 10;
            WarningFrames = 60;
            ChompDust = DustID.Titanium;
        }
        public override bool OneTimeLatchEffect()
        {
            Projectile.CritChance += addCritChance;
            AdvancedPopupRequest popupSettings = new()
            {
                Text = OneTimeLatchMessage.WithFormatArgs(addCritChance).Value,
                Color = Color.White,
                DurationInFrames = 60 * 2,
                Velocity = Projectile.velocity,
            };
            PopupText.NewText(popupSettings, Projectile.Center + new Vector2(0f, -50f));
            return true;
        }
    }
}