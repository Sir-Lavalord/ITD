using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace ITD.Content.Projectiles.Friendly.Melee.Snaptraps
{
    public class VocalZeroProjectile : ITDSnaptrap
    {
        public static LocalizedText OneTimeLatchMessage { get; private set; }
        int constantEffectFrames = 60;
        int constantEffectTimer = 0;
        int effectCount = 0;
        public override void SetSnaptrapDefaults()
        {
            OneTimeLatchMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"Projectiles.{nameof(VocalZeroProjectile)}.OneTimeLatchMessage"));
            ShootRange = 16f * 16f;
            RetractAccel = 1.8f;
            ExtraFlexibility = 16f * 4f;
            MinDamage = 1280;
            FullPowerHitsAmount = 10;
            WarningFrames = 80;
            ChompDust = DustID.Blood;
            ToChainTexture = "ITD/Content/Projectiles/Friendly/Melee/Snaptraps/VocalZeroChain";
            DrawOffsetX = -22;
            DrawOriginOffsetY = -22;
        }
        public override void ConstantLatchEffect()
        {
            constantEffectTimer += 1;
            if (constantEffectTimer >= constantEffectFrames)
            {
                constantEffectTimer = 0;
                if (effectCount < 10)
                {
                    effectCount += 1;
                    AdvancedPopupRequest popupSettings = new()
                    {
                        Text = OneTimeLatchMessage.WithFormatArgs(10).Value,
                        Color = Color.Red,
                        DurationInFrames = 120,
                        Velocity = Projectile.velocity,
                    };
                    PopupText.NewText(popupSettings, Projectile.Center + new Vector2(0f, -50f));
                }
            }
        }
        public override void ModifyMaxDamage(ref int maxDamage)
        {
            maxDamage = (int)(maxDamage * (1f + (effectCount / 10f)));
        }
        public override void PostAI()
        {
            Dust.NewDust(Projectile.Center, 6, 6, ChompDust, 0f, 0f, 0, default(Color), 1);
        }
    }
}