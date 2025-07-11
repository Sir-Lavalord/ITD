using ITD.Systems;

namespace ITD.Content.Buffs.GeneralBuffs
{
    public class WeaponImbueMelomycosis : ModBuff
    {
        public override void SetStaticDefaults()
        {
            BuffID.Sets.IsAFlaskBuff[Type] = true;
            Main.meleeBuff[Type] = true;
            Main.persistentBuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<WeaponEnchantmentPlayer>().melomycosisImbue = true;
            player.MeleeEnchantActive = true;
        }
    }
}