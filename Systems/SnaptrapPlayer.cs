using ITD.Content.Projectiles.Friendly.Melee.Snaptraps;

namespace ITD.Systems;

public class SnaptrapPlayer : ModPlayer
{
    public bool ChainWeightEquipped;
    public StatModifier LengthModifier;
    public StatModifier RetractVelocityModifier;
    public StatModifier FullPowerHitsModifier;
    public StatModifier WarningModifier;
    private ITDSnaptrap _activeSnaptrap;
    public ITDSnaptrap ActiveSnaptrap
    {
        get => _activeSnaptrap;
        set
        {
            if (value is null || value.Projectile is null || !value.Projectile.active)
                _activeSnaptrap = null;
            else
                _activeSnaptrap = value;
        }
    }
    public bool CanUseSnaptrap
    {
        get
        {
            ITDSnaptrap snaptrap = ActiveSnaptrap;
            return snaptrap is null || snaptrap.IsStickingToTarget;
        }
    }
    public override void ResetEffects()
    {
        ChainWeightEquipped = false;
        LengthModifier = StatModifier.Default;
        RetractVelocityModifier = StatModifier.Default;
        FullPowerHitsModifier = StatModifier.Default;
        WarningModifier = StatModifier.Default;
    }
    public bool ShootSnaptrap()
    {
        ITDSnaptrap snaptrap = ActiveSnaptrap;
        if (snaptrap is null)
            return true;
        else if (snaptrap is not null)
        {
            snaptrap.manualRetract = true;
            snaptrap.retracting = true;
            snaptrap.Projectile.netUpdate = true;
        }
        return false;
    }
}
