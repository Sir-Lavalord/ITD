namespace ITD.Content.Buffs.EquipmentBuffs;

public class CrimsonAntidoteBuff : ModBuff
{
    public override void Update(Player player, ref int buffIndex)
    {
        player.lifeRegen += 5;
    }
}
