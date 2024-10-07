using ITD.Utilities.Placeholders;
using ITD.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Systems.Recruitment
{
    public class RecruitedNPC : ModNPC
    {
        public ref float Recruiter => ref NPC.ai[0]; // index of the player who recruited this NPC
        public override string Texture => Placeholder.PHGeneric;
        public override void SetStaticDefaults()
        {
            
        }
        public override void SetDefaults()
        {
            NPC.townNPC = true;
            NPC.width = 24;
            NPC.height = 44;
            NPC.lifeMax = 100;
            NPC.friendly = true;
        }
        public override void ModifyTypeName(ref string typeName) => typeName = Main.player[(int)Recruiter].GetITDPlayer().recruitmentData.FullName;

        // this implementation using the vanilla interface sucks because it requires NPC.townNPC to be set to true. is there something better?
        public override string GetChat()
        {
            return base.GetChat();
        }
        public override void SetChatButtons(ref string button, ref string button2)
        {
            button = "Unrecruit";
        }
        public override void OnChatButtonClicked(bool firstButton, ref string shopName)
        {
            if (firstButton)
            {
                TownNPCRecruitmentLoader.Unrecruit(NPC.whoAmI, Main.player[(int)Recruiter]);
            }
        }
    }
}
