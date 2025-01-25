using ITD.Systems;
using ITD.Systems.Recruitment;
using System;
using System.IO;

namespace ITD.Networking.Packets
{
    public sealed class SyncRecruitmentPacket : ITDPacket
    {
        public SyncRecruitmentPacket()
        {
            Writer.Write((ushort)ITDSystem.recruitmentData.Count);
            foreach (var pair in ITDSystem.recruitmentData)
            {
                Writer.WriteGuid(pair.Key);
                Writer.WriteRecruitmentData(pair.Value);
            }
        }
        public override void Read(BinaryReader reader, int sender)
        {
            ITDSystem.recruitmentData.Clear();

            ushort count = reader.ReadUInt16();

            for (int i = 0; i < count; i++)
            {
                Guid key = reader.ReadGuid();
                RecruitData value = reader.ReadRecruitmentData();
                ITDSystem.recruitmentData[key] = value;
            }
        }
    }
}
