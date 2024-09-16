﻿using ITD.Content.Items.Other;
using ITD.Content.Items.PetSummons;
using ITD.Content.Items.Placeable;
using ITD.Content.Items.Weapons.Mage;
using ITD.Content.Items.Weapons.Melee;
using ITD.Content.Items.Weapons.Ranger;
using ITD.Content.Items.Weapons.Summoner;
using ITD.Content.Projectiles.Friendly.Misc;
using ITD.Content.Projectiles.Hostile;
using ITD.Players;
using ITD.Utilities;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using System.IO;
using SteelSeries.GameSense;

namespace ITD.Content.NPCs.Bosses

{
    [AutoloadBossHead]
    public class CosmicJellyfish : ModNPC
    {
        private readonly Asset<Texture2D> spriteBack = ModContent.Request<Texture2D>("ITD/Content/NPCs/Bosses/CosmicJellyfish_Back");
        private int hand = -1;
        private int hand2 = -1;

        public static int[] oneFromOptionsDrops =
        {
            ModContent.ItemType<WormholeRipper>(),
            ModContent.ItemType<StarlightStaff>(),
            ModContent.ItemType<Quasar>(),
            ModContent.ItemType<Fishbacker>(),
        };
        //private static List<CosmicJellyfish_Hand> hands = new List<CosmicJellyfish_Hand>();
        public float rotation = 0f;
        public float handCharge = 0f;
        public float handSling = 0f;
        public float handFollowThrough = 0f;
        //Hands must be handled seperately
        public float handCharge2 = 0f;
        public float handSling2 = 0f;
        public float handFollowThrough2 = 0f;
        private Vector2 handTarget2 = Vector2.Zero;

        private Vector2 handTarget = Vector2.Zero;
        private Vector2 handStatic = Vector2.Zero;
        private bool targetPicked = false;
        private Vector2 eyePos;
        //Hand rigamagig will be controlled seperately from the boss
        private bool bSecondStage;
        private bool bOkuu;

        public override void SetStaticDefaults()
        {
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            Main.npcFrameCount[NPC.type] = 10;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(bSecondStage);
            writer.Write(bOkuu);
            writer.Write(NPC.localAI[0]);
            writer.Write(NPC.localAI[1]);
            writer.Write(NPC.localAI[2]);

        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            bSecondStage = reader.ReadBoolean();
            bOkuu = reader.ReadBoolean();
            NPC.localAI[0] = reader.ReadSingle();
            NPC.localAI[1] = reader.ReadSingle();
            NPC.localAI[2] = reader.ReadSingle();
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
        private enum HandState
        {
            Waiting,
            Charging,
            Slinging,
            DownToSize,
            ForcedKill,
            VoidShard,
            StarShard,
        }

        private HandState handState = HandState.Waiting;
        private HandState handState2 = HandState.Waiting;

        public override void SetDefaults()
        {
            NPC.width = 120;
            NPC.height = 200;
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
            //notExpertRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<CosmicJellyfishMask>(), 7));
            notExpertRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<StarlitOre>(), 1, 15, 30));
            notExpertRule.OnSuccess(ItemDropRule.OneFromOptionsNotScalingWithLuck(1, oneFromOptionsDrops));
        }
        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            if (!Main.masterMode)
            {
                NPC.lifeMax = (int)(NPC.lifeMax * 0.8f * bossAdjustment);
            }
            else
            {
                NPC.lifeMax = (int)(NPC.lifeMax * 0.8f * bossAdjustment);

            }
            NPC.damage = (int)(NPC.damage * 0.5f);
        }
        public override void OnKill()
        {
            DownedBossSystem.downedCosJel = true;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            cooldownSlot = ImmunityCooldownID.Bosses;
            if (AI_State == MovementState.Suffocate)
            {
                return false;
            }
            else return true;
        }
        int goodtransition;//Add to current frame for clean tentacles

        public override void FindFrame(int frameHeight)
        {
            int startFrame = 0;
            int finalFrame = 4;
            if (bSecondStage)
            {
                goodtransition = 5;
            }
            int frameSpeed = 5;
            NPC.frameCounter += 1f;
            if (NPC.frameCounter > frameSpeed)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;

                if (NPC.frame.Y > (finalFrame+goodtransition) * frameHeight)
                {
                    NPC.frame.Y = (startFrame + goodtransition) * frameHeight;
                }
            }
        }
        public override Color? GetAlpha(Color drawColor)
        {
            return Color.White;
        }
        public override bool PreAI()
        {
            Dust.NewDust(NPC.Center + new Vector2(Main.rand.Next(NPC.width) - NPC.width / 2, 0), 1, 1, DustID.ShimmerTorch, 0f, 0f, 0, default, 1f);
            return true;
        }
        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest();
            }

            Player player = Main.player[NPC.target];  
            if (player.dead || !player.active || Main.IsItDay())
            {
                //flee
                //
                handState = HandState.ForcedKill;
                handState2 = HandState.ForcedKill;

                NPC.velocity.Y -= 0.04f;
                NPC.EncourageDespawn(10);
                return;
            }
            if (!bOkuu)
            {
                CheckbSecondStage();
            }
            Attacks(player);
            Movement(player);
            if (hand != -1 || hand2 != -1)
                HandleHand(player);
        }
        private void CheckbSecondStage()
        {
            if (bSecondStage)
            {
                if (NPC.ai[3] >= 8)
                {
                    NPC.ai[3] = 1;
                }
            }
            else
            {
                if (NPC.ai[3] >= 6)
                {
                    NPC.ai[3] = 1;
                }
            }

            if (NPC.life * 100 / NPC.lifeMax < 50)
            {
                if (!bSecondStage)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        //Explosion goes here
                        int projID = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<BlankExplosion>(), NPC.damage, 0, Main.myPlayer);
                        Main.projectile[projID].scale = 3f;//300x300
                        Main.projectile[projID].hostile = true;
                        Main.projectile[projID].friendly = false;
                        Main.projectile[projID].knockBack = 3f;
                    }
                    if (Main.netMode != NetmodeID.Server)//Drop some gore when changing phase
                    {

                        bSecondStage = true;
                        NPC.localAI[0] = 0;

                        NPC.localAI[1] = 0;

                        NPC.localAI[2] = 0;
                        NPC.ai[3]++;
                        if (hand != -1)
                        {
                            handState = HandState.ForcedKill;
                        }
                        if (hand2 != -1)
                        {
                            handState2 = HandState.ForcedKill;
                        }
                        //Gore MUST goes here (else it will fuck up multiplayer)
                    }

/*                    Main.NewText("Fish Your Eyes", Color.Goldenrod);
*/                    NPC.netUpdate = true;

                }
            }
            return;

        }
        //TODO: FIX CODE FOR MULTIPLAYER
        private void Attacks(Player player)//___________________________________________________________________________________________________________________________________________________
        {
            //TODO: Change this??
            Vector2 toPlayer = player.Center - NPC.Center;
            Vector2 toPlayerNormalized = Vector2.Normalize(toPlayer);


            switch (NPC.ai[3])
            {
                case 0:
                    if (NPC.localAI[1]++ >= 0)
                    {
                        NPC.ai[3]++;
                        NPC.localAI[1] = 0;
                    }
                    break;
                case 1:
                    NPC.localAI[1]++;
                    if (NPC.localAI[1] == 30)
                    {
                        int projectileAmount = Main.rand.Next(3, 6);

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
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.velocity + projectileVelo, ModContent.ProjectileType<CosmicSludgeBomb>(), 0, 0, -1, NPC.whoAmI);
                            }
                        }
                    }
                    else if (NPC.localAI[1] == 180 || NPC.localAI[1] == 120 && bSecondStage)
                    {
                        NPC.ai[3]++;
                        NPC.localAI[1] = 0;
                    }
                    break;
                case 2:
                    if (NPC.localAI[1]++ >= 10)
                    {
                        if (handState == HandState.Waiting && hand != -1)
                            handState = HandState.VoidShard;
                        if (bSecondStage)
                        {
                            if (handState2 == HandState.Waiting && hand2 != -1)
                                handState2 = HandState.StarShard;
                        }
                    }
                    else
                    {
                        if (hand == -1)
                        {
                            hand = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<CosmicJellyfish_Hand>(), 1, 0.1f);
                        }
                        if (bSecondStage)
                        {
                            if (hand2 == -1)
                            {
                                hand2 = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<CosmicJellyfish_Hand>(), 1, 0.1f);
                            }
                        }
                    }
                    if (NPC.localAI[1] >= 200 + Main.rand.Next(50, 100))//Convoluted
                    {
                        NPC.localAI[1] = 0;
                        NPC.ai[3]++;
                        Timer = Timer2 = 0;
                        handState = HandState.ForcedKill;
                        handState2 = HandState.ForcedKill;
                    }
                    break;
                case 3:
                    if (NPC.localAI[1]++ == 60)
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
                    else if (NPC.localAI[1] >= 300)
                    {
                        NPC.ai[3]++;
                        NPC.localAI[1] = 0;
                        NPC.netUpdate = true;
                    }
                    break;
                case 4:
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        if (!bSecondStage)
                        {
                            if (NPC.localAI[1]++ >= 200)
                            {
                                NPC.ai[3]++;
                                NPC.localAI[1] = 0;
                                if (handState2 == HandState.Waiting && hand2 != -1)
                                    handState2 = HandState.Charging;
                                if (handState == HandState.Waiting && hand != -1)
                                    handState = HandState.Charging;
                            }
                            if (hand == -1 && hand2 == -1)
                            {
                                if (Main.rand.NextBool(2))
                                {
                                    if (hand == -1)
                                    {
                                        hand = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<CosmicJellyfish_Hand>(), 1, 0.1f);
                                    }
                                }
                                else
                                {
                                    if (hand2 == -1)
                                    {
                                        hand2 = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<CosmicJellyfish_Hand>(), 1, 0.1f);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (NPC.localAI[1]++ == 100)
                            {
                                if (handState == HandState.Waiting && hand != -1)
                                    handState = HandState.Charging;
                            }
                            if (NPC.localAI[2] > 6)
                            {
                                NPC.localAI[1] = 0;
                                NPC.localAI[2] = 0;
                                NPC.ai[3]++;
                                if (handState == HandState.Waiting && hand != -1)
                                    handState = HandState.ForcedKill;
                                if (handState2 == HandState.Waiting && hand2 != -1)
                                    handState2 = HandState.ForcedKill;
                            }
                            if (hand == -1)
                            {
                                hand = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<CosmicJellyfish_Hand>(), 1, 0.1f);
                            }
                            if (hand2 == -1)
                            {
                                hand2 = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<CosmicJellyfish_Hand>(), 1, 0.1f);
                            }
                        }
                    }
                    break;
                case 5:
                    NPC.localAI[1]++;
                    if (NPC.localAI[1] == 60)
                    {
                        if (AI_State != MovementState.Suffocate)
                            AI_State = MovementState.Ram;
                    }
                    if (NPC.localAI[0] >= 2 && !bSecondStage || NPC.localAI[0] >= 3 && bSecondStage)
                    {
                        //can't believe i have to do this, since the checking doesn't even fucking work
                        NPC.ai[3]++;
                        NPC.localAI[1] = 0;
                        NPC.localAI[0] = 0;
                        NPC.localAI[2] = 0;

                    }
                    break;
                case 6:
                    if (NPC.localAI[1]++ == 60)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(0, -3.5f).RotatedBy((MathHelper.TwoPi / 3) * i),
                                    ModContent.ProjectileType<CosmicLightningOrb>(), NPC.damage / 2, 2f, -1, NPC.whoAmI, i);
                            }
                        }
                    }
                    if (NPC.localAI[1] == 600)
                    {
                        NPC.ai[3]++;
                        NPC.localAI[1] = 0;
                    }
                    break;
                case 7:
                    AI_State = MovementState.Explode;
                    NPC.localAI[1]++;
                    if (NPC.localAI[1] == 10 && NPC.localAI[0] < 30)
                    {

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int saferange = Main.rand.Next(350, 450);
                            int max = Main.rand.Next(1,3);
                            float offset = NPC.localAI[0] > 0 && player.velocity != Vector2.Zero //aim to intercept
                                ? Main.rand.NextFloat((float)Math.PI * 2) : player.velocity.ToRotation();
                            for (int i = 0; i < max ; i++)
                            {
                                float rotation = offset + (float)Math.PI * 2 / Main.rand.Next(10);
                                int proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), player.Center + saferange * Vector2.UnitX.RotatedBy(rotation), Vector2.Zero,
                                    ModContent.ProjectileType<TouhouBullet>(), NPC.damage, 0f,-1,NPC.whoAmI,2);
                                ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Excalibur,
                                    new ParticleOrchestraSettings { PositionInWorld = Main.projectile[proj].Center }, Main.projectile[proj].owner);

                            }
                            NPC.localAI[0]++;
                            NPC.localAI[1] = 0;
                        }
                    }
                    if (NPC.localAI[0]>= 30)
                    {
                        if (NPC.localAI[2]++ == 60)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    Vector2 vel = NPC.DirectionTo(player.Center) * 1f; ;
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, vel,
                                     ModContent.ProjectileType<CosmicRayWarn>(), NPC.damage, 0f, -1, 240, NPC.whoAmI);
                                }
                            }
                        }
                        if (NPC.localAI[2] == 600)
                        {
                            AI_State = MovementState.FollowingRegular;
                            NPC.ai[3]++;
                            NPC.localAI[1] = 0;
                            NPC.localAI[2] = 0;
                            NPC.localAI[0] = 0;
                            NPC.netUpdate = true;
                        }
                        if (NPC.localAI[1] >= 100)
                        {
                            NPC.localAI[1] = 0;
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
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, projectileVelo, ModContent.ProjectileType<CosmicVoidShard>(), 20, 5, -1);
                                }
                            }
                        }
                    }
                        break;
                case 8:
                    if (NPC.localAI[1]++ == 60)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Vector2 vel = NPC.DirectionTo(player.Center) * 1f; ;
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, vel,
                             ModContent.ProjectileType<CosmicRayWarn>(), NPC.damage, 0f, -1, 240, NPC.whoAmI);
                        }
                    }
                    else if (NPC.localAI[1] >= 180)
                    {
                        NPC.ai[3]++;
                        NPC.localAI[1] = 0;
                        NPC.localAI[2] = 0;
                        NPC.localAI[0] = 0;
                        NPC.netUpdate = true;

                    }
                    break;
                case -1:
                    player.GetITDPlayer().Screenshake = 20;
                    if (++NPC.localAI[2] <= 900)
                    {
                        if (Main.expertMode || Main.masterMode)
                        {
                            if (++NPC.localAI[0] > 16)
                            {
                                NPC.localAI[0] = 0;
                                NPC.localAI[1] += (float)Math.PI / 2 / 360 * 75;
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    for (int i = 0; i < 4; i++)
                                    {
                                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + 600 *
                                        Vector2.UnitX.RotatedBy(NPC.localAI[1] + Math.PI / 2 * i), Vector2.Zero, ModContent.ProjectileType<TouhouBullet>(),
                                        NPC.damage, 0f, -1, NPC.whoAmI);
                                    }
                                }

                            }
                            float Power = 0.25f;
                            float DisCap = 3000;
                            for (int i = 0; i < Main.maxPlayers; i++)
                            {
                                float distance = Vector2.Distance(Main.player[i].Center, NPC.Center);
                                if (Vector2.Distance(Main.player[i].Center, NPC.Center) < DisCap)
                                {
                                    Vector2 velToApply = -Main.player[i].Center.DirectionFrom(NPC.Center).SafeNormalize(Vector2.UnitX) * 0.5f;
                                    Main.player[i].velocity.X = velToApply.X;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            //EXPLOSION GOES HERE
                        }
                        NPC.life = 0;
                        NPC.HitEffect(0, 0);
                        NPC.checkDead();
                    }
                    break;

            }               
        }
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {

            var suffer = target.GetModPlayer<ITDPlayer>();
            if (NPC.ai[3] == 5)//Suffocate, actual retarded attack but i'm not the boss here
            {
                AI_State = MovementState.Suffocate;
                if (!suffer.CosJellSuffocated)
                {
                    suffer.CosJellSuffocated = true;
                    if (Main.masterMode || Main.expertMode)//Annoying asdf
                    {
                        suffer.CosJellEscapeCurrent = 10;
                    }
                    else suffer.CosJellEscapeCurrent = 6;
                }
            }
        }
        #region Hands
        Vector2 LockedIn;
        int Timer;
        int Timer2;//Again, hands must be handled seperately

        private void HandleHand(Player player)
        {  
            if (!player.active || player.dead)
            {
                if (hand != -1)
                {
                    handState = HandState.ForcedKill;
                }
                if (hand2 != -1)
                {
                    handState2 = HandState.ForcedKill;
                }
            }
                if (hand != -1)
            {
                Projectile projectile = Main.projectile[hand];
                if (projectile.active && projectile.type == ModContent.ProjectileType<CosmicJellyfish_Hand>())
                {
                    if (projectile.scale < 1f)
                    {
                        projectile.scale += 0.05f;
                    }
                    projectile.timeLeft = 2;
                    Vector2 extendOut = new Vector2((float)Math.Cos(NPC.rotation), (float)Math.Sin(NPC.rotation));
                    Vector2 toPlayer = (player.Center - projectile.Center).SafeNormalize(Vector2.Zero);
                    Vector2 chargedPosition = NPC.Center - extendOut * 110 + new Vector2(0f, NPC.velocity.Y) - toPlayer * 150f;
                    Vector2 normalCenter = NPC.Center - extendOut * 110 + new Vector2(0f, NPC.velocity.Y);
                    float targetRotation = NPC.rotation;
                    if (!Main.IsItDay())
                    {
                        if (handState != HandState.Charging && handState != HandState.Slinging)
                        {
                            if (projectile.frameCounter++ >= 6)
                            {
                                projectile.frameCounter = 0;
                                projectile.frame++;
                                if (projectile.frame >= 4)
                                {

                                    projectile.frame = 0;
                                }
                            }
                        }
                        else
                        {
                            projectile.frame = 5;
                        }
                    }
                    else
                    {
                        projectile.frame = 6;
                    }
                    switch (handState)
                    {
                        case HandState.Waiting:
                            Timer = 0;
                            handSling = 0f;
                            handCharge = 0f;
                            handFollowThrough = 0f;
                            projectile.Center = Vector2.Lerp(projectile.Center, normalCenter, 0.3f);
                            break;
                        case HandState.Charging:
                            if (handCharge < 1f)
                            {
                                handCharge += 0.04f;
                                targetRotation += handCharge;
                            }
                            else
                            {
                                if (Timer++ == 0)
                                {
                                    LockedIn = player.Center;
                                }
                                else if (Timer >= 5 && NPC.localAI[2] > 6 || Timer >= 0 && NPC.localAI[2] <=6)
                                {
                                    Timer = 0;
                                    handState = HandState.Slinging;
                                    Vector2 toTarget = (LockedIn - projectile.Center).SafeNormalize(Vector2.Zero);
                                    handTarget = LockedIn + toTarget * 120f;
                                    handStatic = projectile.Center;
                                }
                            }
                            projectile.Center = Vector2.Lerp(normalCenter, chargedPosition, (float)Math.Sin(handCharge * Math.PI));
                            break;
                        case HandState.Slinging:
                            targetPicked = false;
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
                                    if (NPC.localAI[2]++ <= 6 && bSecondStage)
                                    {
                                        if (handState2 == HandState.Waiting && hand2 != -1)
                                            handState2 = HandState.Charging;
                                    }
                                    if (NPC.localAI[2] > 6 || !bSecondStage)
                                    {
                                        projectile.Kill();
                                    }
                                    handState = HandState.Waiting;
                                }
                            }
                            projectile.Center = Vector2.Lerp(normalCenter, handTarget, (float)Math.Sin(handSling * Math.PI));
                            break;
                        case HandState.DownToSize:
                            handSling = 0f;
                            handCharge = 0f;
                            handFollowThrough = 0f;
                            projectile.Center = Vector2.Lerp(projectile.Center, normalCenter, 0.3f);
                            if (Timer++ >= 30)
                            {
                                if (NPC.ai[3] != 2)
                                {
                                    if (bSecondStage)
                                    {
                                        if (handState2 == HandState.Waiting && hand2 != -1)
                                            handState2 = HandState.Charging;
                                        handState = HandState.Waiting;
                                    }
                                    else
                                    {
                                        handState = HandState.Waiting;
                                        projectile.Kill();
                                    }
                                }
                                else
                                {
                                    handState = HandState.Waiting;
                                    projectile.Kill();
                                }

                            }
                            break;
                        case HandState.ForcedKill:
                            handState = HandState.Waiting;
                            projectile.Kill();
                            Timer = 0;
                            handSling = 0f;
                            handCharge = 0f;
                            handFollowThrough = 0f;
                            projectile.Center = Vector2.Lerp(projectile.Center, normalCenter, 0.3f);
                            break;
                        case HandState.VoidShard:
                            if (Timer++ >= 80)
                            {
                                Timer = 0;
                                SoundEngine.PlaySound(SoundID.Item20, projectile.Center);
                                //P2 stat garbage here
                                int projectileAmount = Main.rand.Next(5, 11);
                                float radius = 6.5f;
                                float sector = MathHelper.ToRadians(80f);
                                //Full circle
                                if (Main.expertMode || Main.masterMode)
                                {
                                    projectileAmount = Main.rand.Next(16, 20);
                                    sector = (float)(MathHelper.TwoPi);
                                }
                                float sectorOfSector = sector / projectileAmount;
                                float towardsAngle = toPlayer.ToRotation();
                                float startAngle = towardsAngle - sectorOfSector * (projectileAmount - 1) / 2;
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    for (int i = 0; i < projectileAmount; i++)
                                    {
                                        float angle = startAngle + sectorOfSector * i;
                                        Vector2 projectileVelo = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                                        Projectile.NewProjectile(projectile.GetSource_FromAI(), projectile.Center, projectileVelo, ModContent.ProjectileType<CosmicVoidShard>(), 20, 5, -1);
                                    }
                                }
                            }
                            projectile.Center = Vector2.Lerp(projectile.Center, normalCenter, 0.3f);
                            break;
                    }
                    projectile.rotation = MathHelper.Lerp(projectile.rotation, targetRotation, 0.3f);
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        Projectile other = Main.projectile[i];
                        
                        if (i != projectile.whoAmI && other.active && other.type == ProjectileID.CopperShortswordStab && Math.Abs(projectile.position.X - other.position.X) + Math.Abs(projectile.position.Y - other.position.Y) < projectile.width)
                        {
                            if (Timer == 0 && handState == HandState.Slinging && handFollowThrough < 1f &&
                                player.Distance(projectile.Center) < 60f
                                )
                            {
                                ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Excalibur,
                                    new ParticleOrchestraSettings { PositionInWorld = other.Center }, projectile.owner);
                                SoundEngine.PlaySound(new SoundStyle("ITD/Content/Sounds/UltraParry"), projectile.Center);
                                player.GetITDPlayer().Screenshake = 20;
                                    handState = HandState.DownToSize;

                                if (NPC.life > NPC.lifeMax / 10)
                                {
                                    NPC.life -= NPC.lifeMax / 10;
                                }
                                else CheckDead();
                                CombatText.NewText(projectile.Hitbox, Color.Violet, "DOWN TO SIZE", true);
                                projectile.velocity = -projectile.velocity *2;
                                // if the achievements mod is on, unlock the parry achievement
                                ITD.Instance.achievements?.Call("Event", "ParryCosJelHand");
                            }
                        }
                    }
                }
                else
                {
                    hand = -1;
                }
            }
            if (hand2 != -1)
            {
                Projectile projectile2 = Main.projectile[hand2];
                if (projectile2.active && projectile2.type == ModContent.ProjectileType<CosmicJellyfish_Hand>())
                {
                    if (projectile2.scale < 1f)
                    {
                        projectile2.scale += 0.05f;
                    }
                    projectile2.timeLeft = 2;
                    Vector2 extendOut = new Vector2((float)Math.Cos(NPC.rotation), (float)Math.Sin(NPC.rotation));
                    Vector2 toPlayer = (player.Center - projectile2.Center).SafeNormalize(Vector2.Zero);
                    Vector2 chargedPosition = NPC.Center - extendOut * -110 + new Vector2(0f, NPC.velocity.Y) - toPlayer * 150f;
                    Vector2 normalCenter = NPC.Center - extendOut * -110 + new Vector2(0f, NPC.velocity.Y);
                    float targetRotation = NPC.rotation;
                    if (!Main.IsItDay())
                    {
                        if (handState2 != HandState.Charging && handState2 != HandState.Slinging)
                        {
                            if (projectile2.frameCounter++ >= 6)
                            {
                                projectile2.frameCounter = 0;
                                projectile2.frame++;
                                if (projectile2.frame >= 4)
                                {

                                    projectile2.frame = 0;
                                }
                            }
                        }
                        else
                        {
                            projectile2.frame = 5;
                        }
                    }
                    else
                    {
                        projectile2.frame = 6;
                    }
                    switch (handState2)
                    {
                        case HandState.Waiting:
                            Timer2 = 0;
                            handSling2 = 0f;
                            handCharge2 = 0f;
                            handFollowThrough2 = 0f;
                            projectile2.Center = Vector2.Lerp(projectile2.Center, normalCenter, 0.3f);
                            break;
                        case HandState.Charging:
                            if (handCharge2 < 1f)
                            {
                                handCharge2 += 0.04f;
                                targetRotation += handCharge2;
                            }
                            else
                            {
                                if (Timer2++ == 0)
                                {
                                        LockedIn = player.Center;
                                }
                                else if (Timer2 >= 5 && bSecondStage || Timer2 >= 0 && !bSecondStage)
                                {
                                    Timer2 = 0;
                                    handState2 = HandState.Slinging;
                                    Vector2 toTarget = (LockedIn - projectile2.Center).SafeNormalize(Vector2.Zero);
                                    handTarget2 = LockedIn + toTarget * 120f;
                                    handStatic = projectile2.Center;
                                }
                            }
                            projectile2.Center = Vector2.Lerp(normalCenter, chargedPosition, (float)Math.Sin(handCharge2 * Math.PI));
                            break;
                        case HandState.Slinging:
                            targetPicked = false;
                            handCharge2 = 0f;
                            if (handSling2 < 1f)
                            {
                                handSling2 += 0.03f;
                                targetRotation -= handSling2;
                            }
                            else
                            {
                                if (handFollowThrough2 < 1f)
                                {
                                    handFollowThrough2 += 0.1f;
                                }
                                else
                                {
                                    if (NPC.localAI[2]++ <= 6 && bSecondStage)
                                    {
                                        if (handState == HandState.Waiting && hand != -1)
                                            handState = HandState.Charging;
                                    }
                                    if (NPC.localAI[2] > 6 || !bSecondStage)
                                    {
                                        projectile2.Kill();
                                    }
                                    handState2 = HandState.Waiting;
                                }
                                
                            }
                            projectile2.Center = Vector2.Lerp(normalCenter, handTarget2, (float)Math.Sin(handSling2 * Math.PI));
                            break;
                        case HandState.DownToSize:
                            handSling2 = 0f;
                            handCharge2 = 0f;
                            handFollowThrough2 = 0f;
                            projectile2.Center = Vector2.Lerp(projectile2.Center, normalCenter, 0.3f);
                            if (Timer2++ >= 30)
                            {
                                if (NPC.ai[3] != 2)
                                {
                                    if (bSecondStage)
                                    {
                                        if (handState == HandState.Waiting && hand != -1)
                                            handState = HandState.Charging;
                                        handState2 = HandState.Waiting;
                                    }
                                    else
                                    {
                                        handState2 = HandState.Waiting;
                                        projectile2.Kill();
                                    }
                                }
                                else
                                {
                                    handState2 = HandState.Waiting;
                                    projectile2.Kill();
                                }

                            }
                            break;
                        case HandState.ForcedKill:
                            handState2 = HandState.Waiting;
                            projectile2.Kill();
                            Timer2 = 0;
                            handSling2 = 0f;
                            handCharge2 = 0f;
                            handFollowThrough2 = 0f;
                            projectile2.Center = Vector2.Lerp(projectile2.Center, normalCenter, 0.3f);
                            break;
                        case HandState.StarShard:
                            if (Timer2++ >= 100)
                            {
                                Timer2 = 0;
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Excalibur,new ParticleOrchestraSettings { PositionInWorld = projectile2.Center }, projectile2.owner);
                                    Vector2 vel = projectile2.DirectionTo(player.Center) * 12f;
                                    if (Main.expertMode || Main.masterMode)
                                    {
                                        vel = projectile2.DirectionTo(player.Center + player.velocity * 20f) * 12f;
                                    }
                                    int projID = Projectile.NewProjectile(NPC.GetSource_FromThis(), projectile2.Center, vel,
                                     ProjectileID.FallingStar, NPC.damage, 0f, -1, 240, NPC.whoAmI);
                                    Main.projectile[projID].friendly = false;
                                    Main.projectile[projID].hostile = true;
                                    Main.projectile[projID].tileCollide = false;

                                }

                            }
                            projectile2.Center = Vector2.Lerp(projectile2.Center, normalCenter, 0.3f);

                            break;
                    }
                    projectile2.rotation = MathHelper.Lerp(projectile2.rotation, targetRotation, 0.3f);
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        Projectile other = Main.projectile[i];

                        if (i != projectile2.whoAmI && other.active && other.type == ProjectileID.CopperShortswordStab && Math.Abs(projectile2.position.X - other.position.X) + Math.Abs(projectile2.position.Y - other.position.Y) < projectile2.width)
                        {
                            if (Timer2 == 0 && handState2 == HandState.Slinging && handFollowThrough2 < 1f &&
                                player.Distance(projectile2.Center) < 60f
                                )
                            {
                                ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Excalibur,
                                    new ParticleOrchestraSettings { PositionInWorld = other.Center },projectile2.owner);
                                SoundEngine.PlaySound(new SoundStyle("ITD/Content/Sounds/UltraParry"), projectile2.Center);
                                player.GetITDPlayer().Screenshake = 20;
                                    handState2 = HandState.DownToSize;
                                
                                if (NPC.life > NPC.lifeMax / 10)
                                {
                                    NPC.life -= NPC.lifeMax / 10;
                                }
                                else CheckDead();
                                CombatText.NewText(projectile2.Hitbox, Color.Violet, "DOWN TO SIZE", true);
                                projectile2.velocity = -projectile2.velocity * 2;
                            }
                        }
                    }
                }
                else
                {
                    hand2 = -1;
                }
            }
        }
        #endregion
        //a fuck you when despawn is epic
        private void Movement(Player player)//___________________________________________________________________________________________________________________________________________________
        {
            float distanceAbove = 250f;//True melee
            Vector2 toPlayer = player.Center - NPC.Center;
            Vector2 toPlayerNormalized = Vector2.Normalize(toPlayer);
            Vector2 abovePlayer = toPlayer + new Vector2(0f, -distanceAbove);
            Vector2 aboveNormalized = Vector2.Normalize(abovePlayer);
            Vector2 dashvel;
            float speed = abovePlayer.Length()/1.3f;//True melee
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
                        NPC.velocity *= 0.9f;
                        NPC.netUpdate = true;
                    }
                    //very hard coded
                    if (NPC.localAI[2] == 10)//set where to dash
                    {
                        dashvel = Vector2.Normalize(new Vector2(player.Center.X, player.Center.Y) - new Vector2(NPC.Center.X, NPC.Center.Y));
                        NPC.velocity = dashvel;
                        NPC.netUpdate = true;
                    }

                    if (NPC.localAI[2] > 10 && NPC.localAI[2] < 25)//xcel
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            NPC.netUpdate = true;

                            NPC.velocity *= 1.18f;
                            NPC.netUpdate = true;
                        }
                    }
                    if (NPC.localAI[2] > 45) //Decelerate 
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            NPC.netUpdate = true;

                            NPC.velocity *= 0.96f;
                            NPC.netUpdate = true;

                        }
                    }
                    if (NPC.localAI[2] >= 80)
                    {
                        NPC.localAI[0]++;
                        NPC.localAI[1] = 0;
                        NPC.localAI[2] = 0;
                        AI_State = MovementState.FollowingRegular;
                    }
                    break;
                case MovementState.Suffocate:
                    var suffer = player.GetModPlayer<ITDPlayer>();
                    NPC.velocity *= 0.6f;
                    player.velocity *= 0;
                    player.Center = NPC.Center;
                    player.AddBuff(BuffID.Suffocation, 5);
                    if (!suffer.CosJellSuffocated)
                    {
                        NPC.localAI[1] = 0;
                        NPC.ai[3] ++;
                        AI_State = MovementState.FollowingRegular;

                    }
                    break;

                case MovementState.Explode:
                    NPC.velocity *= 0.9f;
                    break;

            }
            float maxRotation = MathHelper.Pi / 6;
            float rotationFactor = MathHelper.Clamp(NPC.velocity.X / 8f, -1f, 1f);

            rotation = rotationFactor * maxRotation;
            NPC.rotation = rotation;
        }
        public override bool CheckDead()
        {
            if (!bOkuu)//Subterranean Sun
            {
                NPC.ai[3] =-1;
                NPC.localAI[0] = 0;
                NPC.localAI[1] = 0;
                NPC.localAI[2] = 0;
                NPC.life = NPC.lifeMax;
                NPC.dontTakeDamage = true;
                NPC.netUpdate = true;
                bOkuu = true;
                if (hand != -1)
                {
                    handState = HandState.ForcedKill;
                }
                if (hand2 != -1)
                {
                    handState2 = HandState.ForcedKill;
                }
                AI_State = MovementState.Explode;
                Main.NewText("The world around you is crumbling.", Color.Violet);
                return false;

            }
            return true;
        }
        public override void DrawBehind(int index)
        {
            Main.instance.DrawCacheProjsBehindNPCs.Add(index);
            Main.instance.DrawCacheProjsOverPlayers.Add(index);

        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (AI_State == MovementState.Ram)
            {
                Texture2D texture = TextureAssets.Npc[Type].Value;
                Vector2 drawOrigin = texture.Size() / 2f;
                for (int k = 0; k < NPC.oldPos.Length; k++)
                {
                    Vector2 drawPos = NPC.oldPos[k] - screenPos + new Vector2(NPC.width * 0.5f, NPC.height * 0.5f) + new Vector2(0f, NPC.gfxOffY + 4f);
                    Color color = drawColor * ((NPC.oldPos.Length - k) / (float)NPC.oldPos.Length);
                    SpriteEffects effects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                    spriteBatch.Draw(texture, drawPos, null, color, 0f, drawOrigin, NPC.scale, effects, 0);
                }
            }
        spriteBatch.Draw(spriteBack.Value, NPC.Center - screenPos - new Vector2(0f, 46f), NPC.frame, Color.White, rotation, new Vector2(spriteBack.Width() / 2f, spriteBack.Height() / Main.npcFrameCount[NPC.type] / 2f), 1f, SpriteEffects.None, default);
            if (hand != -1)
            {
                Projectile projectile = Main.projectile[hand];
                Texture2D tex = TextureAssets.Projectile[projectile.type].Value;
                int vertSize = tex.Height / Main.projFrames[projectile.type];
                Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f / Main.projFrames[projectile.type]);
                Rectangle frameRect = new Rectangle(0, vertSize * projectile.frame, tex.Width, vertSize);
                if (handState == HandState.Slinging)
                {
                    for (int k = 0; k < projectile.oldPos.Length; k++)
                    {
                        Vector2 center = projectile.Size / 2f;
                        Vector2 drawPos = projectile.oldPos[k] - Main.screenPosition + center;
                        Color color = projectile.GetAlpha(drawColor) * ((projectile.oldPos.Length - k) / (float)projectile.oldPos.Length);
                        spriteBatch.Draw(tex, drawPos, frameRect, color, projectile.oldRot[k], origin, projectile.scale, SpriteEffects.None, 0f);
                    }
                }
                spriteBatch.Draw(tex, projectile.Center - screenPos, frameRect, Color.White, projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0f);
            }
            if (hand2 != -1)
            {
                Projectile projectile = Main.projectile[hand2];
                Texture2D tex = TextureAssets.Projectile[projectile.type].Value;
                int vertSize = tex.Height / Main.projFrames[projectile.type];
                Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f / Main.projFrames[projectile.type]);
                Rectangle frameRect = new Rectangle(0, vertSize * projectile.frame, tex.Width, vertSize);
                if (handState2 == HandState.Slinging)
                {
                    for (int k = 0; k < projectile.oldPos.Length; k++)
                    {
                        Vector2 center = projectile.Size / 2f;
                        Vector2 drawPos = projectile.oldPos[k] - Main.screenPosition + center;
                        Color color = projectile.GetAlpha(drawColor) * ((projectile.oldPos.Length - k) / (float)projectile.oldPos.Length);
                        spriteBatch.Draw(tex, drawPos, frameRect, color, projectile.oldRot[k], origin, projectile.scale, SpriteEffects.FlipHorizontally, 0f);
                    }
                }
                spriteBatch.Draw(tex, projectile.Center - screenPos, frameRect, Color.White, projectile.rotation, origin, projectile.scale, SpriteEffects.FlipHorizontally, 0f);
            }
            return true;
        }
    }
}
