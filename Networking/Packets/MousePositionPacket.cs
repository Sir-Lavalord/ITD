using ITD.Players;
using ITD.Utilities;
using System.IO;
using Terraria;
using Terraria.ID;

namespace ITD.Networking.Packets
{
    public sealed class MousePositionPacket : ITDPacket
    {
        public MousePositionPacket(Player player)
        {
            var modPlayer = player.GetITDPlayer();
            Writer.TryWriteSenderPlayer(player);
            Writer.WriteVector2(modPlayer.MousePosition);
        }
        public override void Read(BinaryReader reader, int sender)
        {
            if (!reader.TryReadSenderPlayer(sender, out var player) || !player.TryGetModPlayer(out ITDPlayer modPlayer))
            {
                return;
            }
            modPlayer.MousePosition = reader.ReadVector2();
            if (Main.dedServ)
            {
                NetSystem.SendPacket(new MousePositionPacket(player), ignoreClient: sender);
            }
        }
    }
}
