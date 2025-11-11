using ITD.Content.NPCs;

namespace ITD.Content.Buffs.Debuffs;

public class MelomycosisBuff : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.buffNoSave[Type] = true;
        Main.debuff[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;
    }
    public override void Update(Player player, ref int buffIndex)
    {
        player.ITD().melomycosis = true;
    }
    public override void Update(NPC npc, ref int buffIndex)
    {
        npc.GetGlobalNPC<ITDGlobalNPC>().melomycosis = true;
    }
}
