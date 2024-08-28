using ITD.Content.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace ITD.Content.Buffs.Debuffs
{
    public class ToastedBuff : ModBuff
    {
        public const int DefenseReductionPercent = 15;
        public static float DefenseMultiplier = 1 - DefenseReductionPercent / 100f;
        public static float DamageMultiplier = 0.15f;
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = true;
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<ITDGlobalNPC>().toasted = true;
        }
    }
}
