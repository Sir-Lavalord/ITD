using ITD.Systems;
using System.IO;

namespace ITD.Networking.Packets
{
    public sealed class QueueRecruitmentPacket : ITDPacket
    {
        public QueueRecruitmentPacket(QueuedRecruitment request)
        {
            Writer.Write(request.NPC);
            Writer.Write(request.NPCType);
            Writer.WriteGuid(request.player);
        }
        public override void Read(BinaryReader reader, int sender)
        {
            QueuedRecruitment request = new
            (
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadGuid()
            );
            ITDSystem.recruitment.Enqueue(request);
        }
    }
}
