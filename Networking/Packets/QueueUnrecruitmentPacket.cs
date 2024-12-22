using ITD.Systems;
using System;
using System.IO;

namespace ITD.Networking.Packets
{
    public sealed class QueueUnrecruitmentPacket : ITDPacket
    {
        public QueueUnrecruitmentPacket(Guid plr)
        {
            Writer.WriteGuid(plr);
        }
        public override void Read(BinaryReader reader, int sender)
        {
            Guid plr = reader.ReadGuid();

            ITDSystem.unrecruitment.Enqueue(new QueuedUnrecruitment(plr));
        }
    }
}
