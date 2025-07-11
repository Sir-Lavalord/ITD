using ITD.Content.NPCs;

namespace ITD.Content.Buffs.Debuffs
{
    public class ToastedBuff : ModBuff
    {
        public const int DefenseReductionPercent = 25;
        public static float DefenseMultiplier = 1 - DefenseReductionPercent / 100f;
        public static float DamageMultiplier = 0.25f;
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
