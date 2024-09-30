using ITD.Detours;
using ITD.Particles;
using ITD.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace ITD.Systems.Recruitment
{
    public class TownNPCRecruitmentLoader : DetourGroup
    {
        public static List<RecruitBehavior> recruitBehaviors = [];
        public override void Load()
        {
            foreach (Type t in ITD.Instance.Code.GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(RecruitBehavior))))
            {
                RecruitBehavior instance = (RecruitBehavior)Activator.CreateInstance(t);
                instance.SetStaticDefaults();
                recruitBehaviors.Add(instance);
            }
        }
        public override void Unload()
        {
            recruitBehaviors?.Clear();
        }
        public static RecruitBehavior SearchForNPCType(int type)
        {
            return recruitBehaviors.FirstOrDefault(b => b.NPCType == type, null);
        }
        public static bool CanBeRecruited(int type)
        {
            return recruitBehaviors.Any(b => b.NPCType == type);
        }
        public bool TryRecruit(int whoAmI, Player player)
        {
            NPC npc = Main.npc[whoAmI];
            if (CanBeRecruited(npc.type) && npc.TryGetGlobalNPC(out TownNPCRecruitmentRunner runner))
            {
                player.GetITDPlayer().SetRecruit(whoAmI);
                return true;
            }
            return false;
        }
        public void Unrecruit(int whoAmI, Player player)
        {
            NPC npc = Main.npc[whoAmI];
            if (CanBeRecruited(npc.type) && npc.TryGetGlobalNPC(out TownNPCRecruitmentRunner runner))
            {
                runner.isRecruitedBy = -1;
                player.GetITDPlayer().SetRecruit(-1);
            }
        }
    }
}
