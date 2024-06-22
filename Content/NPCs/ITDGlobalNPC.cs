using ITD.Content.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
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
        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.GetModPlayer<ITD.ITDPlayer>().ZoneDeepDesert)
            {
                pool[NPCID.CaveBat] = 0f;
                pool[NPCID.BlueSlime] = 0f;
                pool[NPCID.GiantWalkingAntlion] = 0f;
                pool[NPCID.GiantFlyingAntlion] = 0f;
                pool[NPCID.FlyingAntlion] = 0f;
                pool[NPCID.WalkingAntlion] = 0f;
                pool[NPCID.Antlion] = 0f;
            }
        }
    }
}
