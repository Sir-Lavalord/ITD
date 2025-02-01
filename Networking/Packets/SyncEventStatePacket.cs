using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITD.Content.Events;
using Terraria;

namespace ITD.Networking.Packets
{
    /// <summary>
    /// This should only be sent on the server
    /// </summary>
    public sealed class SyncEventStatePacket : ITDPacket
    {
        public SyncEventStatePacket(bool start)
        {
            Writer.Write(EventsSystem.ActiveEvent);
            Writer.Write(start);
        }
        public override void Read(BinaryReader reader, int sender)
        {
            sbyte activeEvent = reader.ReadSByte();
            bool start = reader.ReadBoolean();
            if (start)
                EventsSystem.BeginEvent(activeEvent);
            else
                EventsSystem.StopEvent(activeEvent);
            if (Main.dedServ)
                NetSystem.SendPacket(new SyncEventStatePacket(start), sender);
        }
    }
}
