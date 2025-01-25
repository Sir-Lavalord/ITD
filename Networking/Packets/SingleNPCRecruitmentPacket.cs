using ITD.Systems.Recruitment;
using System;
using System.IO;
using Terraria;

namespace ITD.Networking.Packets
{
    public sealed class SingleNPCRecruitmentPacket : ITDPacket
    {
        public SingleNPCRecruitmentPacket(byte npc, Guid recruiter, RecruitData rData)
        {
            Writer.Write(npc);
            Writer.WriteGuid(recruiter);
            Writer.WriteRecruitmentData(rData);
        }
        public override void Read(BinaryReader reader, int sender)
        {
            NPC npc = Main.npc[reader.ReadByte()];
            Guid recruiter = reader.ReadGuid();
            RecruitData rData = reader.ReadRecruitmentData();
            if (npc.ModNPC is RecruitedNPC rNpc)
            {
                rNpc.Recruiter = recruiter;
                rNpc.recruitmentData = rData;
            }
        }
    }
}
