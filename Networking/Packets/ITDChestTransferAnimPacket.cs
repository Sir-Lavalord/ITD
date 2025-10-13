using ITD.Content.TileEntities;
using System.IO;
using Terraria.DataStructures;

namespace ITD.Networking.Packets;

public sealed class ITDChestTransferAnimPacket : ITDPacket
{
    public ITDChestTransferAnimPacket(ushort ID)
    {
        Writer.Write(ID);
    }
    public override void Read(BinaryReader reader, int sender)
    {
        ushort ID = reader.ReadUInt16();
        if (TileEntity.ByID.TryGetValue(ID, out var t) && t is ITDChestTE chest)
        {
            chest.OpenToReceiveParticles();
        }
    }
}
