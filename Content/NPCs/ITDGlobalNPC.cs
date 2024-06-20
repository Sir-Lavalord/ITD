using ITD.Content.Items;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.NPCs
{
    public class ITDGlobalNPC : GlobalNPC
    {
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            if (npc.type == NPCID.EaterofSouls || npc.type == NPCID.BigEater || npc.type == NPCID.LittleEater)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Soulsnapper>(), 50));
            }
            if (npc.type == NPCID.Crimera || npc.type == NPCID.LittleCrimera || npc.type == NPCID.BigCrimera)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Crimeratrap>(), 50));
            }
        }
    }
}
