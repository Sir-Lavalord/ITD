using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ITD.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace ITD.Content.NPCs.Bosses
{
    public class CosmicJellyfishHand : ModNPC
    {
        // Fargo echamp clone tempo
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
            CooldownSlot = 1;
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
        public bool isLeftHand;
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
            NPC body = MiscHelpers.NPCExists(NPC.ai[2], ModContent.NPCType<CosmicJellyfish>());
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

            NPC.direction = NPC.spriteDirection = (int)NPC.ai[3];
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
            switch ((int)NPC.ai[0])
            {
                case 0:
                    iMeteorCount = 0;
                    bSmackdown = false;
                    bStopfalling = false;
                    Timer = 0;
                    handSling = 0f;
                    handCharge = 0f;
                    handFollowThrough = 0f;
                    NPC.Center = Vector2.Lerp(NPC.Center, normalCenter, 0.3f);
                    break;

                case 1: 
                    if (handCharge < 1f)
                    {
                        handCharge += 0.04f;
                        targetRotation += handCharge;
                    }
                    else
                    {
                        NPC.ai[0]++;
                        Vector2 toTarget = (player.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                        handTarget = player.Center + toTarget * 120f;
                    }
                    NPC.Center = Vector2.Lerp(normalCenter, chargedPosition, (float)Math.Sin(handCharge * Math.PI));
                    break;

                case 2:
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
                    NPC.ai[0] = 0;
                    goto case 0;

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
    }
}