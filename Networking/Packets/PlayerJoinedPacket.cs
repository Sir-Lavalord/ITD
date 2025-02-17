using ITD.Content.TileEntities;
using ITD.Players;
using ITD.Systems.Recruitment;
using ITD.Utilities;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace ITD.Networking.Packets
{
    public sealed class PlayerJoinedPacket : ITDPacket
    {
        public PlayerJoinedPacket(Player player)
        {
            var modPlayer = player.GetITDPlayer();
            Writer.TryWriteSenderPlayer(player);
            Writer.WriteGuid(modPlayer.guid);

        }
        public override void Read(BinaryReader reader, int sender)
        {
            if (!reader.TryReadSenderPlayer(sender, out var player) || !player.TryGetModPlayer(out ITDPlayer modPlayer))
            {
                return;
            }
            modPlayer.guid = reader.ReadGuid();

            // send data from server to clients (selectively, single clients)
            if (Main.dedServ)
            {
                NetSystem.SendPacket(new PlayerJoinedPacket(player), ignoreClient: sender);
                foreach (var npc in Main.npc.Where(n => n.ModNPC is RecruitedNPC))
                {
                    RecruitedNPC rNPC = npc.ModNPC as RecruitedNPC;
                    NetSystem.SendPacket(new SingleNPCRecruitmentPacket((byte)npc.whoAmI, rNPC.Recruiter, rNPC.recruitmentData), toClient: player.whoAmI);
                }
            }
        }
    }
}
