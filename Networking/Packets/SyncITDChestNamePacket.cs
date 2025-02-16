using ITD.Content.TileEntities;
using System.IO;
using Terraria.DataStructures;

namespace ITD.Networking.Packets
{
    public sealed class SyncITDChestNamePacket : ITDPacket
    {
        public SyncITDChestNamePacket(int tileEntity)
        {
            ITDChestTE chest = TileEntity.ByID[tileEntity] as ITDChestTE;
            Writer.Write((ushort)tileEntity);
            Writer.Write(chest.StorageName);
        }
        public override void Read(BinaryReader reader, int sender)
        {
            ITDChestTE chest = TileEntity.ByID[reader.ReadUInt16()] as ITDChestTE;
            chest.StorageName = reader.ReadString();
            if (Main.dedServ)
                NetSystem.SendPacket(new SyncITDChestNamePacket(chest.ID), ignoreClient: sender);
        }
    }
}
