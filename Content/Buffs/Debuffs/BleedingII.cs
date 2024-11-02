using ITD.Content.NPCs;
using ITD.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Buffs.Debuffs
{
    public class BleedingII : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
            BuffID.Sets.IsATagBuff[Type] = true;
        }
        public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<ITDGlobalNPC>().bleedingII = true;
		}
    }
}
