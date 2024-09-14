using ITD.Systems;
using Terraria;
using Terraria.ModLoader;

namespace ITD.Content.Buffs.Debuffs
{
    public class FavorFatigue : ModBuff
    {
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<FavorPlayer>().favorFatigue = true;
        }
    }
}
