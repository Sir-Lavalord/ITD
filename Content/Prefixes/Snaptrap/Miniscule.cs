using ITD.Common.Prefixes;
using ITD.Content.Items.Weapons.Melee.Snaptraps;
using ITD.Content.Prefixes.Snaptrap;
using ITD.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ITD.Content.Prefixes
{
    public class Miniscule : SnaptrapPrefix
    {
        public Miniscule() : base(shootSpeedBonus: 0.16f, retractRateBonus: 0.16f, damageBonus: -20)
        {

        }

        public override void ModifyValue(ref float valueMult)
        {
            valueMult *= 1.6f;
        }

        public override float RollChance(Item item)
        {
            return 1.62f;
        }
    }
}

