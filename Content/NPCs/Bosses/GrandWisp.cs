using ITD.Content.Buffs.Debuffs;
using ITD.Particles;
using ITD.Particles.Misc;
using System;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
namespace ITD.Content.NPCs.Bosses;

public class GrandWisp : ModNPC
{
    public override void SetDefaults()
    {
        NPC.width = 30;
        NPC.height = 30;
        NPC.lifeMax = 500;
        NPC.damage = 30;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.aiStyle = NPCAIStyleID.FlyingFish; 
    }

    public override void OnKill()
    {
        int parentID = (int)NPC.ai[0];
        if (Main.npc[parentID].active && Main.npc[parentID].type == ModContent.NPCType<MotherWisp>())
        {
            Main.npc[parentID].ai[2] += 1;

            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, parentID);
            }
        }
    }
}