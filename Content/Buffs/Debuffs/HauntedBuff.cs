﻿using ITD.Content.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace ITD.Content.Buffs.Debuffs
{
    public class HauntedBuff : ModBuff
    {
		public static float DamageTakenMultiplier = 0.4f;
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = true;
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<ITDGlobalNPC>().haunted = true;
        }
    }
}