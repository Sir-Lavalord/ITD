using ITD.Content.NPCs;

namespace ITD.Content.Buffs.Debuffs
{
    public class FrostgripChilledBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<ITDGlobalNPC>().chilled = true;
		}
    }
}
