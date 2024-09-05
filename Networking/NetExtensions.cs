using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;

namespace ITD.Networking
{
    public static class NetExtensions // from overhaul. credits to Mirsario
    {
        public static void TryWriteSenderPlayer(this BinaryWriter writer, Player player)
        {
            if (Main.netMode == NetmodeID.Server)
            {
                writer.Write((byte)player.whoAmI);
            }
        }

        public static bool TryReadSenderPlayer(this BinaryReader reader, int sender, out Player player)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                sender = reader.ReadByte();
            }

            player = Main.player[sender];

            return player != null && player.active;
        }
    }
}
