/*
using ITD.Content.Items.Other;
using ITD.Content.Items.PetSummons;
using ITD.Content.Items.Placeable;
using ITD.Content.Projectiles.Friendly.Misc;
using ITD.Content.Projectiles.Hostile;
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

namespace ITD.Content.NPCs.Bosses

{
    [AutoloadBossHead]
    public class CosJelOld : ModNPC
    {
        public int hand = -1;
        public int hand2 = -1;
        public CosmicJellyfish_Hand RightHand
        {
            get
            {
                if (hand > -1)
                {
                    Projectile proj = Main.projectile[hand];
                    if (!proj.active)
                        return null;
                    return proj.ModProjectile as CosmicJellyfish_Hand;
                }
                return null;
            }
        }
        public CosmicJellyfish_Hand LeftHand
        {
            get
            {
                if (hand2 > -1)
                {
                    Projectile proj = Main.projectile[hand2];
                    if (!proj.active)
                        return null;
                    return proj.ModProjectile as CosmicJellyfish_Hand;
                }
                return null;
            }
        }

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

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            cooldownSlot = ImmunityCooldownID.Bosses;
            if (AI_State == MovementState.Suffocate)
            {
                return false;
            }
            else return true;
        }

        public override void FindFrame(int frameHeight)
        {
            int startFrame = 0;
            int finalFrame = 4;
            if (bSecondStage())
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
                if (!bSecondStage())
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
        private void NetSync()
        {
            NPC.netUpdate = true;
            if (RightHand != null)
                RightHand.Projectile.netUpdate = true;
            if (LeftHand != null)
                LeftHand.Projectile.netUpdate = true;
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
                NPC.velocity.Y -= 0.04f;
                NPC.EncourageDespawn(10);
                return;
            }
            if (bSecondStage())
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
            if (!SkyManager.Instance["ITD:CosjelOkuuSky"].IsActive() && bOkuu)
            {
                SkyManager.Instance.Activate("ITD:CosjelOkuuSky");
            }
            Attacks(player);
            Movement(player);
            HandControl(player);
            iDelayTime = masterMode ? 0 : expertMode ? 4 : 6;
        }
        public bool bSecondStage()
        {
            if (Main.expertMode && NPC.life < NPC.lifeMax * (2f / 3))
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.localAI[0] = 0;
                    NPC.localAI[1] = 0;
                    NPC.localAI[2] = 0;
                    NPC.ai[0] = 0;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = 0;
                    NPC.ai[3] = 0;
                    NPC.netUpdate = true;
                }
                return true;
            }
            return false;
        }
        private void CreateLeftHand(int AttackID)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;
            hand2 = Projectile.NewProjectile(
                NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<CosmicJellyfish_Hand>(), 20, 0.1f, -1, AttackID, NPC.whoAmI
                );
            (Main.projectile[hand2].ModProjectile as CosmicJellyfish_Hand).isLeftHand = true;
            NetSync();
        }
        private void CreateRightHand(int AttackID)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;
            hand = Projectile.NewProjectile(
                NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<CosmicJellyfish_Hand>(), 20, 0.1f, -1, AttackID, NPC.whoAmI
                );
            (Main.projectile[hand].ModProjectile as CosmicJellyfish_Hand).isLeftHand = false;
            NetSync();
        }
        //TODO: FIX CODE FOR MULTIPLAYER
        private void Attacks(Player player)//___________________________________________________________________________________________________________________________________________________
        {
            Vector2 toPlayer = player.Center - NPC.Center;
            Vector2 toPlayerNormalized = Vector2.Normalize(toPlayer);
            switch (NPC.ai[3])
            {

                case 0:
                    if (NPC.localAI[1]++ >= 120)//take time to get to the player
                    {
                        NPC.ai[3]++;
                        NPC.localAI[1] = 0;
                    }
                    break;
                case 1:
                    NPC.localAI[1]++;
                    if (NPC.localAI[1] == 50)
                    {
                        int projectileAmount = Main.rand.Next(5, 8);

                        if (bSecondStage())
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
                    else if (NPC.localAI[1] == 150 || NPC.localAI[1] == 100 && bSecondStage())
                    {
                        NPC.ai[3]++;
                        NPC.localAI[1] = 0;
                    }
                    break;
                case 2:
                    //Tempo
                    if (NPC.localAI[1]++ >= 200 || expertMode && NPC.localAI[1]++ >= 180 || masterMode && NPC.localAI[1]++ >= 150)
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
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, projectileVelo * 0.01f, ModContent.ProjectileType<CosmicVoidShard>(), 20, 5, -1,0,2);
                               
                            }
                        }
                    }
                    if (NPC.localAI[2]++ >= 400 + Main.rand.Next(-100, 150))
                    {
                        NPC.ai[3]++;

                        NPC.localAI[1] = 0;

                        NPC.localAI[2] = 0;
                    }
                    break;
                case 3:
                    if (NPC.localAI[1]++ == 60)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)//Fix later, this will do for now
                        {
                            if (!bSecondStage())
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
                        NetSync();
                    }
                    break;
                case 4:
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        if (!bSecondStage())
                        {
                            if (NPC.localAI[1]++ >= 100)
                            {
                                NPC.ai[3]++;
                                NPC.localAI[1] = 0;
                                if (LeftHand != null && LeftHand.HandState == CosJelHandState.Waiting)
                                    LeftHand.HandState = CosJelHandState.Charging;
                                if (RightHand != null && RightHand.HandState == CosJelHandState.Waiting)
                                    RightHand.HandState = CosJelHandState.Charging;
                            }
                            if (RightHand is null && LeftHand is null)
                            {
                                AIRand = Main.rand.Next(2);
                                NetSync();
                                bool b = AIRand == 0;
                                if (b)
                                {
                                    if (hand == -1)
                                    {
                                        CreateRightHand(0);
                                    }
                                }
                                else
                                {
                                    if (hand2 == -1)
                                    {
                                        CreateLeftHand(0);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (NPC.localAI[1]++ == 100)
                            {
                                if (RightHand != null && RightHand.HandState == CosJelHandState.Waiting)
                                    RightHand.HandState = CosJelHandState.Charging;
                            }

                            if (attackCount >= 6)
                            {
                                NPC.localAI[1] = 0;
                                NPC.localAI[2] = 0;
                                attackCount = 0;
                                NPC.ai[3]++;
                                TryKillBothHands();
                                NetSync();
                            }
                            else
                            {
                                if (hand == -1)
                                {
                                    CreateRightHand(0);
                                }
                                if (hand2 == -1)
                                {
                                    CreateLeftHand(0);
                                }
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
                    if (NPC.localAI[0] >= 2 && !bSecondStage() || NPC.localAI[0] >= 3 && bSecondStage() || NPC.localAI[1] >= 400)
                    {
                        //can't believe i have to do this, since the checking doesn't even fucking work
                        NPC.ai[3]++;
                        NPC.localAI[1] = 0;
                        NPC.localAI[0] = 0;
                        NPC.localAI[2] = 0;
                        NPC.ai[1] = 0;

                        NPC.ai[2] = 0;


                    }
                    break;
                case 6:
                    bool leftHandNotNullAndWaiting = LeftHand != null && LeftHand.HandState == CosJelHandState.Waiting;
                    bool rightHandNotNullAndWaiting = RightHand != null && RightHand.HandState == CosJelHandState.Waiting;
                    if (NPC.localAI[1]++ >= 100)
                    {
                        NPC.localAI[1] = 0;
                        if (NPC.Center.X < player.Center.X)
                        {
                            if (leftHandNotNullAndWaiting)
                                LeftHand.HandState = CosJelHandState.Charging;
                        }
                        else
                        {
                            if (rightHandNotNullAndWaiting)
                                RightHand.HandState = CosJelHandState.Charging;
                        }
                    }
                    AIRand = Main.rand.Next(100, 200);
                    NetSync();
                    if (NPC.localAI[2]++ >= 600 )
                    {
                        if (rightHandNotNullAndWaiting && leftHandNotNullAndWaiting)
                        {
                            RightHand.HandState = CosJelHandState.Charging;
                            LeftHand.HandState = CosJelHandState.Charging;
                        }

                        *//*                        NPC.ai[3]++;
*/
/*                        NPC.localAI[0] = 0;

                        NPC.localAI[1] = 0;

                        NPC.localAI[2] = 0;
                        TryKillBothHands();*//*
                        NetSync();
                    }
                    else
                    {
                        if (NPC.localAI[2] >= 50)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                if (hand == -1)
                                {
                                    CreateRightHand(1);
                                }
                                if (hand2 == -1)
                                {
                                    CreateLeftHand(1);
                                }
                            }
                        }
                    }
                        break;
                    
                case 7:
                    BlackholeDusting();
                    AI_State = MovementState.Explode;
                    NPC.localAI[1]++;
                    if (NPC.localAI[0] >= 30)
                    {
                        if (NPC.localAI[2]++ == 150)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    Vector2 vel = NPC.DirectionTo(player.Center) * 1f; ;
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, vel,
                                     ModContent.ProjectileType<CosmicRayWarn>(), NPC.damage, 0f, -1, 300, NPC.whoAmI);
                                }
                            }
                        }
                        if (NPC.localAI[2] >= 800)
                        {
                            AI_State = MovementState.FollowingRegular;
                            NPC.ai[3] = 0;
                            NPC.localAI[1] = 0;
                            NPC.localAI[2] = 0;
                            NPC.localAI[0] = 0;
                            NetSync();
                        }
                        if (NPC.localAI[1] >= 100)
                        {
                            NPC.localAI[1] = 0;
                            SoundEngine.PlaySound(SoundID.Item20, NPC.Center);
                            //P2 stat garbage here
                            AIRand = Main.rand.Next(20, 24);
                            NetSync();
                            int projectileAmount = (int)AIRand;
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
                    else
                    {
                        if (NPC.localAI[1] >= 8)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                NPC.localAI[0]++;
                                AIRand = Main.rand.Next(600, 800);
                                NetSync();
                                int saferange = (int)AIRand;
                                float offset = NPC.localAI[0] > 0 && player.velocity != Vector2.Zero
                                    ? Main.rand.NextFloat((float)Math.PI * 2) : player.velocity.ToRotation();
                                float rotation = offset + (float)Math.PI * 2 / Main.rand.Next(10);
                                int proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + saferange * Vector2.UnitX.RotatedBy(rotation), Vector2.Zero,
                                    ModContent.ProjectileType<TouhouBullet>(), 30, 0f, -1, NPC.whoAmI, 2);
                                ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Excalibur,
                                    new ParticleOrchestraSettings { PositionInWorld = Main.projectile[proj].Center }, Main.projectile[proj].owner);
                                NPC.localAI[1] = 0;
                            }
                        }
                    }
                    break;
                case -1:
                    player.GetITDPlayer().BetterScreenshake(20, 5, 5, true);

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
                                    for (int i = 0; i < 2; i++)
                                    {
                                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + 600 *
                                        Vector2.UnitX.RotatedBy(NPC.localAI[1] + Math.PI / 2 * i), Vector2.Zero, ModContent.ProjectileType<TouhouBullet>(),
                                        25, 0f, -1, NPC.whoAmI);
                                    }
                                }

                            }
                            float Range = 3000;
                            float Power = 0.125f + 1.5f * 0.125f;
                            for (int i = 0; i < Main.maxPlayers; i++)
                            {
                                float Distance = Vector2.Distance(Main.player[i].Center, NPC.Center);
                                if (Distance < 500 && Main.player[i].grappling[0] == -1)
                                {
                                    if (Collision.CanHit(NPC.Center, 1, 1, Main.player[i].Center, 1, 1))
                                    {
                                        float distanceRatio = Distance / Range;
                                        float multiplier = distanceRatio;

                                        if (Main.player[i].Center.X < NPC.Center.X)
                                            Main.player[i].velocity.X += Power * multiplier;
                                        else
                                            Main.player[i].velocity.X -= Power * multiplier;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile Blast = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero,
                    ModContent.ProjectileType<CosmicLightningBlast>(), (int)(NPC.damage), 2f, player.whoAmI);
                            Blast.damage = 0;
                            Blast.ai[1] = 1000f;
                            Blast.localAI[1] = Main.rand.NextFloat(0.18f, 0.3f);
                            Blast.netUpdate = true;
                        }
                        NPC.scale *= 1.01f;
                        if (NPC.scale <= 1.5f)
                        {
                            NPC.life = 0;
                            NPC.HitEffect(0, 0);
                            NPC.checkDead();
                        }
                    }
                    break;

            }
        }
        private void TryKillBothHands(CosJelHandState? targetState = null)
        {
            if (RightHand != null)
            {
                if (targetState is null)
                {
                    RightHand.Projectile.Kill();
                }
                else if (RightHand.HandState == targetState)
                {
                    RightHand.Projectile.Kill();
                }
            }
            if (LeftHand != null)
            {
                if (targetState is null)
                {
                    LeftHand.Projectile.Kill();
                }
                else if (LeftHand.HandState == targetState)
                {
                    LeftHand.Projectile.Kill();
                }
            }
        }
        //Kind of hardcoded but it's fine
        //Will customize more if needed
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
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {

            var suffer = target.GetModPlayer<ITDPlayer>();
            if (NPC.ai[3] == 5)//Suffocate, actual retarded attack but i'm not the boss here
            {
                AI_State = MovementState.Suffocate;
                if (!suffer.CosJellSuffocated)
                {
                    suffer.CosJellSuffocated = true;
                    suffer.CosJellEscapeCurrent = masterMode ? 12 : expertMode ? 8 : 6;
                }
            }
            else
            {
                suffer.CosJellEscapeCurrent = 0;
                suffer.CosJellSuffocated = false;
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
        //Can't kill hand directly for some reasons
        //When this is called, kill both hands
        //hand - right
        //hand2 - left
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
                NetSync();
                bOkuu = true;
                TryKillBothHands();
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(), ModContent.ProjectileType<CosmicJellyfishBlackholeAura>(), 0, 0, -1, NPC.whoAmI);
                }
                AI_State = MovementState.Explode;
                Main.NewText("Slop slop slop slop.", Color.Violet);
                return false;

            }
            return true;
        }
        #region HandRigamagic

        bool expertMode = Main.expertMode;
        bool masterMode = Main.masterMode;
        public int iDelayTime;
        public Vector2 vLockedIn;

        private void HandControl(Player player)
        {
            if (!player.Exists())
            {
                TryKillBothHands();
                NetSync();
            }
            if (RightHand == null)
            {
                hand = -1;
            }
            if (LeftHand == null)
            {
                hand2 = -1;
            }
        }

        #endregion


        public override void DrawBehind(int index)
        {
            if (bOkuu)
            {
                Main.instance.DrawCacheNPCsOverPlayers.Add(index);
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.ai[3] == 5)
            {
                //Rgb effect like the hand later
                *//*                Texture2D outline = ModContent.Request<Texture2D>(Texture + "_Outline").Value;
                *//*
                Texture2D tex = TextureAssets.Npc[NPC.type].Value;
                int vertSize = tex.Height / Main.npcFrameCount[NPC.type];
                Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f / Main.npcFrameCount[NPC.type]);
                Rectangle frameRect = new Rectangle(0, NPC.frame.Y, tex.Width, vertSize);
                for (int k = 0; k < NPC.oldPos.Length; k++)
                {
                    Vector2 center = NPC.Size / 2f;
                    Vector2 drawPos = NPC.oldPos[k] - Main.screenPosition + center;
                    Color color = NPC.GetAlpha(drawColor) * ((NPC.oldPos.Length - k) / (float)NPC.oldPos.Length);
                    spriteBatch.Draw(tex, drawPos, frameRect, color, NPC.oldRot[k], origin, NPC.scale, SpriteEffects.None, 0f);
                }
            }
            return true;
        }
    }
}
*/