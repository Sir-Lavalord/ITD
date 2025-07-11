using Terraria.Chat;
using Terraria.Localization;

namespace ITD.Utilities
{
    public static class Debugging
    {
        public static void Message(object client, object server, Color? clientColor = null, Color? serverColor = null)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(server.ToString()), serverColor ?? Color.White);
            if (!Main.dedServ)
                Main.NewText(client, clientColor ?? Color.White);
        }
        /// <summary>
        /// Same as <see cref="Message(object, object, Color?, Color?)"/> except it transmits one single message, prefixed by Client or Server.
        /// </summary>
        /// <param name="all"></param>
        public static void Message(object all, Color? clientColor = null, Color? serverColor = null) => Message($"Client: {all}", $"Server: {all}", clientColor, serverColor);
    }
}
