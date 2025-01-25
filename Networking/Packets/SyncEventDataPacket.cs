using ITD.Content.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITD.Networking.Packets
{
    public sealed class SyncEventDataPacket : ITDPacket
    {
        public SyncEventDataPacket(sbyte eventType)
        {
            Writer.Write(eventType);
            EventsSystem.EventsByID[eventType].NetSend(Writer);
        }
        public override void Read(BinaryReader reader, int sender)
        {
            sbyte eventType = reader.ReadSByte();
            EventsSystem.EventsByID[eventType].NetReceive(reader);
        }
    }
}
