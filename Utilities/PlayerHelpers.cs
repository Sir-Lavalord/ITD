using ITD.Players;
using ITD.Systems;
using Microsoft.Xna.Framework;
using Terraria;

namespace ITD.Utilities
{
    public static class PlayerHelpers
    {
        public static ITDPlayer GetITDPlayer(this Player player) => player.GetModPlayer<ITDPlayer>();
        public static SnaptrapPlayer GetSnaptrapPlayer(this Player player) => player.GetModPlayer<SnaptrapPlayer>();
        public static bool IsLocalPlayer(this Player player) => player.whoAmI == Main.myPlayer;
        public static Vector2 LookDirection(this Player player) => (player.GetITDPlayer().MousePosition - player.Center).SafeNormalize(Vector2.Zero);

    }
}
