using System.Collections.Generic;

namespace ITD.Content.Events
{
    public class TestChaoticEvent : ITDEvent
    {
        public override IEnumerable<(int, float)> GetPool(NPCSpawnInfo spawnInfo)
        {
            yield return (NPCID.MoonLordCore, 1f);
            yield return (NPCID.HallowBoss, 1f);
        }
    }
}
