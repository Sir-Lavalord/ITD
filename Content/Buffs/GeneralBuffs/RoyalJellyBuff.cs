using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace ITD.Content.Buffs.GeneralBuffs
{
    public class RoyalJellyBuff : ModBuff
    {
        public static float DefReduce = 6;

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.statDefense += 6;
            player.GetAttackSpeed<MeleeDamageClass>() += 0.05f;
            player.GetCritChance<RangedDamageClass>() += 0.05f;
            player.GetDamage<MagicDamageClass>() += 0.05f;
            player.honey = true;
            player.honeyWet = true;
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
        }
    }
}