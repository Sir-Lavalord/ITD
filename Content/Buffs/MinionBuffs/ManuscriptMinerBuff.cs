using ITD.Content.Projectiles.Friendly.Summoner;
using ITD.Utilities.Placeholders;
using Terraria;
using Terraria.ModLoader;

namespace ITD.Content.Buffs.MinionBuffs
{
    public class ManuscriptMinerBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<ManuscriptMinerProj>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}