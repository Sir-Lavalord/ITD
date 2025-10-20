using ITD.Content.NPCs;
using ITD.Utilities.Placeholders;

namespace ITD.Content.Buffs.Debuffs;

public class ToppledDebuff : ModBuff
{
    public override string Texture => Placeholder.PHDebuff;

    public override void SetStaticDefaults()
    {
    }
    public override void Update(NPC npc, ref int buffIndex)
    {
        npc.GetGlobalNPC<ITDGlobalNPC>().toppled = true;
    }

}
