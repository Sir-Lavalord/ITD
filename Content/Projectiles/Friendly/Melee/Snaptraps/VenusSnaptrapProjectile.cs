using Terraria.Localization;

namespace ITD.Content.Projectiles.Friendly.Melee.Snaptraps;


public class VenusSnaptrapProjectile : ITDSnaptrap
{
    public static LocalizedText OneTimeLatchMessage { get; private set; }

    public override void SetSnaptrapDefaults()
    {
        OneTimeLatchMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"Projectiles.{nameof(VenusSnaptrapProjectile)}.OneTimeLatchMessage"));
        ShootRange = 16f * 10f;
        RetractAccel = 1.9f;
        ExtraFlexibility = 16f * 2f;
        MinDamage = 1;
        FullPowerHitsAmount = 13;
        WarningFrames = 60;
        ChompDust = DustID.JungleSpore;
        ToChainTexture = "ITD/Content/Projectiles/Friendly/Melee/Snaptraps/VenusSnaptrapChain";
        //maybe there's a way to optimize sound swapping?
        // optimized it
        toSnaptrapChomp = "ITD/Content/Sounds/VenusClose";
        toSnaptrapForcedRetract = "ITD/Content/Sounds/VenusRetract";
        toSnaptrapChain = "ITD/Content/Sounds/VenusChain";
        toSnaptrapWarning = "ITD/Content/Sounds/VenusWarning";
        DrawOffsetX = -8;
        DrawOriginOffsetY = -16;
    }
    public override bool OneTimeLatchEffect()
    {
        Main.npc[TargetWhoAmI].AddBuff(BuffID.Poisoned, 80);
        AdvancedPopupRequest popupSettings = new()
        {
            Text = OneTimeLatchMessage.Value,

            Color = Color.GreenYellow,
            DurationInFrames = 60,
            Velocity = Projectile.velocity,
        };
        PopupText.NewText(popupSettings, Projectile.Center + new Vector2(0f, -50f));
        return true;
    }

}