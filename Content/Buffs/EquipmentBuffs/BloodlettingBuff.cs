using ITD.Content.Items.Accessories.Master;

namespace ITD.Content.Buffs.EquipmentBuffs
{
    public class BloodlettingBuff : ModBuff
    {		
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
        }
		
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<ProphylaxisPlayer>().bloodletting = true;
        }
    }
}
