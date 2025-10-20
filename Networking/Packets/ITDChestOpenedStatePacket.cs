using ITD.Content.TileEntities;
using System.IO;
using Terraria.DataStructures;

namespace ITD.Networking.Packets;

public sealed class ITDChestOpenedStatePacket : ITDPacket
{
    /// <summary>
    /// Fun fact: 255 is reserved for the server in multiplayer so we can avoid sending a short or a bool
    /// </summary>
    /// <param name="player"></param>
    public ITDChestOpenedStatePacket(ushort te, byte player = 255)
    {
        Writer.Write(te);
        Writer.Write(player);
    }
    public override void Read(BinaryReader reader, int sender)
    {
        ushort te = reader.ReadUInt16();
        byte player = reader.ReadByte();
        if (TileEntity.ByID.TryGetValue(te, out var t) && t is ITDChestTE chest)
        {
            if (player != 255)
                chest.OpenedBy = player;
            else
                chest.OpenedBy = -1;
        }
        if (Main.dedServ)
            NetSystem.SendPacket(new ITDChestOpenedStatePacket(te, player));
    }
}
