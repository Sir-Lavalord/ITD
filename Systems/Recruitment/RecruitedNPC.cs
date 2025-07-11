using ITD.Utilities;
using System;
using Terraria.GameContent;
using Terraria.ModLoader.IO;
using System.IO;
using ITD.Content.NPCs;
using ITD.Content.UI;
using Terraria.Audio;
using System.Linq;

namespace ITD.Systems.Recruitment
{
    public class RecruitedNPC : ITDNPC
    {
        private enum ActionState : byte
        {
            Active,
            Recovering,
            WaitingForRecruiter,
        }
        public Guid Recruiter = Guid.Empty;
        public RecruitData recruitmentData = RecruitData.Invalid;
        public int armFrame = 0;
        public int timeToBeUnrecruited = 2000;
        public override void SetStaticDefaultsSafe()
        {
            HiddenFromBestiary = true;
            Main.npcFrameCount[Type] = 15;
        }
        public override void SetDefaults()
        {
            NPC.width = 18;
            NPC.height = 40;
            NPC.lifeMax = 100;
            NPC.friendly = true;
        }
        public override bool NeedSaving() => true;
        public override void SaveData(TagCompound tag)
        {
            tag["recruiter"] = Recruiter.ToByteArray();
        }
        public override void LoadData(TagCompound tag)
        {
            Recruiter = new Guid(tag.GetByteArray("recruiter"));
        }
        private ActionState AIState = ActionState.Active;
        public override void OnRightClick(Player player)
        {
            if (!RecruiterExists || PlayerHelpers.FromGuid(Recruiter).whoAmI != player.whoAmI)
                return;
            UnrecruitmentGui gui = UILoader.GetUIState<UnrecruitmentGui>();
            if (gui.isOpen)
            {
                gui.Close();
                SoundEngine.PlaySound(SoundID.MenuClose);
                return;
            }
            gui.Open(NPC.Center, NPC.whoAmI);
            SoundEngine.PlaySound(SoundID.MenuTick);
        }
        public override void ModifyTypeName(ref string typeName)
        {
            RecruitData recruitData = ITDSystem.recruitmentData.Values.FirstOrDefault(v => v.OriginalType == recruitmentData.OriginalType, RecruitData.Invalid);
            if (recruitData.INVALIDDATA)
            {
                typeName = "";
                return;
            }
            typeName = recruitData.FullName.ToString();
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write((byte)AIState);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            AIState = (ActionState)reader.ReadByte();
        }
        public bool RecruiterExists => Main.player.Any(p => p.Exists() && ITDSystem.recruitmentData.TryGetValue(p.GetITDPlayer().guid, out RecruitData recDat) && recDat.OriginalType == recruitmentData.OriginalType);

        public void DoMerchantAI()
        {
            if (Main.GameUpdateCount % 20 == 0)
            {
                //ParticleSystem.NewEmitter<ShaderTestParticle>(NPC.Center, Vector2.Zero,1);
            }
        }
        public override void AI()
        {
            if (ITDSystem.recruitmentData.TryGetValue(Recruiter, out RecruitData rd))
            {
                recruitmentData = rd;
            }
            if (!RecruiterExists)
            {
                AIState = ActionState.WaitingForRecruiter;
                timeToBeUnrecruited--;
                if (timeToBeUnrecruited <= 0)
                {
                    TownNPCRecruitmentLoader.ServerUnrecruit(NPC.whoAmI);
                }
                return;
            }
            else
            {
                AIState = ActionState.Active;
                timeToBeUnrecruited = 2000;
            }

            Player player = PlayerHelpers.FromGuid(Recruiter);
            // testing AI
            NPC.velocity.X = Math.Sign(player.Center.X - NPC.Center.X)*2f;
            NPC.spriteDirection = NPC.direction = NPC.velocity.X > 0 ? 1 : -1;
            StepUp();
            ExternalRecruitmentData extData = TownNPCRecruitmentLoader.GetExternalRecruitmentData(recruitmentData.OriginalType);
            if (extData?.AIDelegate != null) // try to run custom mod AI
            {
                extData.AIDelegate(NPC, player);
            }
            else
            {
                switch ((short)recruitmentData.OriginalType) // AI type switch
                {
                    case NPCID.Merchant:
                        DoMerchantAI();
                        break;
                }
            }
        }
        private static int MapBaseFrameToArmFrame(int currentFrame)
        {
            return currentFrame switch
            {
                2 or 3 or 4 => 1,
                1 or 5 or 6 or 7 or 8 or 13 or 14 => 2,
                9 or 10 or 11 => 3,
                12 => 4,
                _ => 0,
            };
        }
        public override void FindFrame(int frameHeight)
        {
            int walkStartFrame = 1;
            int finalWalkFrame = Main.npcFrameCount[Type] - 1;

            int frameSpeed = 3;
            if (Math.Abs(NPC.velocity.X) < float.Epsilon) // if x velocity is 0
            {
                NPC.frameCounter = 0;
                NPC.frame.Y = 0;
            }
            else
            {
                NPC.frameCounter += 1f;
                if (NPC.frameCounter > frameSpeed)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y += frameHeight;

                    if (NPC.frame.Y > finalWalkFrame * frameHeight)
                    {
                        NPC.frame.Y = walkStartFrame * frameHeight;
                    }
                }
            }
            int currentFrame = NPC.frame.Y / frameHeight;
            armFrame = MapBaseFrameToArmFrame(currentFrame);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            RecruitData recruitData = ITDSystem.recruitmentData.Values.FirstOrDefault(v => v.OriginalType == recruitmentData.OriginalType, RecruitData.Invalid);
            Asset<Texture2D> tex = null;
            ExternalRecruitmentData extData = TownNPCRecruitmentLoader.GetExternalRecruitmentData(recruitmentData.OriginalType);
            string pathToTypeTexture = Texture + "_" + recruitmentData.OriginalType;
            if (extData != null)
            {
                pathToTypeTexture = extData.TexturePath;
            }
            string pathToShimmerTexture = pathToTypeTexture + "_Shimmer";
            if (recruitData.Shimmered) // try to load shimmer texture
            {
                ModContent.RequestIfExists(pathToShimmerTexture, out tex);
            }
            if (tex == null && !ModContent.RequestIfExists(pathToTypeTexture, out tex)) // try to load type-specific texture
            {
                tex = TextureAssets.Npc[Type]; // if type-specific texture doesn't exist, get default texture
            }
            int framesX = 2;
            int framesY = Main.npcFrameCount[Type];
            Rectangle vR = NPC.frame;
            Rectangle baseQuad = new(vR.X, vR.Y, vR.Width/2, vR.Height);
            Rectangle armQuad = tex.Frame(framesX, framesY, 1, armFrame);
            Vector2 baseOrigin = new (tex.Width() / framesX / 2, tex.Height() / framesY / 2);
            SpriteEffects flip = NPC.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Vector2 offset = new(0f, -4f);
            Main.EntitySpriteDraw(tex.Value, NPC.Center - screenPos + offset, armQuad, drawColor, 0f, baseOrigin, 1f, flip);
            Main.EntitySpriteDraw(tex.Value, NPC.Center - screenPos + offset, baseQuad, drawColor, 0f, baseOrigin, 1f, flip);
            return false;
        }
    }
}
