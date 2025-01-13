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
    public class Dwarf : SnaptrapPrefix
    {
        public Dwarf() : base(shootSpeedBonus: 0.08f, retractRateBonus: 0.08f, damageBonus: -12)
        {

        }

        public override void ModifyValue(ref float valueMult)
        {
            valueMult *= 0.8f;
        }

        public override float RollChance(Item item)
        {
            return 2.42f;
        }
    }
}

