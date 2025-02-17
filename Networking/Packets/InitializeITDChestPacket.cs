using ITD.Content.TileEntities;
using ITD.Systems.DataStructures;
using System.IO;
using Terraria.DataStructures;

namespace ITD.Networking.Packets
{
    public sealed class InitializeITDChestPacket : ITDPacket
    {
        public InitializeITDChestPacket(int id, Point8 dimensions)
        {
            Writer.Write(id);
            Writer.WritePoint8(dimensions);
        }
        public override void Read(BinaryReader reader, int sender)
        {
            int id = reader.ReadInt32();
            Point8 dimens = reader.ReadPoint8();
            if (TileEntity.ByID[id] is ITDChestTE chest)
            {
                chest.StorageDimensions = dimens;
                chest.EnsureArrayIsInitialized();
            }
        }
    }
}
