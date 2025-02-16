using ITD.Content.TileEntities;
using System;
using System.IO;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace ITD.Networking.Packets
{
    public sealed class SyncITDChestItemPacket : ITDPacket
    {
        // basically just MessageID.SyncChestItem
        public SyncITDChestItemPacket(int tileEntity, int slot)
        {
            ITDChestTE entity = TileEntity.ByID[tileEntity] as ITDChestTE;
            Item sendItem = entity[slot];
            Writer.Write((ushort)tileEntity);
            Writer.Write((byte)slot);
            ItemIO.Send(sendItem, Writer, true);
        }
        public override void Read(BinaryReader reader, int sender)
        {
            ushort tileEntity = reader.ReadUInt16();
            byte slot = reader.ReadByte();
            ITDChestTE entity = TileEntity.ByID[tileEntity] as ITDChestTE;
            ItemIO.Receive(entity[slot], reader, true);
            Recipe.FindRecipes(true);
            // replicate on clients
            if (Main.dedServ)
                NetSystem.SendPacket(new SyncITDChestItemPacket(tileEntity, slot), ignoreClient: sender);
        }
    }
}
