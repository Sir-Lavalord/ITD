using Terraria.Localization;

namespace ITD.Content.Projectiles.Friendly.Melee.Snaptraps;

public class StickyHandProjectile : ITDSnaptrap
{
    public static LocalizedText OneTimeLatchMessage { get; private set; }
    public override void SetSnaptrapDefaults()
    {
        Main.projFrames[Projectile.type] = 1;
        OneTimeLatchMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"Projectiles.{nameof(StickyHandProjectile)}.OneTimeLatchMessage"));
        ShootRange = 12f * 12f;
        RetractAccel = 3.5f;
        ExtraFlexibility = 30f * 2f;
        MinDamage = 9;
        FullPowerHitsAmount = 1;
        WarningFrames = 200;
        ChompDust = DustID.t_Slime;
        DrawOffsetX = -16;

        ToChainTexture = "ITD/Content/Projectiles/Friendly/Melee/Snaptraps/StickyHandChain";
        toSnaptrapChomp = "ITD/Content/Sounds/StickyHandSplat";
        toSnaptrapForcedRetract = "ITD/Content/Sounds/StickyHandUnlatch";
        toSnaptrapChain = "ITD/Content/Sounds/StickyHandUnlatch";
    }
    public override void ExtraChainEffects(ref Vector2 chainDrawPosition, int chaincount)
    {
        float factor = WarningTimer / (float)WarningFrames * 2.25f;
        Projectile.Center += Main.rand.NextVector2Circular(factor, factor);
    }
}