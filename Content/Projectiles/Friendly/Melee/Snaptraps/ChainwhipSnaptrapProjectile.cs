using Terraria.Localization;

namespace ITD.Content.Projectiles.Friendly.Melee.Snaptraps;

public class ChainwhipSnaptrapProjectile : ITDSnaptrap
{
    public static LocalizedText OneTimeLatchMessage { get; private set; }
    readonly int constantEffectFrames = 20;
    int constantEffectTimer = 0;
    float constantEffect = 0f;
    public override void SetSnaptrapDefaults()
    {
        OneTimeLatchMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"Projectiles.{nameof(ChainwhipSnaptrapProjectile)}.OneTimeLatchMessage"));
        ShootRange = 16f * 18f;
        RetractAccel = 1.5f;
        ExtraFlexibility = 16f * 2f;
        MinDamage = 22;
        FullPowerHitsAmount = 10;
        WarningFrames = 60;
        ToChainTexture = "ITD/Content/Projectiles/Friendly/Melee/Snaptraps/ChainwhipChain";
        ChompDust = DustID.Bone;
        DrawOffsetX = -16;
    }
    public override bool OneTimeLatchEffect()
    {
        AdvancedPopupRequest popupSettings = new()
        {
            Text = OneTimeLatchMessage.Value,
            Color = Color.Silver,
            DurationInFrames = 60 * 2,
            Velocity = Projectile.velocity,
        };
        PopupText.NewText(popupSettings, Projectile.Center + new Vector2(0f, -50f));
        return true;
    }

    public override void ConstantLatchEffect()
    {
        Player player = Main.player[Projectile.owner];
        if (constantEffect < 0.6f)
        {
            constantEffectTimer++;
            if (constantEffectTimer >= constantEffectFrames)
            {
                constantEffectTimer = 0;
                constantEffect += 0.01f;
            }
        }
        player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += constantEffect;
        player.GetDamage(DamageClass.Summon) += constantEffect;
        player.moveSpeed += constantEffect;
    }

    public override bool PreAI()
    {
        Player player = Main.player[Projectile.owner];
        ITDSnaptrap snaptrap = player.Snaptrap().ActiveSnaptrap;
        if (snaptrap.retracting)
            constantEffect = 0f;

        return true;
    }
}