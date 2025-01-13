using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ITD.Common.Prefixes
{
    public abstract class ComplexPrefix : ModPrefix
    {
        public virtual void UpdateHeldPrefix(Item item, Player player)
        {

        }

        public virtual void UpdateEquippedPrefix(Item item, Player player)
        {

        }

        public override bool CanRoll(Item item)
        {
            return true;
        }

        public override float RollChance(Item item)
        {
            return 5f;
        }
    }
}

        
