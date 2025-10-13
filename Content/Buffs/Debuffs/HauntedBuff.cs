using ITD.Content.NPCs;

namespace ITD.Content.Buffs.Debuffs;

public class HauntedBuff : ModBuff
{
    public const float DamageTakenMultiplier = 0.4f;
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
