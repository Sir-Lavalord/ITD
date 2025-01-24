using ITD.Content.NPCs.DeepDesert;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

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
