using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace ITD.Utilities
{
    public static class CommonPredicates
    {
        public static Func<Entity, bool> EntityRange(Entity sourceEntity, float range)
        {
            return targetEntity => sourceEntity.DistanceSQ(targetEntity.Center) <= range * range;
        }
    }
}
