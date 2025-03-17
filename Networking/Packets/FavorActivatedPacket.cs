using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader.IO;

using ITD.Players;
using ITD.Utilities;
using ITD.Content.Items.Favors;

namespace ITD.Networking.Packets
{
    public sealed class FavorActivatedPacket : ITDPacket
    {
        public FavorActivatedPacket(Player player, Item FavorItem)
        {
            Writer.TryWriteSenderPlayer(player);
            ItemIO.Send(FavorItem, Writer, true);
        }
        public override void Read(BinaryReader reader, int sender)
        {
            if (!reader.TryReadSenderPlayer(sender, out var player))
            {
                return;
            }
            Item FavorItem = ItemIO.Receive(reader, true);
			if (FavorItem.ModItem is Favor favorItem)
				favorItem.UseFavor(player);
            if (Main.dedServ)
            {
                NetSystem.SendPacket(new FavorActivatedPacket(player, FavorItem), ignoreClient: sender);
            }
        }
    }
}
