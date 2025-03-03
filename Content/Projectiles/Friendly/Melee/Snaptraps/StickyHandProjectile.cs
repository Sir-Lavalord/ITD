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
            Main.projFrames[Projectile.type] = 1;
            OneTimeLatchMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"Projectiles.{nameof(StickyHandProjectile)}.OneTimeLatchMessage"));
            ShootRange = 12f * 12f;
            RetractAccel = 1.5f;
            ExtraFlexibility = 28f * 2f;
            MinDamage = 9;
            FullPowerHitsAmount = 1;
            WarningFrames = 0;//Looks ugly so no
            ChompDust = DustID.t_Slime;
            DrawOffsetX = -16;

            ToChainTexture = "ITD/Content/Projectiles/Friendly/Melee/Snaptraps/StickyHandChain";
            toSnaptrapChomp = "ITD/Content/Sounds/StickyHandSplat";
            toSnaptrapForcedRetract = "ITD/Content/Sounds/StickyHandUnlatch";
            toSnaptrapChain = "ITD/Content/Sounds/StickyHandUnlatch";
        }
    }
}