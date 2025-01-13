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
    public class Sleek : SnaptrapPrefix
    {
        public Sleek() : base(critBonus: -1, shootSpeedBonus: 0.16f, retractRateBonus: 0.16f)
        {

        }

        public override void ModifyValue(ref float valueMult)
        {
            valueMult *= 1.75f;
        }

        public override float RollChance(Item item)
        {
            return 1.47f;
        }
    }
}

