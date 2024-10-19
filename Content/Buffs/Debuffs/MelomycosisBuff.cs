using ITD.Utilities;
using Terraria;
using Terraria.ModLoader;

namespace ITD.Content.Buffs.Debuffs
{
    public class MelomycosisBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetITDPlayer().melomycosis = true;
        }
    }
}
