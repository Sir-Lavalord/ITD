using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
