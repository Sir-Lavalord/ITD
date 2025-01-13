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
    public class Wretched : SnaptrapPrefix
    {
        public Wretched() : base(shootSpeedBonus: -0.04f, lengthBonus: -0.08f, damageBonus: -8)
        {

        }

        public override void ModifyValue(ref float valueMult)
        {
            valueMult *= 0.2f;
        }

        public override float RollChance(Item item)
        {
            return 3f;
        }
    }
}

