﻿using ITD.Content.Projectiles.Friendly.Melee.Snaptraps;

namespace ITD.Systems
{
    public class SnaptrapPlayer : ModPlayer
    {
        public bool ChainWeightEquipped;
        public StatModifier LengthModifier;
        public StatModifier RetractVelocityModifier;
        public StatModifier FullPowerHitsModifier;
        public StatModifier WarningModifier;
        public bool CanUseSnaptrap
        {
            get
            {
                ITDSnaptrap snaptrap = GetActiveSnaptrap();
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
            ITDSnaptrap snaptrap = GetActiveSnaptrap();
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
        public ITDSnaptrap GetActiveSnaptrap()
        {
            foreach (var proj in Main.ActiveProjectiles)
            {
                if (proj.ModProjectile is ITDSnaptrap snaptrap && proj.owner == Player.whoAmI)
                {
                    return snaptrap;
                }
            }
            return null;
        }
    }
}
