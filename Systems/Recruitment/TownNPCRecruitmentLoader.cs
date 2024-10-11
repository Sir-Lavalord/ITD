using ITD.Detours;
using ITD.Particles;
using ITD.Utilities;
using System;
using Microsoft.Xna.Framework;
using System.Linq;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Terraria.GameContent;
using Terraria.Enums;
using System.Collections.Generic;

namespace ITD.Systems.Recruitment
{
    public delegate void RecruitmentAIDelegate(NPC npc, Player recruiter);
    public class RecruitmentData
    {
        public int WhoAmI { get; set; } = -1;
        public string FullName { get; set; } = "";
        public int OriginalType { get; set; } = -1;
        public int Recruiter { get; set; } = -1;
        public bool IsShimmered { get; set; } = false;
        public void Clear()
        {
            WhoAmI = -1;
            FullName = string.Empty;
            OriginalType = -1;
            Recruiter = -1;
            IsShimmered = false;
        }
    }
    public class ExternalRecruitmentData (Delegate aiDelegate, Mod modInstance, string texturePath)
    {
        public Delegate AIDelegate { get; set; } = aiDelegate;
        public Mod ModInstance { get; set; } = modInstance;
        public string TexturePath {  get; set; } = texturePath;
    }
    public static class TownNPCRecruitmentLoader
    {
        private static Dictionary<int, ExternalRecruitmentData> recruitmentDataRegistry = [];
        private readonly static int[] NPCsThatCanBeRecruited =
        [
            NPCID.Merchant
        ];
        public static void RegisterRecruitmentData(Mod mod, int npcType, Delegate recruitmentAI, string texturePath)
        {
            ITD.Instance.Logger.Info("Trying to add recruitment data for NPC of ID " +  npcType + " from mod " + mod.Name);
            recruitmentDataRegistry[npcType] = new ExternalRecruitmentData(recruitmentAI, mod, texturePath);
        }
        public static ExternalRecruitmentData GetExternalRecruitmentData(int npcType)
        {
            return recruitmentDataRegistry.TryGetValue(npcType, out ExternalRecruitmentData data) ? data : null;
        }
        public static bool CanBeRecruited(int type) => NPCsThatCanBeRecruited.Contains(type) || recruitmentDataRegistry.ContainsKey(type);
        public static Player TryFindRecruiterOf(int type) => Main.player.FirstOrDefault(p => p.GetITDPlayer().recruitmentData.OriginalType == type, Main.player[0]);
        public static bool TryRecruit(int whoAmI, Player player)
        {
            NPC npc = Main.npc[whoAmI];
            RecruitmentData data = player.GetITDPlayer().recruitmentData;
            if (CanBeRecruited(npc.type))
            {
                //if (Main.ShopHelper.GetShoppingSettings(player, npc).PriceAdjustment < 0.82f) // if happiness is max
                if (true is true || true is !false && true) // just for testing yaehahh
                {
                    data.WhoAmI = npc.whoAmI;
                    data.FullName = npc.FullName;
                    data.OriginalType = npc.type;
                    data.Recruiter = player.whoAmI;
                    data.IsShimmered = npc.IsShimmerVariant;
                    npc.Transform(ModContent.NPCType<RecruitedNPC>());
                    if (npc.ModNPC is RecruitedNPC rNpc)
                    {
                        rNpc.Recruiter = player.whoAmI;
                        rNpc.originalType = data.OriginalType;
                    }
                    Main.NewText("Recruited " + data.FullName + "!", Color.LimeGreen); // replace with localizable stuff
                    return true;
                }
                else
                {
                    Main.NewText(npc.FullName + " is not at maximum happiness!", Color.Red); // replace with lolcalzic stuf
                }
            }
            Main.NewText(npc.FullName + " cannot be recruited!", Color.Red); //a plapa local sutf
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
