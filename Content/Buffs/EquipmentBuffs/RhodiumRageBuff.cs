namespace ITD.Content.Buffs.EquipmentBuffs
{
    public class RhodiumRageBuff : ModBuff
    {
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Generic) *= 1.1f;
			player.GetCritChance(DamageClass.Generic) += 10f;
        }
    }
}
