﻿using ITD.Content.NPCs;
using ITD.Players;

namespace ITD.Content.Buffs.Debuffs
{
    public class SoulRotBuff : ModBuff
    {		
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = true;
        }
		
		public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<ITDPlayer>().soulRot = true;
        }
		
        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<ITDGlobalNPC>().soulRot = true;
        }
    }
}
