using ITD.Players;
using ITD.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ITD.Systems.Recruitment
{
    public class TownNPCRecruitmentRunner : GlobalNPC
    {
        public int isRecruitedBy = -1;
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.isLikeATownNPC;
        public Player GetRecruiter(NPC npc)
        {
            return Main.player.FirstOrDefault(p => p.TryGetModPlayer(out ITDPlayer player) && player.NPCRecruit == npc.whoAmI, null);
        }
        public RecruitBehavior GetBehavior(NPC npc)
        {
            if (GetRecruiter(npc) is null)
                return null;
            return TownNPCRecruitmentLoader.SearchForNPCType(npc.type);
        }
        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            RecruitBehavior behavior = GetBehavior(npc);
            if (behavior != null)
            {
                binaryWriter.Write(behavior.AITimer);
            }
        }
        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            RecruitBehavior behavior = GetBehavior(npc);
            if (behavior != null)
            {
                binaryReader.ReadSingle();
            }
        }
        public override bool PreAI(NPC npc)
        {
            RecruitBehavior behavior = GetBehavior(npc);
            if (behavior is null)
                return true;
            behavior?.Run(npc, GetRecruiter(npc));
            behavior?.UpdateAttack(npc);
            return false;
        }
        public override void FindFrame(NPC npc, int frameHeight)
        {
            RecruitBehavior behavior = GetBehavior(npc);
            if (behavior is null)
                return;
            behavior?.FindFrame(npc, frameHeight);
        }
    }
}
