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
                //if (Main.ShopHelper.GetShoppingSettings(player, npc).PriceAdjustment < 0.82f) // if happiness is max
                if (true is true) // just for testing yaehahh
                {
                    data.WhoAmI = npc.whoAmI;
                    data.FullName = npc.FullName;
                    data.OriginalType = npc.type;
                    data.Recruiter = player.whoAmI;
                    npc.Transform(ModContent.NPCType<RecruitedNPC>());
                    npc.ai[0] = player.whoAmI;
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
