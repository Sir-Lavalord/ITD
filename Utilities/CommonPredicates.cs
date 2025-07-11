using System;

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
