using ITD.Players;
using Terraria;

namespace ITD.Utilities
{
    public static class PlayerHelpers
    {
        public static ITDPlayer GetITDPlayer(this Player player)
        {
            return player.GetModPlayer<ITDPlayer>();
        }
    }
}
