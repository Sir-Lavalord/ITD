using ITD.Content.Items.Other;
using ITD.Content.Items.PetSummons;
using ITD.Content.Items.Placeable;
using ITD.Content.Projectiles.Friendly.Misc;
using ITD.Content.Projectiles.Hostile;
using ITD.Content.NPCs.Bosses;
using ITD.Players;
using ITD.Utilities;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using System.IO;
using ITD.Content.Dusts;
using ITD.Content.Items.Armor.Vanity.Masks;
using Terraria.Graphics.Effects;
using ITD.Particles.CosJel;
using ITD.Particles;
using Terraria.UI.Chat;
using Terraria.Chat;
using Terraria.Localization;
using ITD.Content.Items.Accessories.Movement.Boots;
using ITD.Content.NPCs.Bosses;

namespace ITD.Content.NPCs.Bosses

{
    [AutoloadBossHead]
    public class CosmicJellyfish : ModNPC
    {
        public float rotation = 0f;
        public float AIRand = 0f;
        public bool bOkuu;
        int goodtransition;//Add to current frame for clean tentacles
        public int attackCount;
        public override void SetStaticDefaults()
        {
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            Main.npcFrameCount[NPC.type] = 10;
            NPCID.Sets.TrailCacheLength[Type] = 6;
            NPCID.Sets.TrailingMode[Type] = 3;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(bOkuu);
            writer.Write(goodtransition);
            writer.Write(NPC.localAI[0]);
            writer.Write(NPC.localAI[1]);
            writer.Write(NPC.localAI[2]);
            writer.Write(AIRand);

        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            bOkuu = reader.ReadBoolean();

            goodtransition = reader.ReadInt32();
            NPC.localAI[0] = reader.ReadSingle();
            NPC.localAI[1] = reader.ReadSingle();
            NPC.localAI[2] = reader.ReadSingle();
            AIRand = reader.ReadSingle();
        }
        private enum MovementState
        {
            FollowingRegular,
            Wandering,
            Ram,
            Suffocate,
            Explode
        }
        private MovementState AI_State = MovementState.FollowingRegular;

        public override void SetDefaults()
        {
            NPC.width = 180;
            NPC.height = 252;
            NPC.damage = 8;
            NPC.defense = 5;
            NPC.lifeMax = 3000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 5);
            NPC.SpawnWithHigherTime(30);
            NPC.boss = true;
            NPC.npcSlots = 10f;
            NPC.aiStyle = -1;
            if (!Main.dedServ)
            {
                Music = ITD.Instance.GetMusic("InterstellarInvertebrate") ?? MusicID.Boss1;
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<CosmicJellyfishBag>()));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CosmicJellyfishTrophy>(), 10));
            npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<CosmicJellyfishRelic>()));
            npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<CosmicJam>(), 4));

            LeadingConditionRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());
            notExpertRule.OnSuccess(ItemDropRule.NotScalingWithLuck(ModContent.ItemType<GravityBoots>(), 10));
            notExpertRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<CosmicJellyfishMask>(), 7));
            notExpertRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<StarlitOre>(), 1, 15, 30));
            notExpertRule.OnSuccess(ItemDropRule.Common(ItemID.Star, 1, 10, 20));


        }
        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 0.8f * balance * bossAdjustment);

            NPC.damage = (int)(NPC.damage * 0.7f);
        }
        public override void OnKill()
        {
            DownedBossSystem.downedCosJel = true;
        }
        bool expertMode = Main.expertMode;
        bool masterMode = Main.masterMode;
        public bool bSecondStage;
        public override void AI()
        {
            ITDGlobalNPC.cosjelBoss = NPC.whoAmI;
            Player player = Main.player[NPC.target];
            Movement(player);
            Vector2 toPlayer = player.Center - NPC.Center;
            Vector2 toPlayerNormalized = Vector2.Normalize(toPlayer);
            NPC.dontTakeDamage = false;
            NPC.alpha = 0;

            switch ((int)NPC.ai[3])
            {
                case 0:
                    if (NPC.ai[1]++ >= 120)//take time to get to the player
                    {
                        NPC.ai[3]++;
                        NPC.ai[1] = 0;
                    }
                    break;
                case 1: //sludge
                        NPC.ai[1]++;
                        if (NPC.ai[1] == 50)
                        {
                            int projectileAmount = Main.rand.Next(5, 8);

                            if (bSecondStage)
                            {
                                projectileAmount = Main.rand.Next(7, 10);
                            }
                            else
                            {
                                projectileAmount = Main.rand.Next(3, 6);
                            }
                            float XVeloDifference = 2f;
                            float startXVelo = -((float)(projectileAmount - 1) / 2) * (float)XVeloDifference;
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                for (int i = 0; i < projectileAmount; i++)
                                {
                                    Vector2 projectileVelo = new Vector2(startXVelo + XVeloDifference * i, -8f);
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.velocity + projectileVelo, ModContent.ProjectileType<CosmicSludgeBomb>(), 30, 0, -1, NPC.whoAmI);
                                }
                            }
                        }
                        else if (NPC.ai[1] == 150 || NPC.ai[1] == 100 && bSecondStage)
                        {
                            NPC.ai[3]++;
                            NPC.ai[1] = 0;
                        }
                    
                    break;
                case 2: //zingers
                    if (NPC.ai[2]++ >= 240 || expertMode && NPC.ai[2]++ >= 200 || masterMode && NPC.ai[2]++ >= 180)
                    {
                        NPC.ai[2] = 0;
                        SoundEngine.PlaySound(SoundID.Item20, NPC.Center);
                        //P2 stat garbage here
                        int projectileAmount = Main.rand.Next(20, 24);
                        float radius = 6.5f;
                        float sector = (float)(MathHelper.TwoPi);
                        float sectorOfSector = sector / projectileAmount;
                        float towardsAngle = toPlayer.ToRotation();
                        float startAngle = towardsAngle - sectorOfSector * (projectileAmount - 1) / 2;
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            for (int i = 0; i < projectileAmount; i++)
                            {
                                float angle = startAngle + sectorOfSector * i;
                                Vector2 projectileVelo = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, projectileVelo * 0.01f, ModContent.ProjectileType<CosmicVoidShard>(), 20, 5, -1, 0, 2);

                            }
                        }
                    }
                    if (NPC.ai[1]++ >= 400 + Main.rand.Next(-100, 150))
                    {
                        NPC.ai[2] = 0;
                        NPC.ai[1] = 0;
                        NPC.ai[3]++;
                    }
                    break;
                case 3:
                    if (NPC.ai[1]++ == 60)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)//Fix later, this will do for now
                        {
                            if (!bSecondStage)
                            {
                                NPC.NewNPCDirect(NPC.GetSource_FromThis(), new Vector2(NPC.Center.X, NPC.Center.Y - 100), ModContent.NPCType<CosmicJellyfishMini>());
                            }
                            else
                            {
                                NPC.NewNPCDirect(NPC.GetSource_FromThis(), new Vector2(NPC.Center.X, NPC.Center.Y - 100), ModContent.NPCType<CosmicJellyfishMini>());
                                NPC.NewNPCDirect(NPC.GetSource_FromThis(), new Vector2(NPC.Center.X - 200, NPC.Center.Y + 100), ModContent.NPCType<CosmicJellyfishMini>());
                                NPC.NewNPCDirect(NPC.GetSource_FromThis(), new Vector2(NPC.Center.X + 200, NPC.Center.Y + 100), ModContent.NPCType<CosmicJellyfishMini>());
                            }
                        }
                    }
                    else if (NPC.ai[1] >= 300)
                    {
                        NPC.ai[2] = 0;
                        NPC.ai[1] = 0;
                        NPC.ai[3]++;
                        NetSync();
                    }
                    break;
                case 4:
                    if (NPC.ai[1]++ == 0)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            NPCHelpers.NewNPCEasy(NPC.GetSource_FromAI(), NPC.Center, ModContent.NPCType<CosmicJellyfishHand>(), NPC.whoAmI, 0, 0, NPC.whoAmI, 1);
                            NPCHelpers.NewNPCEasy(NPC.GetSource_FromAI(), NPC.Center, ModContent.NPCType<CosmicJellyfishHand>(), NPC.whoAmI, 0, 0, NPC.whoAmI, -1);
                        }
                    }
                    else if(NPC.ai[1] >= 100)
                    {
                        NPC.ai[2] = 0;
                        NPC.ai[1] = 0;
                        NPC.ai[3] = 0;
                        NetSync();
                        for (int i = 0; i < Main.maxNPCs; i++) //find hands, update
                        {
                            if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<CosmicJellyfishHand>() && Main.npc[i].ai[2] == NPC.whoAmI)
                            {
                                Main.npc[i].ai[0] = 1;
                                Main.npc[i].ai[1] = 0;
                                Main.npc[i].localAI[0] = 0;
                                Main.npc[i].localAI[1] = 0;
                                Main.npc[i].netUpdate = true;
                            }
                        }
                    }

                    break;
            }
        }

        private void Movement(Player player)//___________________________________________________________________________________________________________________________________________________
        {
            float distanceAbove = 250f;//True melee
            Vector2 toPlayer = player.Center - NPC.Center;
            Vector2 toPlayerNormalized = Vector2.Normalize(toPlayer);
            Vector2 abovePlayer = toPlayer + new Vector2(0f, -distanceAbove);
            Vector2 aboveNormalized = Vector2.Normalize(abovePlayer);
            Vector2 dashvel;
            float speed = abovePlayer.Length() / 1.3f;//True melee
            switch (AI_State)
            {
                case MovementState.FollowingRegular:
                    if (speed > 1.1f)
                    {
                        NPC.velocity = aboveNormalized * (speed + 1f) / 20;
                    }
                    else
                    {
                        NPC.velocity = Vector2.Zero;
                    }
                    break;
                case MovementState.Wandering:
                    break;
                case MovementState.Ram:
                    NPC.localAI[2]++;
                    if (NPC.localAI[2] < 10)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            NPC.velocity *= 0.9f;
                            NetSync();
                        }
                    }
                    //very hard coded
                    if (NPC.localAI[2] == 10)//set where to dash
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            dashvel = Vector2.Normalize(new Vector2(player.Center.X, player.Center.Y) - new Vector2(NPC.Center.X, NPC.Center.Y));
                            NPC.velocity = dashvel;
                            NetSync();
                        }
                    }

                    if (NPC.localAI[2] > 10 && NPC.localAI[2] < 30)//xcel
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            NPC.velocity *= 1.18f;
                            NetSync();
                        }
                    }
                    if (NPC.localAI[2] > 50) //Decelerate 
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            NPC.velocity *= 0.96f;
                            NetSync();
                        }
                    }
                    if (NPC.localAI[2] >= 80)
                    {
                        NPC.localAI[0]++;
                        NPC.localAI[1] = 0;
                        NPC.localAI[2] = 0;
                        NetSync();
                        AI_State = MovementState.FollowingRegular;
                    }
                    break;
                case MovementState.Suffocate:
                    var suffer = player.GetModPlayer<ITDPlayer>();
                    NPC.velocity *= 0.6f;
                    player.velocity *= 0;
                    player.Center = NPC.Center;
                    player.AddBuff(BuffID.Suffocation, 5);
                    player.AddBuff(BuffID.Obstructed, 5);
                    if (!suffer.CosJellSuffocated)
                    {
                        NPC.localAI[1] = 0;
                        NPC.localAI[2] = 0;
                        NPC.localAI[0]++;
                        AI_State = MovementState.FollowingRegular;

                    }
                    break;

                case MovementState.Explode:
                    NPC.velocity *= 0.9f;
                    break;

            }
            float maxRotation = MathHelper.Pi / 6;
            float rotationFactor = MathHelper.Clamp(NPC.velocity.X / 8f, -1f, 1f);
            if (AI_State == MovementState.Ram)
            {
                Vector2 velo = Vector2.Normalize(new Vector2(player.Center.X, player.Center.Y) - new Vector2(NPC.Center.X, NPC.Center.Y));
                NPC.rotation = velo.ToRotation();
                NPC.rotation = NPC.velocity.X / 50;

            }
            else
            {
                rotation = rotationFactor * maxRotation;
                NPC.rotation = rotation;
            }
        }
        private void NetSync()
        {
            //Will find and update hand here
            NPC.netUpdate = true;
        }
        public override void FindFrame(int frameHeight)
        {
            int startFrame = 0;
            int finalFrame = 4;
            if (bSecondStage)
            {
                goodtransition = 5;
            }
            if (!bOkuu)
            {
                int frameSpeed = 5;
                NPC.frameCounter += 1f;
                if (NPC.frameCounter > frameSpeed)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y += frameHeight;

                    if (NPC.frame.Y > (finalFrame + goodtransition) * frameHeight)
                    {
                        NPC.frame.Y = (startFrame + goodtransition) * frameHeight;
                    }
                }
            }
            else
            {
                NPC.frame.Y = 10 * frameHeight;
            }
        }
        public override Color? GetAlpha(Color drawColor)
        {
            return Color.White;
        }
        public override bool PreAI()
        {
            if (!bOkuu)
            {
                if (!bSecondStage)
                {
                    Dust.NewDust(NPC.Center + new Vector2(Main.rand.Next(NPC.width) - NPC.width / 2, 0), 1, 1, DustID.ShimmerTorch, 0f, 0f, 0, default, 1f);
                }
                else
                {
                    Dust.NewDust(NPC.Center + new Vector2(Main.rand.Next(NPC.width) - NPC.width / 2, 0), 1, 1, DustID.ShimmerTorch, 0f, 0f, 0, Color.DarkViolet, 1f);
                }
            }
            else
            {
                BlackholeDusting();
            }
            return true;
        }
        private void BlackholeDusting()
        {
            int dustRings = 3;
            for (int h = 0; h < dustRings; h++)
            {
                float distanceDivisor = h + 1f;
                float dustDistance = 750 / distanceDivisor;
                int numDust = (int)(0.1f * MathHelper.TwoPi * dustDistance);
                float angleIncrement = MathHelper.TwoPi / numDust;
                Vector2 dustOffset = new Vector2(dustDistance, 0f);
                dustOffset = dustOffset.RotatedByRandom(MathHelper.TwoPi);

                int var = (int)(dustDistance);
                float dustVelocity = 24f / distanceDivisor;
                for (int i = 0; i < numDust; i++)
                {
                    if (Main.rand.NextBool(var))
                    {
                        dustOffset = dustOffset.RotatedBy(angleIncrement);
                        int dust = Dust.NewDust(NPC.Center, 1, 1, ModContent.DustType<CosJelDust>());
                        Main.dust[dust].position = NPC.Center + dustOffset;
                        Main.dust[dust].fadeIn = 1f;
                        Main.dust[dust].velocity = Vector2.Normalize(NPC.Center - Main.dust[dust].position) * dustVelocity;
                        Main.dust[dust].scale = 3f - h;
                    }
                }
            }
        }
    }
}