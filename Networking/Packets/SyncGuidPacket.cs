using ITD.Players;
using ITD.Utilities;
using System.IO;
using Terraria;
using Terraria.ID;

namespace ITD.Networking.Packets
{
    public sealed class SyncGuidPacket : ITDPacket
    {
        public SyncGuidPacket(Player player)
        {
            var modPlayer = player.GetITDPlayer();
            Writer.TryWriteSenderPlayer(player);
            Writer.WriteGuid(modPlayer.guid);
        }
        public override void Read(BinaryReader reader, int sender)
        {
            if (!reader.TryReadSenderPlayer(sender, out var player) || !player.TryGetModPlayer(out ITDPlayer modPlayer))
            {
                return;
            }
            modPlayer.guid = reader.ReadGuid();
            if (Main.netMode == NetmodeID.Server)
            {
                NetSystem.SendPacket(new SyncGuidPacket(player), ignoreClient: sender);
            }
        }
    }
}
