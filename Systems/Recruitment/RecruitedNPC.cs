using ITD.Utilities.Placeholders;
using ITD.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ReLogic.Content;
using System;
using Terraria.GameContent;
using Terraria.ModLoader.IO;
using Terraria.DataStructures;
using System.IO;
using ITD.Particles;
using ITD.Particles.Testing;
using ITD.Content.NPCs;
using ITD.Content.UI;
using Terraria.Audio;

namespace ITD.Systems.Recruitment
{
    public class RecruitedNPC : ITDNPC
    {
        public int Recruiter = 0;
        public int originalType = -1;
        public int armFrame = 0;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 15;
        }
        public override void SetDefaults()
        {
            NPC.width = 18;
            NPC.height = 40;
            NPC.lifeMax = 100;
            NPC.friendly = true;
        }
        public RecruitmentData GetRecruitmentData()
        {
            if (Recruiter < 0)
            {
                Recruiter = TownNPCRecruitmentLoader.TryFindRecruiterOf(originalType).whoAmI;
            }
            return Main.player[(int)Recruiter].GetITDPlayer().recruitmentData;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Recruiter);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Recruiter = reader.ReadInt32();
        }
        public override void SaveData(TagCompound tag)
        {
            tag["originalType"] = originalType;
        }
        public override void LoadData(TagCompound tag)
        {
            originalType = tag.GetInt("originalType");
        }
        public override void OnRightClick(Player player)
        {
            if (GetRecruitmentData().Recruiter != player.whoAmI)
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
        public override void ModifyTypeName(ref string typeName) => typeName = GetRecruitmentData().FullName.ToString();

        // i'm not even going to try to code AI cuz i suck at it but i had the idea of having a common AI method that handles player following and such.
        // these smaller methods would handle stuff specific to that npc (obviously).
        // the common method would also be called in these smaller methods (just in case something needs to be done conditionally)
        public void DoMerchantAI()
        {
            if (Main.GameUpdateCount % 20 == 0)
            {
                ParticleSystem.NewParticle<ShaderTestParticle>(NPC.Center, Vector2.Zero,1);
            }
        }
        public override void AI()
        {
            if (Recruiter < 0 || GetRecruitmentData().FullName is null)
            {
                TownNPCRecruitmentLoader.Unrecruit(NPC.whoAmI);
                return;
            }

            Player player = Main.player[Recruiter];
            // testing AI
            NPC.velocity.X = Math.Sign(player.Center.X - NPC.Center.X)*2f;
            NPC.spriteDirection = NPC.direction = NPC.velocity.X > 0 ? 1 : -1;
            NPCHelpers.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height);
            ExternalRecruitmentData extData = TownNPCRecruitmentLoader.GetExternalRecruitmentData(originalType);
            if (extData?.AIDelegate != null) // try to run custom mod AI
            {
                extData.AIDelegate(NPC, player); // here's an idea: once common AI methods like FollowPlayer() and Jump() or others, send them through the delegate here to be captured in the other mod and used
            }
            else
            {
                switch (originalType) // AI type switch
                {
                    case NPCID.Merchant:
                        DoMerchantAI();
                        break;
                }
            }
        }
        private static int MapBaseFrameToArmFrame(int currentFrame) // is this stupid? am i stupid?
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
            Asset<Texture2D> tex = null;
            ExternalRecruitmentData extData = TownNPCRecruitmentLoader.GetExternalRecruitmentData(originalType);
            string pathToTypeTexture = Texture + "_" + originalType;
            if (extData != null)
            {
                pathToTypeTexture = extData.TexturePath;
            }
            string pathToShimmerTexture = pathToTypeTexture + "_Shimmer";
            if (GetRecruitmentData().IsShimmered) // try to load shimmer texture
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
            Vector2 offset = new(0f, -4f); // random ahh magic number?
            Main.EntitySpriteDraw(tex.Value, NPC.Center - screenPos + offset, armQuad, drawColor, 0f, baseOrigin, 1f, flip);
            Main.EntitySpriteDraw(tex.Value, NPC.Center - screenPos + offset, baseQuad, drawColor, 0f, baseOrigin, 1f, flip);
            return false;
        }
    }
}
