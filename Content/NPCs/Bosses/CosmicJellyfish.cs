﻿using ITD.Content.Items.Other;
using ITD.Content.Items.PetSummons;
using ITD.Content.Items.Placeable;
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
using ITD.Content.Items.Accessories.Movement.Boots;
using ITD.Content.Projectiles.Hostile.CosjelTest;
using ITD.PrimitiveDrawing;
using Terraria.Graphics.Shaders;
using ITD.Content.Items.Placeable.Furniture.Relics;
using ITD.Content.Items.Placeable.Furniture.Trophies;


namespace ITD.Content.NPCs.Bosses

{
    [AutoloadBossHead]
    public class CosmicJellyfish : ModNPC
    {
        
        public float rotation = 0f;
        public float AIRand = 0f;
        public bool bOkuu;
        int goodtransition;//Add to current frame for clean tentacles
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
            writer.Write(NPC.localAI[1]);
            writer.Write(NPC.localAI[2]);
            writer.Write(AIRand);

        }
        public ref float AITimer1 => ref NPC.ai[1];
        public ref float AITimer2 => ref NPC.ai[2];
        public ref float AttackID => ref NPC.ai[3];
        public ref float AttackCount => ref NPC.ai[0];
        public ref float AttackTotal => ref NPC.localAI[0];
        public ref float DashTimer => ref NPC.localAI[1];
        public ref float AITimer3 => ref NPC.localAI[2];



        public override void ReceiveExtraAI(BinaryReader reader)
        {
            bOkuu = reader.ReadBoolean();
            goodtransition = reader.ReadInt32();
            AttackCount = reader.ReadSingle();
            NPC.localAI[0] = reader.ReadSingle();
            NPC.localAI[1] = reader.ReadSingle();
            NPC.localAI[2] = reader.ReadSingle();
            AIRand = reader.ReadSingle();
        }
        private enum MovementState
        {
            FollowingRegular,
            FollowingSlow,
            Inbetween,
            Dashing,
            Suffocate,
            Explode,
            Slamdown
        }
        private MovementState AI_State = MovementState.FollowingRegular;

        public override void SetDefaults()
        {
            NPC.width = 180;
            NPC.height = 252;
            NPC.damage = 15;
            NPC.defense = 5;
            NPC.lifeMax = 3500;
            NPC.HitSound = SoundID.NPCHit25;
            NPC.DeathSound = SoundID.DD2_DefeatScene;
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
                Music = ITD.Instance.GetMusic("CosmicJellyfish") ?? MusicID.Boss1;
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
        public bool bSecondStage => NPC.localAI[2] != 0;//ai 2 for cosjel p2

        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest();
            }
            ITDGlobalNPC.cosjelBoss = NPC.whoAmI;
            Player player = Main.player[NPC.target];
            Movement(player);
            Vector2 toPlayer = player.Center - NPC.Center;
            Vector2 toPlayerNormalized = Vector2.Normalize(toPlayer);
            if (player.dead || !player.active || Main.IsItDay())
            {
                NPC.velocity.Y -= 0.04f;
                NPC.EncourageDespawn(10);
                return;
            }
            if (!bOkuu)
            {
                CheckSecondStage();
            }
/*            Main.NewText(AITimer2, Color.Violet);
*/
            if (!SkyManager.Instance["ITD:CosjelOkuuSky"].IsActive() && bOkuu)
            {
                SkyManager.Instance.Activate("ITD:CosjelOkuuSky");
            }
            switch ((int)AttackID)
            {
                case -2:
                    player.GetITDPlayer().BetterScreenshake(20, 5, 5, true);

                    if (++AITimer1 <= 900)
                    {
                        if (Main.expertMode || Main.masterMode)
                        {
                            if (++NPC.ai[0] > 16)
                            {
                                NPC.ai[0] = 0;
                                NPC.localAI[1] += (float)Math.PI / 2 / 360 * 75;
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    for (int i = 0; i < 1; i++)
                                    {
                                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + 600 *
                                        Vector2.UnitX.RotatedBy(NPC.localAI[1] + Math.PI / 2 * i), Vector2.Zero, ModContent.ProjectileType<TouhouBullet>(),
                                        25, 0f, -1, NPC.whoAmI);
                                    }
                                }

                            }
                            float Range = 1000;
                            float Power = 0.125f + 2f * 0.125f;
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
                            Blast.localAI[1] = 1000f;
                            Blast.ai[1] = Main.rand.NextFloat(0.18f, 0.3f);
                            Blast.netUpdate = true;
                        }
                        NPC.scale *= 1.1f;
                        if (NPC.scale <= 1.5f)
                        {
                            NPC.life = 0;
                            NPC.HitEffect(0, 0);
                            NPC.checkDead();
                        }
                    }
                    break;

                case -1://p2 transition
                    BlackholeDusting(1);
                    NPC.dontTakeDamage = true;
                    if (AITimer1++ >= 300)
                    {
                        AI_State = MovementState.FollowingRegular;
                        AttackID = Main.rand.Next(1, 4);//randomized, but not reveal new attack now
                        AITimer1 = 0;
                        NPC.dontTakeDamage = false;
                    }
                    break;
                case 0://Free
                    distanceAbove = 700;
                    if (AITimer1++ == 120)
                    {
                        AI_State = MovementState.Slamdown;
                    }
                    if (AttackCount > 0)
                    {
                        Main.NewText(AttackCount);
                        AI_State = MovementState.FollowingRegular;
                        AttackID = 1;
                        AITimer1 = 0;
                        AITimer2 = 0;
                        AttackCount = 0;
                        distanceAbove = 250;
                    }
                    break;
                case 1: //Dashstard
                    if (AITimer1++ == 80)
                    {
                        SoundEngine.PlaySound(SoundID.Zombie101, NPC.Center);
                        dashPos = player.Center;
                        if (AI_State != MovementState.Suffocate)
                            AI_State = MovementState.Dashing;
                    }
                    if (AttackCount > 4)
                    {
                        AI_State = MovementState.FollowingRegular;
                        distanceAbove = 250;
                        AITimer1 = 0;
                        AITimer2 = 0;
                        AttackCount = 0;
                        DashTimer = 0;
                        AttackTotal++;
                        AttackID++;
                    }
                    break;
                case 2: //Leaperbomb reference???
                    distanceAbove = 700;
                    if (AI_State != MovementState.Slamdown)
                    {
                        AITimer1++;
                        if (AITimer1 <= 30)
                        {
                            Dash(dashPos, 1, 20, 50, 100, 2);
                        }
                        else if (AITimer1 > 30 && AITimer1 < 120)
                        {
                            AI_State = MovementState.FollowingRegular;

                        }
                        else if (AITimer1 == 120)
                        {
                            AITimer1 = 0;
                            AI_State = MovementState.Slamdown;
                        }
                    }
                    if (AttackCount > 4)
                    {
                        AI_State = MovementState.FollowingRegular;
                        AttackID = Main.rand.Next(1, 4);
                        AITimer1 = 0;
                        AITimer2 = 0;
                        AttackCount = 0;
                        DashTimer = 0;
                        AttackTotal++;
                        distanceAbove = 250;

                    }
                    break;
                case 3://shit enemy spawn
                    if (AITimer2++ == 200)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)//Fix later, this will do for now
                        {
                            for (int i = 0; i <= 1; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(player.Center.X + 500, player.Center.Y + 400 * Main.rand.Next(-1, 2)), Vector2.Zero, ModContent.ProjectileType<CosmicWarning>(), NPC.damage, 0f, -1, NPC.whoAmI);
                            }
                            AITimer2 = 0;
                        }
                    }
                    else if (AITimer1++ >= 100)
                    {
                        AITimer1 = 0;
                        AttackID++;
                        NetSync();
                    }
                    break;
                case 4://slap
                    AITimer1++;
                    if (!bSecondStage)
                    {
                        if (AITimer1 == 20)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                if (Main.rand.NextBool(2))
                                {
                                    NPCHelpers.NewNPCEasy(NPC.GetSource_FromAI(), NPC.Center, ModContent.NPCType<CosmicJellyfishHand>(), NPC.whoAmI, 0, 0, NPC.whoAmI, 1);
                                }
                                else NPCHelpers.NewNPCEasy(NPC.GetSource_FromAI(), NPC.Center, ModContent.NPCType<CosmicJellyfishHand>(), NPC.whoAmI, 0, 0, NPC.whoAmI, -1);
                            }
                        }
                        else if (AITimer1 >= 100)
                        {
                            AttackID++;
                            AITimer1 = 0;
                            HandControl(-1, 1, 2, false);
                            HandControl(1, 1, 2, false);
                        }
                    }
                    else
                    {

                        if (AITimer1 == 100)
                        {
                            if (NPC.Center.X < player.Center.X)
                            {
                                HandControl(-1, 1, 2, false);
                            }
                            else
                            {
                                HandControl(1, 1, 2, false);
                            }
                        }
                        if (AITimer1 >= 600)
                        {
                            HandControl(1, 6, 3, true);
                            HandControl(-1, 6, 3, true);
                            NetSync();
                            AITimer1 = 0;
                            AttackID++;
                            NetSync();

                        }
                        if (AITimer1 == 20)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                if (!HandExist(1))
                                    NPCHelpers.NewNPCEasy(NPC.GetSource_FromAI(), NPC.Center, ModContent.NPCType<CosmicJellyfishHand>(), NPC.whoAmI, 0, 0, NPC.whoAmI, 1);
                                if (!HandExist(-1))
                                    NPCHelpers.NewNPCEasy(NPC.GetSource_FromAI(), NPC.Center, ModContent.NPCType<CosmicJellyfishHand>(), NPC.whoAmI, 0, 0, NPC.whoAmI, -1);
                            }
                        }
                    }



                        break;
                case 5://dash
                    if (AITimer1++ == 60)
                    {
                        AITimer2 = 0;
                        SoundEngine.PlaySound(SoundID.Zombie101, NPC.Center);
                        if (AI_State != MovementState.Suffocate)
                            AI_State = MovementState.Dashing;
                    }
                    break;
                case 6:

                    if (AITimer1++ == 100)
                    {
                        AITimer1 = 0;

                        if (NPC.Center.X < player.Center.X)
                        {
                            HandControl(-1, 1, 4, false);
                        }
                        else
                        {
                            HandControl(1, 1, 4, false);
                        }
                    }

                    if (AITimer2++ >= 600)
                    {
                        AITimer1 = 0;
                        AITimer2 = 0;
                        AttackID++;

                        NetSync();
                        //ForceKill returns
                        HandControl(1, 6, 3, true);
                        HandControl(-1, 6, 3, true);

                    }
                    if (AITimer1 == 20)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            if (!HandExist(1))
                                NPCHelpers.NewNPCEasy(NPC.GetSource_FromAI(), NPC.Center, ModContent.NPCType<CosmicJellyfishHand>(), NPC.whoAmI, 0, 0, NPC.whoAmI, 1);
                            if (!HandExist(-1))
                                NPCHelpers.NewNPCEasy(NPC.GetSource_FromAI(), NPC.Center, ModContent.NPCType<CosmicJellyfishHand>(), NPC.whoAmI, 0, 0, NPC.whoAmI, -1);
                        }
                    }
                    break;

                case 7:
                    BlackholeDusting(3);
                    AI_State = MovementState.Explode;
                    AITimer1++;
                    if (AttackCount >= 30)
                    {
                        if (AITimer2++ == 60)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Vector2 vel = NPC.DirectionTo(player.Center) * 1f; ;
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, vel,
                                 ModContent.ProjectileType<CosmicRayWarn>(), NPC.damage, 0f, -1, 100, NPC.whoAmI);
                            }
                        }       
                        if (AITimer2 >= 800)
                        {
                            AI_State = MovementState.FollowingRegular;
                            AttackID = 0;
                            AITimer1 = 0;
                            AITimer2 = 0;
                            NetSync();
                        }
                        if (AITimer1 >= 100)
                        {
                            AITimer1 = 0;
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
                        if (AITimer1 >= 8)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                AIRand = Main.rand.Next(600, 800);
                                NetSync();
                                int saferange = (int)AIRand;
                                float offset = AttackCount > 0 && player.velocity != Vector2.Zero
                                    ? Main.rand.NextFloat((float)Math.PI * 2) : player.velocity.ToRotation();
                                float rotation = offset + (float)Math.PI * 2 / Main.rand.Next(10);
                                int proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + saferange * Vector2.UnitX.RotatedBy(rotation), Vector2.Zero,
                                    ModContent.ProjectileType<TouhouBullet>(), 30, 0f, -1, NPC.whoAmI, 2);
                                ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Excalibur,
                                    new ParticleOrchestraSettings { PositionInWorld = Main.projectile[proj].Center }, Main.projectile[proj].owner);
                                AITimer1 = 0;
                            }
                        }
                    }
                    break;
            }
        }
        float distanceAbove = 250f;//True melee
        private void ShardSlam()
        {
            float XVeloDifference = 2;
            float startXVelo = -((float)(6) / 2) * (float)XVeloDifference;
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < 8; i++)
                {
                    Vector2 projectileVelo = new Vector2(startXVelo + XVeloDifference * i, -5f);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, projectileVelo, ModContent.ProjectileType<CosmicSludgeBomb>(), 30, 0, -1, NPC.whoAmI);
                }
            }
        }
            private void Movement(Player player)//___________________________________________________________________________________________________________________________________________________
        {
            Vector2 toPlayer = player.Center - NPC.Center;
            Vector2 toPlayerNormalized = Vector2.Normalize(toPlayer);
            Vector2 abovePlayer = toPlayer + new Vector2(0f, -distanceAbove);
            Vector2 aboveNormalized = Vector2.Normalize(abovePlayer);
            float speed = abovePlayer.Length() / 1.2f;//True melee
            float maxRotation = MathHelper.Pi / 6;
            float rotationFactor = MathHelper.Clamp(NPC.velocity.X / 8f, -1f, 1f);
            switch (AI_State)
            {
                case MovementState.FollowingRegular:

                    if (speed > 1.1f)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            NPC.velocity = aboveNormalized * (speed + 1f) / 20;
                            NetSync();
                        }
                    }
                    else
                    {
                        NPC.velocity = Vector2.Zero;
                    }
                    rotation = rotationFactor * maxRotation;
                    NPC.rotation = rotation;

                    break;
                case MovementState.FollowingSlow:
                    if (speed > 1.1f)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            NPC.velocity = aboveNormalized * (speed) / 24;
                            NetSync();
                        }
                    }
                    else
                    {
                        NPC.velocity = Vector2.Zero;
                    }
                    NPC.rotation = 0;

                    break;
                case MovementState.Dashing:

                    AITimer1++;
                    if (AITimer1 < 10)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            NPC.velocity *= 0.9f;
                            NetSync();
                        }
                        dashPos = player.Center;
                    }
                    else
                    {
                        Dash(dashPos, 10, 30, 80, 100, 1);
                    }
                    NPC.rotation = NPC.rotation.AngleTowards(NPC.velocity.ToRotation() + MathHelper.PiOver2, 0.2f);
                    NetSync();
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
                        AITimer1 = 0;
                        AI_State = MovementState.FollowingRegular;

                    }
                    break;

                case MovementState.Explode:
                    NPC.velocity *= 0.9f;
                    break;
                case MovementState.Slamdown:
                    RaycastData data = Helpers.QuickRaycast(NPC.Center, NPC.velocity, (point) => { return (player.Center.Y >= point.ToWorldCoordinates().Y + 20); }, 1000);
                    if (NPC.Center.Distance(data.End) >= 20)
                    {
                        if (AITimer1++ >= 5)
                        NPC.velocity.Y += 0.5f;
                        else
                        {
                            NPC.velocity.X *= 0.95f;
                        }
                    }
                    else
                    {
                        NPC.velocity *= 0;
                        if (AITimer2++ == 1)
                        {
                            ShardSlam();
                            player.GetITDPlayer().BetterScreenshake(30, 10, 20, true);//Very shaky, might need some tweaking to the decay
                        }
                        if (AITimer2 >= 50)
                        {
                            dashVel = Vector2.Normalize(new Vector2(NPC.Center.X, NPC.Center.Y - 750) - new Vector2(NPC.Center.X, NPC.Center.Y)) * 20;
                            NPC.velocity = dashVel;
                            AttackCount++;
                            AITimer1 = 0;
                            AITimer2 = 0;
                            AI_State = MovementState.Inbetween;
                        }
                    }
                    NPC.rotation = 0;

                    break;
            }
        }
        bool IsDashing;
        Vector2 dashVel;
        private Vector2 dashPos = Vector2.Zero;

        public void Dash(Vector2 pos, int time1, int time2, int time3, int reset, int attackID)
        {
            Player player = Main.player[NPC.target];
            if (DashTimer == time1)
            {
                dashVel = Vector2.Normalize(new Vector2(pos.X, pos.Y) - new Vector2(NPC.Center.X, NPC.Center.Y));
                NPC.velocity = dashVel;
                NPC.netUpdate = true;
            }
            DashTimer++;
            if (attackID == 1)
            {
                if (DashTimer % 10 == 0 && DashTimer > time1 && DashTimer < reset)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Projectile proj1 = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, Vector2.Normalize(NPC.velocity).RotatedBy(Math.PI / 2) * 4, ModContent.ProjectileType<CosmicVoidShard>(), (NPC.defDamage), 0, Main.myPlayer);
                        Projectile proj2 = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, Vector2.Normalize(NPC.velocity).RotatedBy(-Math.PI / 2) * 4, ModContent.ProjectileType<CosmicVoidShard>(), (NPC.defDamage), 0, Main.myPlayer);
                        proj1.tileCollide = false;
                        proj2.tileCollide = false;

                    }
                }
            }
            if (DashTimer > time1 && DashTimer < time2)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.velocity *= 1.18f;
                    NPC.netUpdate = true;
                }
            }
            if (DashTimer > time3)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.velocity *= 0.96f;
                    NPC.netUpdate = true;

                }
            }
            if (DashTimer >= reset)
            {
                AITimer1 = 0;
                if (attackID != 2)
                {
                    AITimer2 = 0;
                }
                DashTimer = 0;
                if (attackID == 1)
                {
                    AttackCount++;
                }
                NetSync();
            }

        }
        private void CheckSecondStage()
        {
            if (bSecondStage)
            {
                if (AttackID >= 8)
                {
                    AttackID = 1;
                }
            }
            else
            {
                if (AttackID >= 6)
                {
                    AttackID = 1;
                }
            }
            if (NPC.life * 100 / NPC.lifeMax < 50 && !bSecondStage)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                }
                if (Main.netMode != NetmodeID.Server)
                {
                }
                AITimer1 = 0;
                AITimer2 = 0;
                NPC.localAI[2] = 1;
                HandControl(1, 6, 3, true);
                HandControl(-1, 6, 3, true);
                AttackID = -1;
                AI_State = MovementState.Explode;
                NetSync();
                
            }
            return;
        }
        public override bool CheckDead()
        {
            if (!bOkuu)//Subterranean Sun
            {
                AttackID = -2;
                AITimer1 = 0;
                AITimer2 = 0;
                AttackCount = 0;
                NPC.life = NPC.lifeMax;
                NPC.dontTakeDamage = true;
                HandControl(1, 6, 3, true);
                HandControl(-1, 6, 3, true);
                NetSync();
                bOkuu = true;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(), ModContent.ProjectileType<CosmicJellyfishBlackholeAura>(), 0, 0, -1, NPC.whoAmI);
                }
                AI_State = MovementState.Explode;
                return false;

            }
            return true;
        }
        //[3]: 1 is left
        //[2]: 0: wait, 1: charge, 2: sling

        //AttackID useless for now, change to this attack
        //UpcomingID is the next attack to go to from charging
        private void HandControl(int whichHand, int attackID, int upcomingID, bool IsForceKill)
        {
            for (int i = 0; i < Main.maxNPCs; i++) //find hands, update
            {
                if ((Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<CosmicJellyfishHand>()
                    && Main.npc[i].ai[2] == NPC.whoAmI
                    && Main.npc[i].ai[3] == whichHand//so that force kill work?
                    && Main.npc[i].ai[0] == 0) || IsForceKill)//waiting ai state
                {
                    Main.npc[i].ai[0] = attackID;
                    Main.npc[i].ai[1] = upcomingID;
                    Main.npc[i].localAI[0] = 0;
                    Main.npc[i].localAI[1] = 0;
                    Main.npc[i].netUpdate = true;
                }
            }
        }
        private bool HandExist(int whichHand)
        {
            for (int i = 0; i < Main.maxNPCs; i++) //find hands, update
            {
                if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<CosmicJellyfishHand>()
                    && Main.npc[i].ai[3] == whichHand)
                {
                    return true;
                }               
            }
            return false;
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
                BlackholeDusting(4);
            }
            return true;
        }
        private void BlackholeDusting(int ring)
        {
            int dustRings = ring;
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
        public override void DrawBehind(int index)
        {
            if (bOkuu && AttackID != -2)
            {
                Main.instance.DrawCacheNPCsOverPlayers.Add(index);
            }

        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Vector2 stretch = new Vector2(1f, 1f);

            if(AI_State == MovementState.Dashing)
                stretch = new Vector2(1f, 1f + NPC.velocity.Length() * 0.025f);

            if (AI_State == MovementState.Slamdown)
            {
                if (NPC.velocity.Y != 0)
                {
                    stretch = new Vector2(1f, 1f + NPC.velocity.Length() * 0.02f);
                }
                else
                {
                    stretch = new Vector2(1f + NPC.oldVelocity.Length() * 0.025f, 1f - NPC.oldVelocity.Length() * 0.01f);
                }
            }

            if (AttackID == 5 || AI_State == MovementState.Slamdown)
            {
                Texture2D tex = TextureAssets.Npc[NPC.type].Value;
                int vertSize = tex.Height / Main.npcFrameCount[NPC.type];
                Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f / Main.npcFrameCount[NPC.type]);
                Rectangle frameRect = new Rectangle(0, NPC.frame.Y, tex.Width, vertSize);
                for (int k = 0; k < NPC.oldPos.Length; k++)
                {
                    Vector2 center = NPC.Size / 2f;
                    Vector2 drawPos = NPC.oldPos[k] - Main.screenPosition + center;
                    Color color = NPC.GetAlpha(drawColor) * ((NPC.oldPos.Length - k) / (float)NPC.oldPos.Length);
                    spriteBatch.Draw(tex, drawPos, frameRect, color, NPC.oldRot[k], origin, 1f, SpriteEffects.None, 0f);
                }
            }

            if(AttackID == -2)
            {
                default(BlackholeVertex).Draw(NPC.Center - Main.screenPosition, 1024);

            }



            Main.EntitySpriteDraw(TextureAssets.Npc[Type].Value, NPC.Center - Main.screenPosition, NPC.frame, Color.White, NPC.rotation, NPC.frame.Size() / 2f, stretch, SpriteEffects.None);



            return false;
        }
    }
    public struct BlackholeVertex
    {

        private static SimpleSquare square = new SimpleSquare();

        public void Draw(Vector2 position, float size)
        {
            GameShaders.Misc["Blackhole"].UseImage0(TextureAssets.Extra[193]);
            GameShaders.Misc["Blackhole"].UseColor(new Color(192, 59, 166));
            GameShaders.Misc["Blackhole"].UseSecondaryColor(Color.Beige);
            GameShaders.Misc["Blackhole"].Apply();
            square.Draw(position, size: new Vector2(size, size));
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

    }

    public struct CosmicTelegraphVertex 
    {

        private static SimpleSquare square = new SimpleSquare();

        public void Draw(Vector2 position, Vector2 size, float rotation)
        {
            GameShaders.Misc["Telegraph"].UseColor(new Color(192, 59, 166));
            GameShaders.Misc["Telegraph"].UseSecondaryColor(Color.Beige);
            GameShaders.Misc["Telegraph"].UseImage0(TextureAssets.Extra[193]);
            GameShaders.Misc["Telegraph"].UseShaderSpecificData(new Vector4(300,0,position.X,position.Y));

            GameShaders.Misc["Telegraph"].Apply();
            square.Draw(position + rotation.ToRotationVector2() * (size.X * 0.5f), Color.White, size * new Vector2(1, 1f), rotation, position + rotation.ToRotationVector2() * size.X / 2f);
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

        }



    }
}