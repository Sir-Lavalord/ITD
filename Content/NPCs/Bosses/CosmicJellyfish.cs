using ITD.Content.Items.Other;
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
using ITD.Content.Dusts;

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
        public float rotation = 0f;
        private Vector2 CorePos;
        //Hand rigamagig will be controlled seperately from the boss
        public bool bSecondStage;
        public bool bOkuu;
        int goodtransition;//Add to current frame for clean tentacles

        public override void SetStaticDefaults()
        {
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            Main.npcFrameCount[NPC.type] = 11;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(bSecondStage);
            writer.Write(bOkuu);
            writer.Write(goodtransition);
            writer.Write(NPC.localAI[0]);
            writer.Write(NPC.localAI[1]);
            writer.Write(NPC.localAI[2]);

        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            bSecondStage = reader.ReadBoolean();
            bOkuu = reader.ReadBoolean();
            goodtransition = reader.ReadInt32();
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
            NPC.lifeMax = (int)(NPC.lifeMax * 0.8f * balance * bossAdjustment);

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
        public override void AI()
        {
            CorePos = new Vector2(NPC.Center.X, NPC.Center.Y - 100);
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
            if (!bOkuu)
            {
                CheckbSecondStage();
            }
            Attacks(player);
            Movement(player);
            CheckHand();
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
                        ForceKillHand();
                    }
                    NPC.netUpdate = true;

                }
            }
            return;

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
                    //Tempo
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
                    if (NPC.localAI[2]++ >= 200 + Main.rand.Next(50, 100))//Convoluted
                    {
                        NPC.localAI[2] = 0;
                        NPC.ai[3]++;
                        ForceKillHand();
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
                                ForceKillHand();
                                NPC.ai[3]++;
                                NPC.localAI[1] = 0;
                                /*                                if (handState2 == HandState.Waiting && hand2 != -1)
                                                                    handState2 = HandState.Charging;*/
                            }
                            if (NPC.localAI[1] == 1)
                            {
                                if (hand == -1 && hand2 == -1)
                                {
                                    if (Main.rand.NextBool(2))
                                    {
                                        if (hand == -1)
                                        {
                                            hand = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<CosmicJellyfish_RightHand>(), 1, 0.1f, -1, NPC.whoAmI, 60, 2);
                                        }
                                    }
                                    else
                                    {
                                        if (hand2 == -1)
                                        {
                                            hand2 = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<CosmicJellyfish_LeftHand>(), 1, 0.1f, -1, NPC.whoAmI, 60, 2);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (NPC.localAI[1]++ >= 400)
                            {
                                ForceKillHand();
                                NPC.ai[3]++;
                                NPC.localAI[1] = 0;
                                NPC.ai[1] = 0;
                                NPC.ai[2] = 0;

                            }
                            else
                            {
                                if (hand == -1)
                                {
                                    hand = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<CosmicJellyfish_RightHand>(), 1, 0.1f, -1, NPC.whoAmI, 0, 2);
                                }
                                if (hand2 == -1)
                                {
                                    hand2 = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<CosmicJellyfish_LeftHand>(), 1, 0.1f, -1, NPC.whoAmI, 0, 3);
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
                    if (NPC.localAI[0] >= 2 && !bSecondStage || NPC.localAI[0] >= 3 && bSecondStage)
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
                    if (NPC.localAI[1]++ >= 600)
                    {
                        ForceKillHand();
                        NPC.ai[3]++;
                        NPC.localAI[1] = 0;
                        NPC.ai[1] = 0;
                        NPC.ai[2] = 0;

                    }
                    else
                    {
                        if (NPC.localAI[1] == 1)
                        {
                            NPC.ai[1] = 1;
                        }
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            if (hand == -1)
                            {
                                hand = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<CosmicJellyfish_RightHand>(), 1, 0.1f, -1, NPC.whoAmI, 0, 4);
                            }
                            if (hand2 == -1)
                            {
                                hand2 = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<CosmicJellyfish_LeftHand>(), 1, 0.1f, -1, NPC.whoAmI, 0, 4);
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
                        if (NPC.localAI[2]++ == 60)
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
                        if (NPC.localAI[2] == 600)
                        {
                            AI_State = MovementState.FollowingRegular;
                            NPC.ai[3] = 0;
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
                    else
                    {
                        if (NPC.localAI[1] == 8)
                        {

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                int saferange = Main.rand.Next(600, 800);
                                int max = 1;
                                float offset = NPC.localAI[0] > 0 && player.velocity != Vector2.Zero
                                    ? Main.rand.NextFloat((float)Math.PI * 2) : player.velocity.ToRotation();
                                float rotation = offset + (float)Math.PI * 2 / Main.rand.Next(10);
                                int proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + saferange * Vector2.UnitX.RotatedBy(rotation), Vector2.Zero,
                                    ModContent.ProjectileType<TouhouBullet>(), NPC.damage, 0f, -1, NPC.whoAmI, 2);
                                ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Excalibur,
                                    new ParticleOrchestraSettings { PositionInWorld = Main.projectile[proj].Center }, Main.projectile[proj].owner);


                                NPC.localAI[1] = 0;
                            }
                        }
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
                                        Projectile.NewProjectile(NPC.GetSource_FromThis(), CorePos + 600 *
                                        Vector2.UnitX.RotatedBy(NPC.localAI[1] + Math.PI / 2 * i), Vector2.Zero, ModContent.ProjectileType<TouhouBullet>(),
                                        NPC.damage, 0f, -1, NPC.whoAmI);
                                    }
                                }

                            }
                            float Range = 2000f;
                            float Power = 0.125f + 1.5f * 0.125f;
                            for (int i = 0; i < Main.maxPlayers; i++)
                            {
                                float Distance = Vector2.Distance(Main.player[i].Center, CorePos);
                                if (Distance < 1000f && Main.player[i].grappling[0] == -1)
                                {
                                    if (Collision.CanHit(CorePos, 1, 1, Main.player[i].Center, 1, 1))
                                    {
                                        float distanceRatio = Distance / Range;
                                        float multiplier = 1f - distanceRatio;

                                        if (Main.player[i].Center.X < CorePos.X)
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
                            Projectile Blast = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), CorePos, Vector2.Zero,
                    ModContent.ProjectileType<CosmicLightningBlast>(), (int)(NPC.damage), 2f, player.whoAmI);
                            Blast.ai[1] = 1000f;
                            Blast.localAI[1] = Main.rand.NextFloat(0.18f, 0.3f);
                            Blast.netUpdate = true; ;
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
                        int dust = Dust.NewDust(CorePos, 1, 1, ModContent.DustType<CosJelDust>());
                        Main.dust[dust].position = CorePos + dustOffset;
                        Main.dust[dust].fadeIn = 1f;
                        Main.dust[dust].velocity = Vector2.Normalize(CorePos - Main.dust[dust].position) * dustVelocity;
                        Main.dust[dust].scale = 2f - h;
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
                    if (Main.masterMode || Main.expertMode)//Annoying asdf
                    {
                        suffer.CosJellEscapeCurrent = 10;
                    }
                    else suffer.CosJellEscapeCurrent = 6;
                }
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

                    if (NPC.localAI[2] > 10 && NPC.localAI[2] < 30)//xcel
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            NPC.netUpdate = true;

                            NPC.velocity *= 1.18f;
                            NPC.netUpdate = true;
                        }
                    }
                    if (NPC.localAI[2] > 50) //Decelerate 
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
                        NPC.ai[3]++;
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
        //Can't kill hand directly for some reasons
        //When this is called, kill both hands
        //hand - right
        //hand2 - left
        private void CheckHand()//Check if hand is dead or alive
        {
            if (hand != -1)
            {
                Projectile projectile = Main.projectile[hand];
                if (projectile.active && projectile.type == ModContent.ProjectileType<CosmicJellyfish_RightHand>())
                {
                }
                else hand = -1;
            }
            if (hand2 != -1)
            {
                Projectile projectile2 = Main.projectile[hand2];
                if (projectile2.active && projectile2.type == ModContent.ProjectileType<CosmicJellyfish_LeftHand>())
                {
                }
                else hand2 = -1;
            }
        }
        private void ForceKillHand()
        {
            NPC.ai[1] = 0;

            NPC.ai[2] = 0;
            if (hand != -1)
            {
                Projectile projectile = Main.projectile[hand];
                if (projectile.active && projectile.type == ModContent.ProjectileType<CosmicJellyfish_RightHand>())
                {
                    hand = -1;
                    projectile.Kill();
                }
                else
                {
                    hand = -1;
                }
            }
            if (hand2 != -1)
            {
                Projectile projectile2 = Main.projectile[hand2];
                if (projectile2.active && projectile2.type == ModContent.ProjectileType<CosmicJellyfish_LeftHand>())
                {
                    hand = -1;
                    projectile2.Kill();
                }
                else
                {
                    hand2 = -1;
                }
            }
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
                ForceKillHand();
                AI_State = MovementState.Explode;
                Main.NewText("The world around you is crumbling.", Color.Violet);
                return false;

            }
            return true;
        }
        public override void DrawBehind(int index)
        {
            if (bOkuu)
            {
                Main.instance.DrawCacheNPCsOverPlayers.Add(index);
            }
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
        spriteBatch.Draw(spriteBack.Value, NPC.Center - screenPos - new Vector2(0f, 46f), NPC.frame, Color.White, rotation, new Vector2(spriteBack.Width() / 2f, spriteBack.Height() / (Main.npcFrameCount[NPC.type] -1) / 2f), 1f, SpriteEffects.None, default);
            return true;
        }
    }
}
