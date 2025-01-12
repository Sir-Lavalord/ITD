using ITD.Content.Projectiles;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace ITD.Systems
{
    public class SnaptrapPlayer : ModPlayer
    {
        public bool ChainWeightEquipped;
        public float LengthIncrease;
        public float RetractMultiplier;
        public int LatchTimeModifer;
        public int WarningModifer;

        public bool CanUseSnaptrap
        {
            get
            {
                if (Player.altFunctionUse == 2 && GetActiveSnaptrap() != null)
                    return true;
                return GetActiveSnaptrap() is null;
            }
        }
        public override void ResetEffects()
        {
            ChainWeightEquipped = false;
            LengthIncrease = 0f;
            RetractMultiplier = 0f;
            LatchTimeModifer = 0;
            WarningModifer = 0;
        }
        public bool ShootSnaptrap()
        {
            ITDSnaptrap snaptrap = GetActiveSnaptrap();
            bool rightClick = Player.altFunctionUse == 2;
            if (snaptrap is null && !rightClick)
                return true;
            else if (snaptrap is not null)
                snaptrap.retracting = true;
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
