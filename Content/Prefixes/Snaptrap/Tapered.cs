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
    public class Tapered : SnaptrapPrefix
    {
        public Tapered() : base(critBonus: 2, shootSpeedBonus: 0.08f, damageBonus: 15)
        {

        }

        public override void ModifyValue(ref float valueMult)
        {
            valueMult *= 4.25f;
        }

        public override float RollChance(Item item)
        {
            return 1.02f;
        }
    }
}

