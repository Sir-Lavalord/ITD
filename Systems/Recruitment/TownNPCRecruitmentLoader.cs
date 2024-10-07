using ITD.Detours;
using ITD.Particles;
using ITD.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;

namespace ITD.Systems.Recruitment
{
    public class RecruitmentData
    {
        public int WhoAmI { get; set; } = -1;
        public string FullName { get; set; }
        public int OriginalType { get; set; } = -1;
        public int Recruiter { get; set; } = -1;
        public void Clear()
        {
            WhoAmI = -1;
            FullName = string.Empty;
            OriginalType = -1;
            Recruiter = -1;
        }
    }
    public static class TownNPCRecruitmentLoader
    {
        private readonly static int[] NPCsThatCanBeRecruited = 
        [
            NPCID.Merchant
        ];
        public static bool CanBeRecruited(int type) => NPCsThatCanBeRecruited.Contains(type);
        public static bool TryRecruit(int whoAmI, Player player)
        {
            NPC npc = Main.npc[whoAmI];
            RecruitmentData data = player.GetITDPlayer().recruitmentData;
            if (CanBeRecruited(npc.type))
            {
                data.WhoAmI = npc.whoAmI;
                data.FullName = npc.FullName;
                data.OriginalType = npc.type;
                data.Recruiter = player.whoAmI;
                npc.Transform(ModContent.NPCType<RecruitedNPC>());
                return true;
            }
            return false;
        }
        public static void Unrecruit(int whoAmI, Player player)
        {
            NPC npc = Main.npc[whoAmI];
            RecruitmentData data = player.GetITDPlayer().recruitmentData;
            npc.Transform(data.OriginalType);
            data.Clear();
        }
    }
}
