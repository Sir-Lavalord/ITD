using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Friendly.Melee.Snaptraps
{
    public class StickyHandProjectile : ITDSnaptrap
    {
        public static LocalizedText OneTimeLatchMessage { get; private set; }
        public override void SetSnaptrapDefaults()
        {
            OneTimeLatchMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"Projectiles.{nameof(StickyHandProjectile)}.OneTimeLatchMessage"));
            ShootRange = 12f * 12f;
            RetractAccel = 1.5f;
            ExtraFlexibility = 24f * 2f;
            MinDamage = 9;
            FullPowerHitsAmount = 10;
            WarningFrames = 120;
            ChompDust = DustID.SlimeBunny;
            DrawOffsetX = -16;

            ToChainTexture = "ITD/Content/Projectiles/Friendly/Melee/Snaptraps/StickyHandChain";
            toSnaptrapChomp = "ITD/Content/Sounds/StickHandSplat";
            toSnaptrapForcedRetract = "ITD/Content/StickyHandUnlatch";
            toSnaptrapChain = "ITD/Content/StickyHandUnlatch";
        }
    }
}