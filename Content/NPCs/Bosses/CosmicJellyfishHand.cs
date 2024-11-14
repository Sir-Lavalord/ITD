using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ITD.Utilities;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using ITD.Particles.CosJel;
using ITD.Particles;
using System.Runtime.CompilerServices;
using ITD.Utilities.EntityAnim;
namespace ITD.Content.NPCs.Bosses
{
    public class CosmicJellyfishHand : ModNPC
    {
        // Fargo echamp clone tempo
        // sorry mirrorman but my ahh isn't gonna keep reading NPC.ai[] everywhere whilst having no idea wtf is happening, have some ref properties and enums :pray:
        private enum ActionState
        {
            Waiting,
            Anticipate,
            ThrowPunch
        }
        private ActionState AIState { get { return (ActionState)NPC.ai[0]; } set { NPC.ai[0] = (float)value; } }
        public bool IsLeftHand => (int)NPC.ai[3] == 1;
        public int CosJelIndex => (int)NPC.ai[2];
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 7;
            NPCID.Sets.TrailCacheLength[NPC.type] = 6;
            NPCID.Sets.TrailingMode[NPC.type] = 1;
            NPCID.Sets.CantTakeLunchMoney[NPC.type] = true;
            NPCID.Sets.ImmuneToRegularBuffs[Type] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 100;
            NPC.height = 100;
            NPC.damage = 130;
            NPC.defense = 80;
            NPC.lifeMax = 6000;
            NPC.HitSound = SoundID.NPCHit41;
            NPC.DeathSound = SoundID.NPCDeath44;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0f;
            NPC.lavaImmune = true;
            NPC.aiStyle = -1;

            NPC.trapImmune = true;
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            //NPC.damage = (int)(NPC.damage * 0.5f);
            NPC.lifeMax = (int)(NPC.lifeMax * balance);
        }

        public override bool CanHitPlayer(Player target, ref int CooldownSlot)
        {
            CooldownSlot = ImmunityCooldownID.Bosses;
            return NPC.localAI[3] == 1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(NPC.localAI[0]);
            writer.Write(NPC.localAI[1]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.localAI[0] = reader.ReadSingle();
            NPC.localAI[1] = reader.ReadSingle();
        }
        public bool bSmackdown;
        public int iMeteorCount;
        public int iDisFromBoss = 160;
        public int Timer;
        public float handCharge = 0f;
        public float handSling = 0f;
        public float handFollowThrough = 0f;
        public bool bStopfalling;
        private Vector2 handTarget = Vector2.Zero;

        public override void AI()
        {
            NPC body = MiscHelpers.NPCExists(CosJelIndex, ModContent.NPCType<CosmicJellyfish>());
            if (body == null)
            {
                NPC.life = 0;
                NPC.checkDead();
                NPC.active = false;
                return;
            }
            NPC.lifeMax = body.lifeMax;
            NPC.damage = body.damage;
            NPC.defDamage = body.defDamage;
            NPC.defense = body.defense;
            NPC.defDefense = body.defDefense;
            NPC.target = body.target;

            NPC.life = NPC.lifeMax;

            Player player = Main.player[NPC.target];
            Vector2 targetPos;

            NPC.direction = NPC.spriteDirection = -(int)NPC.ai[3];
            NPC.localAI[3] = 0;
            if (NPC.scale < 1f)
            {
                NPC.scale += 0.05f;
            }
            Vector2 extendOut = new Vector2((float)Math.Cos(body.rotation), (float)Math.Sin(body.rotation));
            Vector2 toPlayer = (player.Center - body.Center).SafeNormalize(Vector2.Zero);
            Vector2 chargedPosition = body.Center - extendOut * (160 * (int)NPC.ai[3]) + new Vector2(0f, body.velocity.Y) - toPlayer * 150f;
            Vector2 normalCenter = body.Center - extendOut * (160 * (int)NPC.ai[3]) + new Vector2(0f, body.velocity.Y);
            float targetRotation = body.rotation;
            switch (AIState)
            {
                case ActionState.Waiting:
                    iMeteorCount = 0;
                    bSmackdown = false;
                    bStopfalling = false;
                    Timer = 0;
                    handSling = 0f;
                    handCharge = 0f;
                    handFollowThrough = 0f;
                    NPC.Center = Vector2.Lerp(NPC.Center, normalCenter, 0.3f);
                    break;

                case ActionState.Anticipate: 
                    if (handCharge < 1f)
                    {
                        handCharge += 0.04f;
                        targetRotation += handCharge;
                    }
                    else
                    {
                        // advance aistate by one (to throwpunch)
                        AIState++;
                        Vector2 toTarget = (player.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                        handTarget = player.Center + toTarget * 120f;
                    }
                    NPC.Center = Vector2.Lerp(normalCenter, chargedPosition, (float)Math.Sin(handCharge * Math.PI));
                    break;

                case ActionState.ThrowPunch:
                    handCharge = 0f;
                    if (handSling < 1f)
                    {
                        handSling += 0.03f;
                        targetRotation -= handSling;
                    }
                    else
                    {
                        if (handFollowThrough < 1f)
                        {
                            handFollowThrough += 0.1f;
                        }
                        else
                        {
                            NPC.life = 0;
                            NPC.checkDead();
                            NPC.active = false;
                            return;
                        }
                    }
                    NPC.Center = Vector2.Lerp(normalCenter, handTarget, (float)Math.Sin(handSling * Math.PI));
                    break;

                /*                    for (int i = 0; i < Main.maxNPCs; i++) //find the other hand
                                    {
                                        if (Main.npc[i].active && Main.npc[i].type == NPC.type && i != NPC.whoAmI && Main.npc[i].ai[2] == NPC.ai[2])
                                        {
                                            Main.npc[i].velocity = Vector2.Zero;
                                            Main.npc[i].ai[0] = NPC.ai[0];
                                            Main.npc[i].ai[1] = NPC.ai[1];
                                            Main.npc[i].localAI[0] = NPC.localAI[0];
                                            Main.npc[i].localAI[1] = NPC.localAI[1];
                                            Main.npc[i].netUpdate = true;
                                            break;
                                        }
                                    }*/
                default:
                    AIState = ActionState.Waiting;
                    goto case 0;

            }
            NPC.rotation = MathHelper.Lerp(NPC.rotation, targetRotation, 0.05f);
            if (Main.rand.NextBool(3))
            {
                Vector2 velo = NPC.rotation.ToRotationVector2().RotatedBy(MathHelper.PiOver2);
                Vector2 veloDelta = NPC.position - NPC.oldPosition;
                Vector2 sideOffset = new(-30f * NPC.direction, -20f);
                ITDParticle spaceMist = ParticleSystem.NewParticle<SpaceMist>(NPC.Center + new Vector2(0f, NPC.height / 2) + sideOffset, ((velo * 3f) + veloDelta).RotatedByRandom(0.6f), 0f);
                spaceMist.tag = NPC;
            }
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Y = NPC.localAI[3] == 1 ? 0 : frameHeight;
        }
        public override bool CheckActive()
        {
            return false;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;
            Texture2D outline = ModContent.Request<Texture2D>(Texture + "_Outline").Value;
            Vector2 origin = new(tex.Width * 0.5f, tex.Height / Main.npcFrameCount[Type] * 0.5f);
            void DrawAtNPC(Texture2D texture)
            {
                spriteBatch.Draw(texture, NPC.Center - Main.screenPosition, NPC.frame, Color.White, NPC.rotation, origin, NPC.scale, NPC.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            }
            if (AIState == ActionState.ThrowPunch || bSmackdown || AIState == ActionState.Anticipate)
            {
                for (int k = 0; k < NPC.oldPos.Length; k++)
                {
                    Vector2 center = NPC.Size * 0.5f;
                    Vector2 drawPos = NPC.oldPos[k] - Main.screenPosition + center;
                    Color color = NPC.GetAlpha(drawColor) * ((NPC.oldPos.Length - k) / (float)NPC.oldPos.Length);
                    spriteBatch.Draw(outline, drawPos, NPC.frame, color, NPC.oldRot[k], origin, NPC.scale, NPC.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
                }
            }
            DrawAtNPC(outline);
            foreach (ITDParticle mist in ParticleSystem.Instance.particles.Where(p => p.tag == NPC))
            {
                if (mist is SpaceMist sMist)
                {
                    sMist.DrawOutline(spriteBatch);
                }
            }
            foreach (ITDParticle mist in ParticleSystem.Instance.particles.Where(p => p.tag == NPC))
            {
                mist.DrawCommon(spriteBatch, mist.Texture, mist.CanvasOffset);
            }
            DrawAtNPC(tex);
            return false;
        }
    }
}