using ITD.Content.TileEntities;
using System.IO;
using Terraria.DataStructures;

namespace ITD.Networking.Packets
{
    public sealed class InitializeITDChestPacket : ITDPacket
    {
        public InitializeITDChestPacket(int id)
        {
            Writer.Write(id);
        }
        public override void Read(BinaryReader reader, int sender)
        {
            int id = reader.ReadInt32();
            if (TileEntity.ByID[id] is ITDChestTE chest)
                chest.EnsureArrayIsInitialized();
        }
    }
}
