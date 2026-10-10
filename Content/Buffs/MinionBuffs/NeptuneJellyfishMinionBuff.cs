using Terraria;
using Terraria.ModLoader;
using ITD.Content.Projectiles.Friendly.Summoner;
using ITD.Utilities.Placeholders;

namespace ITD.Content.Buffs.MinionBuffs
{
    public class NeptuneJellyfishBuff : ModBuff
    {
        public override string Texture => Placeholder.PHBuff;
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<NeptuneJellyfishMinion>()] > 0)
            {
                player.buffTime[buffIndex] = 3600;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}