using ITD.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Buffs.EquipmentBuffs
{
    public class CorruptAntidoteBuff : ModBuff
    {
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Magic) *= 1.25f;
            player.manaCost *= 0.85f;
        }
    }
}
