using ITD.Players;
using Terraria;

namespace ITD.Utils
{
    public static class PlayerHelpers
    {
        public static ITDPlayer GetITDPlayer(this Player player)
        {
            return player.GetModPlayer<ITDPlayer>();
        }
    }
}
